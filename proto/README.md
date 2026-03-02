# Proto 定义

- `api/v1/service.proto`：主服务定义（来源 [Protobuf-api](https://github.com/lucas-edge/Protobuf-api)），提供 gRPC 与 HTTP REST 接口。
- `api/v1/hello.proto`：Hello 相关消息（同上）。
- `google/api/`：Google API 的 http.proto、annotations.proto，用于 `google.api.http` 注解。
- `google/protobuf/descriptor.proto`：Protocol Buffers 描述符，供 annotations 扩展使用。

## C# 代码生成（本地 CLI，build 不执行 protoc）

**dotnet build 不再执行 protoc / grpc_csharp_plugin**，而是直接使用已生成的 C# 代码：

- **generated/**：`Hello.cs`、`Service.cs`（protoc --csharp_out）
- **services/mono/proto/**：`ServiceGrpc.cs`（grpc_csharp_plugin 生成）

修改 proto 后需在本地重新生成，再提交生成文件。示例（在仓库根目录）：

```bash
# 生成 C# 消息与 gRPC 服务代码（需本地 protoc + grpc_csharp_plugin）
protoc -I proto --csharp_out=services/mono/proto --grpc_out=services/mono/proto \
  --plugin=protoc-gen-grpc=$(which grpc_csharp_plugin) \
  api/v1/hello.proto api/v1/service.proto
# 若 ServiceGrpc.cs 希望放到 services/mono/proto，可再复制或调整 --grpc_out=services/mono/proto
```

也可使用仓库内脚本（若已配置）：`./scripts/generate_proto.sh`（输出目录为 `services/mono/proto`）。
