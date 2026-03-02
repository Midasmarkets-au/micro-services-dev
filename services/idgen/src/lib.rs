//! 由 proto 生成的 gRPC/HTTP 接口（Rust）
//!
//! 通过 `cargo build` 时 build.rs 从 `proto/` 生成代码到 `src/generated/`，在此引入。
//! 业务逻辑在 `service` 中统一实现，gRPC 与 HTTP 共用。

pub mod api {
    pub mod v1 {
        include!("generated/api.v1.rs");
    }
}

/// 由 build.rs 根据 proto 中 google.api.http 生成，HTTP 路由以此为准。
pub mod http_routes {
    include!("generated/http_routes.rs");
}

pub use api::v1::*;

/// 与传输无关的业务逻辑：gRPC 与 HTTP 共用同一套实现。
pub mod service {
    use snowflaked::Generator;

    use crate::api::v1::{
        GenerateIdRequest, GenerateIdResponse, HealthCheckRequest, HealthCheckResponse,
        HelloRequest, HelloResponse, health_check_response::Status as HealthStatus,
    };

    /// 问候逻辑：根据 name 生成 message。
    pub fn say_hello(req: HelloRequest) -> HelloResponse {
        let message = if req.name.is_empty() {
            "Hello14, World!".to_string()
        } else {
            format!("Hello13, {}!", req.name)
        };
        HelloResponse { message }
    }

    /// 健康检查逻辑。
    pub fn check(_req: HealthCheckRequest) -> HealthCheckResponse {
        HealthCheckResponse {
            status: HealthStatus::Serving.into(),
        }
    }

    /// SnowFlakeID generate
    pub fn generate_id(req: GenerateIdRequest) -> GenerateIdResponse {
        let mut generator = Generator::new(req.work_id as u16);
        GenerateIdResponse {
            id: generator.generate(),
        }
    }
}
