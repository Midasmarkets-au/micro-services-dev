fn workspace_root() -> std::path::PathBuf {
    let manifest_dir = std::env::var("CARGO_MANIFEST_DIR").unwrap();
    let mut dir = std::path::Path::new(&manifest_dir).to_path_buf();
    loop {
        if dir.join("Cargo.toml").exists() {
            let content = std::fs::read_to_string(dir.join("Cargo.toml")).unwrap_or_default();
            if content.contains("[workspace]") {
                return dir;
            }
        }
        dir = dir.parent().expect("workspace root not found").to_path_buf();
    }
}

fn main() -> Result<(), Box<dyn std::error::Error>> {
    let ws_root = workspace_root();
    let proto_root = ws_root.join("proto");
    let out_dir = std::env::var("OUT_DIR").unwrap();
    let descriptor_path = std::path::Path::new(&out_dir).join("reflection_descriptor.bin");

    let generated_dir = std::path::Path::new(&std::env::var("CARGO_MANIFEST_DIR").unwrap())
        .join("src")
        .join("generated");
    std::fs::create_dir_all(&generated_dir)?;

    tonic_build::configure()
        .build_server(true)
        .build_client(true)
        .out_dir(&generated_dir)
        .file_descriptor_set_path(&descriptor_path)
        .compile(
            &[
                proto_root.join("api/v1/scheduler.proto"),
                proto_root.join("api/v1/service.proto"),
            ],
            std::slice::from_ref(&proto_root),
        )?;

    println!("cargo:rerun-if-changed=../../proto/api/v1/scheduler.proto");
    println!("cargo:rerun-if-changed=../../proto/api/v1/service.proto");
    Ok(())
}
