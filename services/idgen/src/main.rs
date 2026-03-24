//! gRPC 与 Axum HTTP 服务并行：实现 ApiService（gRPC），同时提供 HTTP API。

use axum::{Json, Router, extract::Query, routing::get};
use serde_json::Value;
use std::collections::HashMap;
use std::net::SocketAddr;
use tonic::{Request, Response, Status};
use tower_http::cors::CorsLayer;
use tracing::{debug, info};

use idgen::api::v1::api_service_server::{ApiService, ApiServiceServer};
use idgen::api::v1::{
    GenerateIdRequest, GenerateIdResponse, HealthCheckRequest, HealthCheckResponse, HelloRequest,
    HelloResponse,
};
use idgen::http_routes;
use idgen::service;

/// gRPC Reflection：供 grpcurl、Postman 等发现服务与消息类型（descriptor 由 build.rs 写入 OUT_DIR）
const FILE_DESCRIPTOR_SET: &[u8] =
    include_bytes!(concat!(env!("OUT_DIR"), "/reflection_descriptor.bin"));

// ---------- gRPC 实现 ----------

#[derive(Debug, Default)]
pub struct ApiServiceImpl;

#[tonic::async_trait]
impl ApiService for ApiServiceImpl {
    async fn say_hello(
        &self,
        request: Request<HelloRequest>,
    ) -> Result<Response<HelloResponse>, Status> {
        Ok(Response::new(service::say_hello(request.into_inner())))
    }

    async fn check(
        &self,
        request: Request<HealthCheckRequest>,
    ) -> Result<Response<HealthCheckResponse>, Status> {
        Ok(Response::new(service::check(request.into_inner())))
    }

    async fn generate_id(
        &self,
        request: Request<GenerateIdRequest>,
    ) -> Result<Response<GenerateIdResponse>, Status> {
        Ok(Response::new(service::generate_id(request.into_inner())))
    }
}

// ---------- HTTP (Axum)：路径来自 proto，按 RPC 分发 ----------

async fn dispatch_http_get(
    rpc: &'static str,
    Query(params): Query<HashMap<String, String>>,
) -> Json<Value> {
    let value = match rpc {
        "SayHello" => {
            let req = HelloRequest {
                name: params.get("name").cloned().unwrap_or_default(),
            };
            let res = service::say_hello(req);
            serde_json::json!({ "message": res.message })
        }
        "Check" => {
            let res = service::check(HealthCheckRequest {});
            let status_str = match res.status {
                1 => "SERVING",
                2 => "NOT_SERVING",
                _ => "UNKNOWN",
            };
            serde_json::json!({ "status": status_str })
        }
        "GenerateID" => {
            let work_id = params
                .get("workid")
                .and_then(|s| s.parse::<u32>().ok())
                .unwrap_or(0);
            let req = GenerateIdRequest { work_id };
            let res = service::generate_id(req);
            serde_json::json!({ "id": res.id })
        }
        _ => serde_json::json!({ "error": format!("unknown rpc : {}", rpc) }),
    };
    Json(value)
}

fn http_app() -> Router {
    let mut router = Router::new();
    for (path, rpc) in http_routes::HTTP_GET_ROUTES {
        debug!("http get : {}", path);
        router =
            router.route(
                path,
                get(move |q: Query<HashMap<String, String>>| async move {
                    dispatch_http_get(rpc, q).await
                }),
            );
    }
    router.layer(CorsLayer::permissive())
}

// ---------- main：gRPC 与 HTTP 并行 ----------

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let _tracing_guard = otel::init_tracing("idgen");

    let grpc_addr = std::env::var("GRPC_ADDR")
        .unwrap_or_else(|_| "[::]:50001".to_string())
        .parse()?;
    let http_addr: SocketAddr = std::env::var("HTTP_ADDR")
        .unwrap_or_else(|_| "[::]:8080".to_string())
        .parse()?;

    let service = ApiServiceImpl::default();
    let reflection = tonic_reflection::server::Builder::configure()
        .register_encoded_file_descriptor_set(FILE_DESCRIPTOR_SET)
        .build()?;

    let grpc = tonic::transport::Server::builder()
        .add_service(reflection)
        .add_service(ApiServiceServer::new(service))
        .serve(grpc_addr);

    let listener = tokio::net::TcpListener::bind(http_addr).await?;
    let http = axum::serve(listener, http_app());

    info!("gRPC 服务监听 {}", grpc_addr);
    info!("HTTP 服务监听 http://{}", http_addr);

    tokio::select! {
        r = grpc => r?,
        r = http => r?,
    }

    Ok(())
}
