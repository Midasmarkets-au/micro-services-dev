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
    let manifest_dir = std::env::var("CARGO_MANIFEST_DIR")?;
    let manifest_dir = std::path::Path::new(&manifest_dir);
    let ws_root = workspace_root()?;
    let proto_root = ws_root.join("proto");
    let generated_dir = manifest_dir.join("src").join("generated");
    let out_dir_s = std::env::var("OUT_DIR")?;
    let out_dir = std::path::Path::new(&out_dir_s);
    let descriptor_path = out_dir.join("boardcast_descriptor.bin");

    std::fs::create_dir_all(&generated_dir)?;

    let proto_file = proto_root.join("api/v1/boardcast.proto");

    tonic_build::configure()
        .build_server(true)
        .build_client(true)
        .out_dir(&generated_dir)
        .file_descriptor_set_path(&descriptor_path)
        .compile(&[proto_file], &[proto_root])?;

    println!("cargo:rerun-if-changed=../../proto/api/v1/boardcast.proto");

    Ok(())
}
