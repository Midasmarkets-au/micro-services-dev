//! Boardcast Service 入口：gRPC（Tonic）与 HTTP/SSE（Axum）并行运行。
//!
//! gRPC 端口：`GRPC_ADDR`（默认 `[::]:50052`）
//! HTTP 端口：`HTTP_ADDR`（默认 `[::]:8081`）
//!
//! SSE 端点：`GET /event?channel=<name>`
//! 发布端点：`POST /publish` body `{"channel":"...","message":"..."}`

use std::net::SocketAddr;
use std::pin::Pin;
use std::time::Duration;

use axum::{
    Json, Router,
    extract::{Query, State},
    response::sse::{Event as SseEvent, KeepAlive, Sse},
    routing::{get, post},
};
use chrono::Utc;
use serde::Deserialize;
use tokio_stream::wrappers::BroadcastStream;
use tokio_stream::{Stream, StreamExt};
use tonic::{Request, Response, Status};
use tower_http::cors::CorsLayer;
use tracing::info;

use boardcast::api::v1::boardcast_service_server::{BoardcastService, BoardcastServiceServer};
use boardcast::api::v1::{PublishRequest, PublishResponse, SubscribeRequest, SubscribeResponse};
use boardcast::BroadcastBus;

const FILE_DESCRIPTOR_SET: &[u8] =
    include_bytes!(concat!(env!("OUT_DIR"), "/boardcast_descriptor.bin"));

// ---------- gRPC 实现 ----------

#[derive(Clone)]
pub struct BoardcastServiceImpl {
    bus: BroadcastBus,
}

impl BoardcastServiceImpl {
    pub fn new(bus: BroadcastBus) -> Self {
        Self { bus }
    }
}

#[tonic::async_trait]
impl BoardcastService for BoardcastServiceImpl {
    async fn publish(
        &self,
        request: Request<PublishRequest>,
    ) -> Result<Response<PublishResponse>, Status> {
        let req = request.into_inner();
        if req.channel.is_empty() {
            return Err(Status::invalid_argument("channel must not be empty"));
        }
        self.bus.publish(&req.channel, req.message);
        Ok(Response::new(PublishResponse { ok: true }))
    }

    type SubscribeStream =
        Pin<Box<dyn Stream<Item = Result<SubscribeResponse, Status>> + Send + 'static>>;

    async fn subscribe(
        &self,
        request: Request<SubscribeRequest>,
    ) -> Result<Response<Self::SubscribeStream>, Status> {
        let channel = request.into_inner().channel;
        if channel.is_empty() {
            return Err(Status::invalid_argument("channel must not be empty"));
        }
        let rx = self.bus.subscribe(&channel);
        let stream = BroadcastStream::new(rx).filter_map(move |msg| {
            let ch = channel.clone();
            match msg {
                Ok(message) => Some(Ok(SubscribeResponse {
                    channel: ch,
                    message,
                    timestamp: Utc::now().timestamp(),
                })),
                Err(_) => None,
            }
        });
        Ok(Response::new(Box::pin(stream)))
    }
}

// ---------- HTTP / SSE ----------

#[derive(Clone)]
struct AppState {
    bus: BroadcastBus,
}

#[derive(Deserialize)]
struct SseParams {
    channel: String,
}

async fn sse_handler(
    State(state): State<AppState>,
    Query(params): Query<SseParams>,
) -> Sse<impl Stream<Item = Result<SseEvent, std::convert::Infallible>>> {
    let rx = state.bus.subscribe(&params.channel);
    let stream = BroadcastStream::new(rx).filter_map(|msg| match msg {
        Ok(data) => {
            let event = SseEvent::default().data(data);
            Some(Ok(event))
        }
        Err(_) => None,
    });
    Sse::new(stream).keep_alive(KeepAlive::new().interval(Duration::from_secs(15)))
}

#[derive(Deserialize)]
struct PublishBody {
    channel: String,
    message: String,
}

async fn publish_handler(
    State(state): State<AppState>,
    Json(body): Json<PublishBody>,
) -> Json<serde_json::Value> {
    if body.channel.is_empty() {
        return Json(serde_json::json!({ "ok": false, "error": "channel must not be empty" }));
    }
    let receivers = state.bus.publish(&body.channel, body.message);
    Json(serde_json::json!({ "ok": true, "receivers": receivers }))
}

fn http_app(bus: BroadcastBus) -> Router {
    let state = AppState { bus };
    Router::new()
        .route("/event", get(sse_handler))
        .route("/publish", post(publish_handler))
        .with_state(state)
        .layer(CorsLayer::permissive())
}

// ---------- main ----------

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let _tracing_guard = otel::init_tracing("boardcast");

    let grpc_addr: SocketAddr = std::env::var("GRPC_ADDR")
        .unwrap_or_else(|_| "[::]:50003".to_string())
        .parse()?;
    let http_addr: SocketAddr = std::env::var("HTTP_ADDR")
        .unwrap_or_else(|_| "[::]:9003".to_string())
        .parse()?;

    let bus = BroadcastBus::new();

    let grpc_service = BoardcastServiceImpl::new(bus.clone());
    let reflection = tonic_reflection::server::Builder::configure()
        .register_encoded_file_descriptor_set(FILE_DESCRIPTOR_SET)
        .build()?;

    let grpc = tonic::transport::Server::builder()
        .add_service(reflection)
        .add_service(BoardcastServiceServer::new(grpc_service))
        .serve(grpc_addr);

    let listener = tokio::net::TcpListener::bind(http_addr).await?;
    let http = axum::serve(listener, http_app(bus));

    info!("gRPC 服务监听 {}", grpc_addr);
    info!("HTTP/SSE 服务监听 http://{}", http_addr);
    info!("  SSE 订阅: GET  http://{}/event?channel=<name>", http_addr);
    info!("  发布消息: POST http://{}/publish  {{\"channel\":\"...\",\"message\":\"...\"}}", http_addr);

    tokio::select! {
        r = grpc => r?,
        r = http => r?,
    }

    Ok(())
}
