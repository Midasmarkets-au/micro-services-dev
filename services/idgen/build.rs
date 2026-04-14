/// 从 CARGO_MANIFEST_DIR 向上查找包含 [workspace] 的 Cargo.toml，返回 workspace 根目录。
fn workspace_root() -> Result<std::path::PathBuf, Box<dyn std::error::Error>> {
    let manifest_dir = std::env::var("CARGO_MANIFEST_DIR")?;
    let mut dir = std::path::Path::new(&manifest_dir);
    loop {
        let cargo_toml = dir.join("Cargo.toml");
        if cargo_toml.exists()
            && let Ok(content) = std::fs::read_to_string(&cargo_toml)
            && content.contains("[workspace]")
        {
            return Ok(dir.to_path_buf());
        }
        dir = match dir.parent() {
            Some(p) => p,
            None => return Err("workspace root (Cargo.toml with [workspace]) not found".into()),
        };
    }
}

fn main() -> Result<(), Box<dyn std::error::Error>> {
    let proto_files = ["api/v1/hello.proto", "api/v1/service.proto"];
    let manifest_dir = std::env::var("CARGO_MANIFEST_DIR")?;
    let manifest_dir = std::path::Path::new(&manifest_dir);
    let ws_root = workspace_root()?;
    let generated_dir = manifest_dir.join("src").join("generated");
    let proto_dir_project = ws_root.join("proto");
    let out_dir_s = std::env::var("OUT_DIR")?;
    let out_dir = std::path::Path::new(&out_dir_s);
    let descriptor_path = out_dir.join("reflection_descriptor.bin");
    std::fs::create_dir_all(&generated_dir)?;

    if let Ok(repo) = std::env::var("PROTO_GITHUB_REPO") {
        // 每次从 GitHub 拉取 proto 到项目 proto/，覆盖本地
        let ref_name = std::env::var("PROTO_GITHUB_REF").unwrap_or_else(|_| "main".to_string());
        let github_prefix =
            std::env::var("PROTO_GITHUB_PATH").unwrap_or_else(|_| "proto".to_string());
        std::fs::create_dir_all(&proto_dir_project)?;

        let base_url = format!(
            "https://raw.githubusercontent.com/{}/{}/",
            repo.trim_end_matches('/'),
            ref_name
        );
        let client = reqwest::blocking::Client::builder()
            .user_agent("protobuf-api-build")
            .build()?;

        let fetch_prefix = if github_prefix.is_empty() {
            String::new()
        } else {
            format!("{}/", github_prefix.trim_end_matches('/'))
        };
        for path in &proto_files {
            let url = format!("{}{}{}", base_url, fetch_prefix, path);
            let resp = client.get(&url).send()?;
            if !resp.status().is_success() {
                return Err(format!("failed to fetch {}: {}", url, resp.status()).into());
            }
            let dest = proto_dir_project.join(path);
            if let Some(parent) = dest.parent() {
                std::fs::create_dir_all(parent)?;
            }
            std::fs::write(&dest, resp.text()?)?;
        }
    } else {
        // 未配置 repo 时：若本地 proto 缺失则写入默认内容
        ensure_proto_files(&proto_dir_project)?;
    }

    // 统一 proto 根：使用 workspace 根目录下的 proto/
    let proto_root = ws_root.join("proto");

    // service.proto 中 option (google.api.http) 未 import 会导致 protoc 失败；
    // 编译时使用去掉该 option 的副本，生成 .rs 到 src/generated/
    let (compile_paths, compile_includes) = prepare_protos_for_compile(&proto_root, out_dir)?;

    tonic_build::configure()
        .build_server(true)
        .build_client(true)
        .type_attribute(".", "#[derive(serde::Serialize, serde::Deserialize)]")
        .out_dir(&generated_dir)
        .file_descriptor_set_path(&descriptor_path)
        .compile(&compile_paths, &compile_includes)?;

    // 保证 descriptor 在 OUT_DIR，供 reflection API 使用（include 与 compile 一致）
    let protoc = std::env::var("PROTOC").unwrap_or_else(|_| "protoc".to_string());
    let mut protoc_cmd = std::process::Command::new(&protoc);
    protoc_cmd.arg("--descriptor_set_out").arg(&descriptor_path);
    for inc in &compile_includes {
        protoc_cmd.arg("--proto_path").arg(inc);
    }
    protoc_cmd.args(&compile_paths);
    let ok = protoc_cmd.status()?;
    if !ok.success() {
        return Err("protoc --descriptor_set_out failed".into());
    }

    // 从 proto 的 google.api.http 生成 HTTP 路由表（读原始 proto，含 option）
    generate_http_routes(&proto_root, &generated_dir)?;

    Ok(())
}

/// 若本地 proto 目录缺少文件，则从 PROTO_GITHUB_REPO 下载，否则写入默认 proto 到 proto/。
fn ensure_proto_files(proto_dir: &std::path::Path) -> Result<(), Box<dyn std::error::Error>> {
    let service_path = proto_dir.join("api/v1/service.proto");
    if service_path.exists() {
        return Ok(());
    }
    std::fs::create_dir_all(proto_dir.join("api/v1"))?;

    if let Ok(repo) = std::env::var("PROTO_GITHUB_REPO") {
        let ref_name = std::env::var("PROTO_GITHUB_REF").unwrap_or_else(|_| "main".to_string());
        let github_prefix =
            std::env::var("PROTO_GITHUB_PATH").unwrap_or_else(|_| "proto".to_string());
        let base_url = format!(
            "https://raw.githubusercontent.com/{}/{}/",
            repo.trim_end_matches('/'),
            ref_name
        );
        let client = reqwest::blocking::Client::builder()
            .user_agent("protobuf-api-build")
            .build()?;
        let fetch_prefix = if github_prefix.is_empty() {
            String::new()
        } else {
            format!("{}/", github_prefix.trim_end_matches('/'))
        };
        for path in &["api/v1/hello.proto", "api/v1/service.proto"] {
            let url = format!("{}{}{}", base_url, fetch_prefix, path);
            let resp = client.get(&url).send()?;
            if !resp.status().is_success() {
                return Err(format!("failed to fetch {}: {}", url, resp.status()).into());
            }
            let dest = proto_dir.join(path);
            if let Some(parent) = dest.parent() {
                std::fs::create_dir_all(parent)?;
            }
            std::fs::write(&dest, resp.text()?)?;
        }
        return Ok(());
    }

    Ok(())
}

