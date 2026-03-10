# Kubernetes 部署

## 架构概览

```
                    ┌──────────────────────────────────────────────────┐
                    │                 Kubernetes Cluster                │
                    │                                                   │
Internet            │  ┌─────────────────────────────────────────────┐ │
   │                │  │           NGINX Ingress (bacera-ingress)     │ │
   │                │  │  portal.trademdm.com/                        │ │
   ├─ /event ──────►│  │    /event  ──────────► boardcast:8081 (SSE) │ │
   └─ /api|...  ───►│  │    /api|.. ──────────► mono:80              │ │
                    │  │    /       ──────────► front-client:80       │ │
                    │  └─────────────────────────────────────────────┘ │
                    │                                                   │
                    │  mono (.NET 8) ──gRPC:50051──► idgen (Rust)      │
                    │       │        ──gRPC:50052──► boardcast (Rust)  │
                    │       └──────────────────────► Redis :6379       │
                    └──────────────────────────────────────────────────┘
```

## 文件说明

| 文件 | 说明 |
|------|------|
| `idgen-deployment.yaml` | idgen Deployment（Rust，HTTP :8080 + gRPC :50051） |
| `idgen-service.yaml` | idgen Service（ClusterIP） |
| `boardcast-deployment.yaml` | boardcast Deployment（Rust，HTTP :8081 + gRPC :50052） |
| `boardcast-service.yaml` | boardcast Service（ClusterIP） |
| `mono-deployment.yaml` | mono Deployment（.NET 8，HTTP :80） |
| `mono-service.yaml` | mono Service（LoadBalancer，公网暴露 :80） |
| `redis-deployment.yaml` | Redis Deployment + Service（ClusterIP `redis:6379`） |
| `gateway-ingress.yaml` | 所有 Ingress 规则（bacera-ingress + boardcast-event-ingress） |
| `nodepool.yaml` | Karpenter NodePool（arm64） |
| `nodegroup.yaml` | EKS Managed Node Group（Graviton） |

## 前置条件

1. 已构建镜像并推送到 ECR。
2. 已创建 Secret `mono-env`（见下方）。
3. 已安装 NGINX Ingress Controller。

## 1. 上传配置为 Secret

```bash
kubectl create secret generic mono-env \
  --from-env-file=services/mono/Bacera.Gateway.Web/.env \
  --dry-run=client -o yaml | kubectl apply -f -
```

更新 Secret 后重启：

```bash
kubectl rollout restart deployment/mono
```

## 2. 部署顺序

```bash
# 基础设施
kubectl apply -f deployment/k8s/redis-deployment.yaml

# Rust 服务
kubectl apply -f deployment/k8s/idgen-deployment.yaml
kubectl apply -f deployment/k8s/idgen-service.yaml
kubectl apply -f deployment/k8s/boardcast-deployment.yaml
kubectl apply -f deployment/k8s/boardcast-service.yaml

# .NET 服务
kubectl apply -f deployment/k8s/mono-deployment.yaml
kubectl apply -f deployment/k8s/mono-service.yaml

# Ingress（主路由 + SSE 路由）
kubectl apply -f deployment/k8s/gateway-ingress.yaml
```

一键部署：

```bash
kubectl apply \
  -f deployment/k8s/redis-deployment.yaml \
  -f deployment/k8s/idgen-deployment.yaml \
  -f deployment/k8s/idgen-service.yaml \
  -f deployment/k8s/boardcast-deployment.yaml \
  -f deployment/k8s/boardcast-service.yaml \
  -f deployment/k8s/mono-deployment.yaml \
  -f deployment/k8s/mono-service.yaml \
  -f deployment/k8s/gateway-ingress.yaml
```

## 3. 关键环境变量

### mono

| 变量 | 默认值 | 说明 |
|------|--------|------|
| `IDGEN_GRPC_ADDR` | `http://idgen:50051` | idgen gRPC 地址 |
| `BOARDCAST_GRPC_ADDR` | `http://boardcast:50052` | boardcast gRPC 地址 |
| `REDIS_CONNECTION` | `redis:6379` | Redis 地址 |

### boardcast

| 变量 | 默认值 | 说明 |
|------|--------|------|
| `GRPC_ADDR` | `[::]:50052` | gRPC 监听地址 |
| `HTTP_ADDR` | `[::]:8081` | HTTP/SSE 监听地址 |

### idgen

| 变量 | 默认值 | 说明 |
|------|--------|------|
| `GRPC_ADDR` | `[::]:50051` | gRPC 监听地址 |
| `HTTP_ADDR` | `[::]:8080` | HTTP 监听地址 |

## 4. Ingress 路由说明

`gateway-ingress.yaml` 包含两个 Ingress 对象：

**`bacera-ingress`** — 主路由

| 路径 | 后端 | 说明 |
|------|------|------|
| `/(api\|connect\|hub\|live\|swagger\|.well-known)/` | `mono:80` | REST API / SignalR / Swagger |
| `/` | `front-client:80` | 前端静态资源 |

**`boardcast-event-ingress`** — SSE 路由（`proxy-buffering: off`）

| 路径 | 后端 | 说明 |
|------|------|------|
| `/event` | `boardcast:8081` | SSE 长连接，实时事件推送 |

两个对象分开的原因：`proxy-buffering: off` 作用于整个 Ingress 对象，若合并会影响所有路由的缓冲行为。

SSE 订阅地址：
```
https://portal.trademdm.com/event?channel=<name>
```

## 5. CI/CD

GitHub Actions 工作流在 `staging` 分支推送时自动构建并部署：

| 工作流 | 触发路径 | 部署目标 |
|--------|---------|---------|
| `deploy-mono-staging.yml` | `services/mono/**`, `proto/**` | `deployment/mono` |
| `deploy-boardcast-staging.yml` | `services/boardcast/**`, `proto/api/v1/boardcast.proto` | `deployment/boardcast` |

所需 Secrets：`AWS_ACCESS_KEY_ID`、`AWS_SECRET_ACCESS_KEY`、`EKS_CLUSTER_NAME`

## 6. 健康检查

```bash
# 查看所有 Pod 状态
kubectl get pods

# idgen
curl http://<idgen-pod-ip>:8080/api/v1/health

# boardcast（TCP 连接即健康）
kubectl port-forward svc/boardcast 8081:8081
curl -N http://localhost:8081/event?channel=ping

# mono
kubectl port-forward svc/mono 8080:80
curl http://localhost:8080/api/v1/health

# 查看 Ingress 地址
kubectl get ingress
```

## 7. 本地集群（Colima / minikube / kind）

ALB/NLB 仅在 EKS 上有效。本地开发改用 port-forward：

```bash
kubectl port-forward svc/mono 5000:80
kubectl port-forward svc/boardcast 8081:8081
kubectl port-forward svc/idgen 8080:8080
```
