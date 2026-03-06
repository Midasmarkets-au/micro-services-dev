# Proto 定义

所有 Protobuf 定义集中在 `api/v1/`，由 Rust 服务（`build.rs`）和 .NET 服务（`Grpc.Tools`）共同引用。

## 文件说明

| 文件 | Package | 说明 |
|------|---------|------|
| `api/v1/service.proto` | `api.v1` | ApiService — gRPC + HTTP REST（SayHello、HealthCheck、GenerateID） |
| `api/v1/hello.proto` | `api.v1` | HelloRequest / HelloResponse 消息定义 |
| `api/v1/auth.proto` | `api.v1` | AuthService — 认证相关接口 |
| `api/v1/boardcast.proto` | `api.v1` | BoardcastService — 消息广播（Publish / Subscribe） |
| `google/api/annotations.proto` | `google.api` | HTTP 注解扩展 |
| `google/api/http.proto` | `google.api` | HTTP 规则消息 |
| `google/protobuf/descriptor.proto` | `google.protobuf` | 描述符，供注解扩展使用 |

## BoardcastService

```protobuf
service BoardcastService {
  // 向指定频道发布一条消息
  rpc Publish(PublishRequest) returns (PublishResponse);
  // 订阅指定频道，服务端以流式推送事件
  rpc Subscribe(SubscribeRequest) returns (stream Event);
}
```

| RPC | 说明 | 使用方 |
|-----|------|--------|
| `Publish` | 向频道推送消息，所有 SSE 订阅者实时收到 | mono `BoardcastController` |
| `Subscribe` | gRPC 服务端流式订阅，与 SSE 共享同一 BroadcastBus | 内部服务间调用 |

## 使用方

### Rust（自动生成）

`build.rs` 在 `cargo build` 时调用 `tonic_build` 从 proto 生成 Rust 代码到 `src/generated/`：

```bash
cargo build -p idgen      # 生成 ApiService 客户端 + 服务端
cargo build -p boardcast  # 生成 BoardcastService 客户端 + 服务端
```

### C#（Grpc.Tools）

在 `.csproj` 中声明引用，`dotnet build` 时自动生成：

```xml
<Protobuf Include="..\..\..\proto\api\v1\service.proto"   GrpcServices="Both"   />
<Protobuf Include="..\..\..\proto\api\v1\auth.proto"      GrpcServices="Server" />
<Protobuf Include="..\..\..\proto\api\v1\boardcast.proto" GrpcServices="Client" />
```

### 手动生成 C# 代码

```bash
protoc -I proto \
  --csharp_out=services/mono/proto \
  --grpc_out=services/mono/proto \
  --plugin=protoc-gen-grpc=$(which grpc_csharp_plugin) \
  api/v1/hello.proto api/v1/service.proto api/v1/boardcast.proto
```

macOS Apple Silicon 需使用 Rosetta 包装脚本：

```bash
bash scripts/grpc_csharp_plugin_rosetta.sh
```
