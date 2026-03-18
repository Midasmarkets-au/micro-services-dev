# Build context: repo root
#   docker build -f services/tools/Dockerfile -t tools .
#   docker build -f services/tools/Dockerfile --build-arg TARGETARCH=amd64 -t tools .
ARG TARGETARCH=arm64

# ── Stage 1: install cargo-chef ──────────────────────────────────────────────
FROM --platform=linux/${TARGETARCH} rust:1-bookworm AS chef
RUN cargo install cargo-chef

# ── Stage 2: generate recipe.json (only needs Cargo files, no source) ────────
FROM chef AS planner
WORKDIR /app
COPY Cargo.lock ./
COPY services/tools/Cargo.toml ./services/tools/Cargo.toml
# Generate minimal workspace containing only tools
RUN printf '[workspace]\nmembers = ["services/tools"]\nresolver = "1"\n\n[workspace.dependencies]\ntokio = { version = "1", features = ["full"] }\nreqwest = { version = "0.11", features = ["blocking", "rustls-tls", "json"] }\nserde = { version = "1", features = ["derive"] }\nserde_json = "1"\nchrono = { version = "0.4", features = ["serde"] }\n' > Cargo.toml
RUN mkdir -p services/tools/src && printf 'fn main() {}' > services/tools/src/main.rs
RUN cargo chef prepare --recipe-path recipe.json

# ── Stage 3: compile dependencies (cached as long as deps unchanged) ─────────
FROM chef AS builder
WORKDIR /app
COPY --from=planner /app/recipe.json recipe.json
COPY Cargo.lock ./
COPY services/tools/Cargo.toml ./services/tools/Cargo.toml
RUN printf '[workspace]\nmembers = ["services/tools"]\nresolver = "1"\n\n[workspace.dependencies]\ntokio = { version = "1", features = ["full"] }\nreqwest = { version = "0.11", features = ["blocking", "rustls-tls", "json"] }\nserde = { version = "1", features = ["derive"] }\nserde_json = "1"\nchrono = { version = "0.4", features = ["serde"] }\n' > Cargo.toml
# 只要 Cargo.toml/Cargo.lock 不变，此层永久缓存
RUN cargo chef cook --release --recipe-path recipe.json

# Copy real source and build only the changed business code
COPY services/tools/ ./services/tools/
RUN cargo build --release -p tools

# ── Stage 4: minimal runtime image ───────────────────────────────────────────
FROM --platform=linux/${TARGETARCH} debian:bookworm-slim

RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=builder /app/target/release/tools /app/tools

ENTRYPOINT ["/app/tools"]
