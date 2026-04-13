fn workspace_root() -> Result<std::path::PathBuf, Box<dyn std::error::Error>> {
    let manifest_dir = std::env::var("CARGO_MANIFEST_DIR")?;
    let mut dir = std::path::Path::new(&manifest_dir).to_path_buf();
    loop {
        let cargo_toml = dir.join("Cargo.toml");
        if cargo_toml.exists() {
            if let Ok(content) = std::fs::read_to_string(&cargo_toml) {
                if content.contains("[workspace]") {
                    return Ok(dir);
                }
            }
        }
        dir = match dir.parent() {
            Some(p) => p.to_path_buf(),
            None => return Err("workspace root not found".into()),
        };
    }
}

fn main() -> Result<(), Box<dyn std::error::Error>> {
    let ws_root = workspace_root()?;
    let proto_root = ws_root.join("proto");
    let manifest_dir = std::env::var("CARGO_MANIFEST_DIR")?;
    let generated_dir = std::path::Path::new(&manifest_dir).join("src").join("generated");
    std::fs::create_dir_all(&generated_dir)?;

    let proto_file = proto_root.join("api/v1/auth.proto");

    tonic_build::configure()
        .build_server(true)
        .build_client(true)
        .out_dir(&generated_dir)
        .compile(&[&proto_file], &[&proto_root])?;

    // Also compile http/v1/auth.proto for message type definitions (API contract)
    let http_proto_file = proto_root.join("http/v1/auth.proto");
    tonic_build::configure()
        .build_server(false)
        .build_client(false)
        .out_dir(&generated_dir)
        .compile(&[&http_proto_file], &[&proto_root])?;

    println!("cargo:rerun-if-changed={}", proto_file.display());
    println!("cargo:rerun-if-changed={}", http_proto_file.display());
    Ok(())
}
