# Build context: repo root
#   docker build -f services/boardcast/Dockerfile -t boardcast .
#   docker build -f services/boardcast/Dockerfile --build-arg TARGETARCH=amd64 -t boardcast .
ARG TARGETARCH=arm64

FROM --platform=linux/${TARGETARCH} rust:1-bookworm AS builder

WORKDIR /app
RUN apt-get update && apt-get install -y protobuf-compiler && rm -rf /var/lib/apt/lists/*

COPY Cargo.lock ./
COPY services/boardcast/ ./services/boardcast/
COPY proto/ ./proto/

# 生成只包含 boardcast 的精简 workspace，避免 cargo 找其他成员目录
RUN printf '[workspace]\nmembers = ["services/boardcast"]\nresolver = "1"\n\n[workspace.dependencies]\ntonic = { version = "0.11", features = ["gzip", "transport"] }\ntonic-reflection = "0.11"\ntonic-build = { version = "0.11", features = ["prost"] }\nprost = "0.12"\ntokio = { version = "1", features = ["full"] }\naxum = { version = "0.7", features = ["json", "macros"] }\ntower = "0.4"\ntower-http = { version = "0.5", features = ["cors"] }\nserde = { version = "1", features = ["derive"] }\nserde_json = "1"\nchrono = { version = "0.4", features = ["serde"] }\ntokio-stream = { version = "0.1", features = ["sync"] }\ndashmap = "5"\n' > Cargo.toml

RUN cargo build --release -p boardcast

FROM --platform=linux/${TARGETARCH} debian:bookworm-slim

RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=builder /app/target/release/boardcast /app/boardcast

ENV GRPC_ADDR=[::]:50052
ENV HTTP_ADDR=[::]:8081
EXPOSE 50052 8081

ENTRYPOINT ["/app/boardcast"]