/// 准备参与编译的 proto：若 service.proto 含 option (google.api.http)，则用去掉该 option 的副本，
/// 避免因未 import google/api/http.proto 导致 protoc 失败；返回 (要编译的文件列表, include 路径列表)。
fn prepare_protos_for_compile(
    proto_root: &std::path::Path,
    out_dir: &std::path::Path,
) -> Result<(Vec<std::path::PathBuf>, Vec<std::path::PathBuf>), Box<dyn std::error::Error>> {
    let hello_path = proto_root.join("api/v1/hello.proto");
    let service_path = proto_root.join("api/v1/service.proto");
    let content = std::fs::read_to_string(&service_path)
        .map_err(|e| format!("read {:?}: {}", service_path, e))?;

    if content.contains("google.api.http") {
        let stripped = strip_google_http_option(&content);
        let stripped = strip_google_api_imports(&stripped);
        let out_api = out_dir.join("api/v1");
        std::fs::create_dir_all(&out_api)?;
        let out_hello = out_api.join("hello.proto");
        let out_service = out_api.join("service.proto");
        std::fs::copy(&hello_path, &out_hello)?;
        std::fs::write(&out_service, stripped)?;
        let paths = vec![out_hello, out_service];
        let includes = vec![out_dir.to_path_buf()];
        Ok((paths, includes))
    } else {
        let paths = vec![hello_path, service_path];
        let includes = vec![proto_root.to_path_buf()];
        Ok((paths, includes))
    }
}

/// 去掉对 google/api 的 import 行，使编译副本不依赖 google/api 文件。
fn strip_google_api_imports(content: &str) -> String {
    content
        .lines()
        .filter(|line| {
            let t = line.trim();
            !(t.starts_with("import ") && t.contains("google/api"))
        })
        .map(|l| format!("{l}\n"))
        .collect()
}

/// 去掉 option (google.api.http) = { ... }; 块，使 protoc 在无 google/api 依赖下能通过。
fn strip_google_http_option(content: &str) -> String {
    let mut out = String::new();
    let mut skip = false;
    let mut brace: i32 = 0;
    for line in content.lines() {
        let trimmed = line.trim();
        if skip {
            if trimmed.starts_with('{') {
                brace += 1;
            }
            if trimmed.ends_with("};") {
                brace -= 1;
                if brace == 0 {
                    skip = false;
                }
            } else if trimmed.ends_with('}') && !trimmed.ends_with("};") {
                brace -= 1;
            }
            continue;
        }
        if trimmed.contains("option (google.api.http)") {
            if trimmed.contains('{') {
                skip = true;
                brace = 1;
                if trimmed.ends_with("};") {
                    brace = 0;
                    skip = false;
                }
            }
            continue;
        }
        out.push_str(line);
        out.push('\n');
    }
    out
}

/// 解析 service.proto 中 option (google.api.http) = { get: "path" }，生成 http_routes.rs
fn generate_http_routes(
    proto_root: &std::path::Path,
    generated_dir: &std::path::Path,
) -> Result<(), Box<dyn std::error::Error>> {
    let service_proto = proto_root.join("api/v1/service.proto");
    let content = std::fs::read_to_string(&service_proto)
        .map_err(|e| format!("read {:?}: {}", service_proto, e))?;

    let mut routes: Vec<(String, String)> = Vec::new();
    let mut current_rpc: Option<String> = None;

    for line in content.lines() {
        let trimmed = line.trim();
        if let Some(after_rpc) = trimmed.strip_prefix("rpc ") {
            let after_rpc = after_rpc.trim_start();
            if let Some(name) = after_rpc.split_whitespace().next() {
                let rpc_name = name.split('(').next().unwrap_or(name).trim();
                current_rpc = Some(rpc_name.to_string());
            }
        }
        if let Some(ref rpc) = current_rpc
            && trimmed.contains("get:")
            && let Some(q) = trimmed.find('"')
        {
            let start = q + 1;
            if let Some(end) = trimmed[start..].find('"') {
                let path = trimmed[start..start + end].to_string();
                routes.push((rpc.clone(), path));
                current_rpc = None;
            }
        }
    }

    let mut out = String::from(
        "// @generated by build.rs from proto option (google.api.http) — do not edit\n\n",
    );
    out.push_str("/// GET 路径与 RPC 方法名，由 proto 中 google.api.http 唯一决定。\n");
    out.push_str("pub const HTTP_GET_ROUTES: &[(&str, &str)] = &[\n");
    for (rpc, path) in &routes {
        out.push_str(&format!("    (\"{}\", \"{}\"),\n", path, rpc));
    }
    out.push_str("];\n");

    let out_path = generated_dir.join("http_routes.rs");
    std::fs::write(&out_path, out)?;

    Ok(())
}
