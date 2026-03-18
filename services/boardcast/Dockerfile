# Build context: repo root
#   docker build -f services/boardcast/Dockerfile -t boardcast .
#   docker build -f services/boardcast/Dockerfile --build-arg TARGETARCH=amd64 -t boardcast .
ARG TARGETARCH=arm64

# ── Stage 1: install cargo-chef ──────────────────────────────────────────────
FROM --platform=linux/${TARGETARCH} rust:1-bookworm AS chef
RUN cargo install cargo-chef
RUN apt-get update && apt-get install -y protobuf-compiler && rm -rf /var/lib/apt/lists/*

# ── Stage 2: generate recipe.json (only needs Cargo files, no source) ────────
FROM chef AS planner
WORKDIR /app
COPY Cargo.lock ./
COPY services/boardcast/Cargo.toml ./services/boardcast/Cargo.toml
# 生成只包含 boardcast 的精简 workspace
RUN printf '[workspace]\nmembers = ["services/boardcast"]\nresolver = "1"\n\n[workspace.dependencies]\ntonic = { version = "0.11", features = ["gzip", "transport"] }\ntonic-reflection = "0.11"\ntonic-build = { version = "0.11", features = ["prost"] }\nprost = "0.12"\ntokio = { version = "1", features = ["full"] }\naxum = { version = "0.7", features = ["json", "macros"] }\ntower = "0.4"\ntower-http = { version = "0.5", features = ["cors"] }\nserde = { version = "1", features = ["derive"] }\nserde_json = "1"\nchrono = { version = "0.4", features = ["serde"] }\ntokio-stream = { version = "0.1", features = ["sync"] }\ndashmap = "5"\n' > Cargo.toml
RUN mkdir -p services/boardcast/src && printf 'fn main() {}' > services/boardcast/src/main.rs
RUN cargo chef prepare --recipe-path recipe.json

# ── Stage 3: compile dependencies (cached as long as deps unchanged) ─────────
FROM chef AS builder
WORKDIR /app
COPY --from=planner /app/recipe.json recipe.json
COPY Cargo.lock ./
COPY services/boardcast/Cargo.toml ./services/boardcast/Cargo.toml
# Proto files needed here because build.rs (tonic-build) runs during cook
COPY proto/ ./proto/
RUN printf '[workspace]\nmembers = ["services/boardcast"]\nresolver = "1"\n\n[workspace.dependencies]\ntonic = { version = "0.11", features = ["gzip", "transport"] }\ntonic-reflection = "0.11"\ntonic-build = { version = "0.11", features = ["prost"] }\nprost = "0.12"\ntokio = { version = "1", features = ["full"] }\naxum = { version = "0.7", features = ["json", "macros"] }\ntower = "0.4"\ntower-http = { version = "0.5", features = ["cors"] }\nserde = { version = "1", features = ["derive"] }\nserde_json = "1"\nchrono = { version = "0.4", features = ["serde"] }\ntokio-stream = { version = "0.1", features = ["sync"] }\ndashmap = "5"\n' > Cargo.toml
# 只要 Cargo.toml/Cargo.lock/proto 不变，此层永久缓存
RUN cargo chef cook --release --recipe-path recipe.json

# Copy real source and build only the changed business code
COPY services/boardcast/ ./services/boardcast/
RUN cargo build --release -p boardcast

# ── Stage 4: minimal runtime image ───────────────────────────────────────────
FROM --platform=linux/${TARGETARCH} debian:bookworm-slim

RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=builder /app/target/release/boardcast /app/boardcast

ENV GRPC_ADDR=[::]:50052
ENV HTTP_ADDR=[::]:8081
EXPOSE 50052 8081

ENTRYPOINT ["/app/boardcast"]
