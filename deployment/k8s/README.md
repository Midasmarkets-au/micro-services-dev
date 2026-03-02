# Kubernetes 部署

## 架构概览

```
┌─────────┐  HTTP   ┌──────────────┐  gRPC    ┌─────────┐
│  ALB /  │ ──────► │  mono (.NET) │ ───────► │  idgen  │
│  NLB    │  :80    │              │  :50051  │  (Rust) │
└─────────┘         └──────┬───────┘          └─────────┘
                           │
                           ▼
                     ┌───────────┐
                     │   Redis   │
                     │   :6379   │
                     └───────────┘
```

- **mono** — .NET 8 Gateway Web 服务，HTTP 端口 80；通过 K8s Service 调用 idgen 的 gRPC。
- **idgen** — Rust ID 生成服务，HTTP 端口 8080 + gRPC 端口 50051。
- **Redis** — 缓存 / SignalR 背板，mono 依赖。

## 文件说明

| 文件 | 说明 |
|------|------|
| **idgen** | |
| `idgen-deployment.yaml` | idgen Deployment（Rust，端口 8080 + 50051） |
| `idgen-service.yaml` | idgen Service（ClusterIP，供 mono 内部调用） |
| **mono** | |
| `mono-deployment.yaml` | mono Deployment（.NET 8，端口 80，依赖 Secret `mono-env`） |
| `mono-service.yaml` | mono Service（LoadBalancer，公网暴露 80） |
| **基础设施** | |
| `redis-deployment.yaml` | Redis Deployment + Service（ClusterIP `redis:6379`） |
| `gateway-ingress.yaml` | IngressClass + Ingress（AWS ALB → mono:80） |
| `nodepool.yaml` | Karpenter NodePool（arm64） |
| `nodegroup.yaml` | EKS Managed Node Group（Graviton） |

## 前置条件

1. 已构建镜像并推送到 ECR（`docker build --target idgen` / `docker build --target mono`）。
2. 已创建 Secret `mono-env`（见下方）。
3. （若用 ALB）已安装 AWS Load Balancer Controller，公网子网打标 `kubernetes.io/role/elb=1`。

## 1. 上传 .env 为 Secret（必做）

mono Pod 内无 .env 文件，配置完全依赖 Secret。

```bash
kubectl create namespace bacera-gateway --dry-run=client -o yaml | kubectl apply -f -

kubectl create secret generic mono-env \
  --from-env-file=services/mono/Bacera.Gateway.Web/.env \
  -n bacera-gateway \
  --dry-run=client -o yaml | kubectl apply -f -
```

更新 Secret 后重启 Deployment：

```bash
kubectl rollout restart deployment/mono -n bacera-gateway
```

## 2. 部署顺序

```bash
# 1. Redis（mono 依赖 redis:6379）
kubectl apply -f deployment/k8s/redis-deployment.yaml

# 2. idgen（mono 依赖 idgen:50051）
kubectl apply -f deployment/k8s/idgen-deployment.yaml
kubectl apply -f deployment/k8s/idgen-service.yaml

# 3. mono
kubectl apply -f deployment/k8s/mono-deployment.yaml
kubectl apply -f deployment/k8s/mono-service.yaml

# 4.（可选）ALB Ingress
kubectl apply -f deployment/k8s/gateway-ingress.yaml
```

一键部署：

```bash
kubectl apply -f deployment/k8s/redis-deployment.yaml \
              -f deployment/k8s/idgen-deployment.yaml \
              -f deployment/k8s/idgen-service.yaml \
              -f deployment/k8s/mono-deployment.yaml \
              -f deployment/k8s/mono-service.yaml \
              -f deployment/k8s/gateway-ingress.yaml
```

## 修改 gRPC 地址

mono 通过环境变量 `Grpc__RemoteHello__Address` 连接 idgen，默认 `http://idgen:50051`。

- 同 namespace：`http://idgen:50051`
- 跨 namespace：`http://idgen.<namespace>.svc.cluster.local:50051`

## Redis 配置

mono 依赖 Redis（SignalR 背板、缓存），Pod 内不可用 `localhost:6379`。

- **集群内 Redis** — 部署 `redis-deployment.yaml`，Service 名 `redis`，端口 6379。在 Secret 中设置 `REDIS_CONNECTION=redis:6379` 和 `REDIS_PASSWORD`。
- **ElastiCache** — 在 `mono-deployment.yaml` 的 `env` 中覆盖 `REDIS_CONNECTION` 和 `REDIS_PASSWORD`；集群模式设 `REDIS_CLUSTER_MODE=true`。

## ALB 暴露（EKS）

`gateway-ingress.yaml` 通过 AWS Load Balancer Controller 创建 ALB → `mono:80`。

### 前置

1. 集群已安装 AWS Load Balancer Controller：`kubectl get pods -A | grep load-balancer`
2. 公网子网打标（至少 2 个、不同 AZ）：`kubernetes.io/role/elb=1`

### 查看 ALB 地址

```bash
kubectl get ingress gateway-web -n bacera-gateway
# ADDRESS 列为 ALB DNS，用 http://<ADDRESS> 访问
```

### ADDRESS 为空排查

1. 确认 Controller 在运行：`kubectl get pods -A | grep load-balancer`
2. 查看 Ingress 事件：`kubectl describe ingress gateway-web`
3. 确认公网子网打标（至少 2 个）
4. 或在 `gateway-ingress.yaml` 显式指定子网 ID：`alb.ingress.kubernetes.io/subnets: subnet-xxx,subnet-yyy`

### 不用 ALB（改用 NLB）

`mono-service.yaml` 已是 `type: LoadBalancer`，EKS 上会直接创建 NLB。此时可不部署 `gateway-ingress.yaml`。

### 本地集群（Colima / minikube / kind）

ALB 仅在 EKS 上有效。本地开发改用 nginx Ingress 或直接 `kubectl port-forward`。
