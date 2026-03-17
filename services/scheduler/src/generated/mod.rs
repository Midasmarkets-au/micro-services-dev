pub mod api {
    pub mod v1 {
        include!(concat!(env!("CARGO_MANIFEST_DIR"), "/src/generated/api.v1.rs"));
    }
}
