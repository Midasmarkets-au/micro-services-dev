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
COPY Cargo.toml Cargo.lock ./
COPY vendor/ ./vendor/
COPY proto/ ./proto/
COPY services/boardcast/Cargo.toml ./services/boardcast/Cargo.toml
# Stubs for other workspace members so the workspace resolves correctly
COPY services/idgen/Cargo.toml ./services/idgen/Cargo.toml
COPY services/auth/Cargo.toml ./services/auth/Cargo.toml
COPY services/scheduler/Cargo.toml ./services/scheduler/Cargo.toml
COPY services/tools/Cargo.toml ./services/tools/Cargo.toml
COPY services/otel/Cargo.toml ./services/otel/Cargo.toml
COPY services/auth_rust/Cargo.toml ./services/auth_rust/Cargo.toml
RUN for svc in idgen auth boardcast scheduler tools; do \
      mkdir -p services/$svc/src && printf 'fn main() {}' > services/$svc/src/main.rs; \
    done && \
    mkdir -p services/otel/src && touch services/otel/src/lib.rs && \
    mkdir -p services/auth_rust/src && printf 'fn main() {}' > services/auth_rust/src/main.rs && touch services/auth_rust/src/lib.rs
RUN cargo chef prepare --recipe-path recipe.json

# ── Stage 3: compile dependencies (cached as long as deps unchanged) ─────────
FROM chef AS builder
WORKDIR /app
COPY --from=planner /app/recipe.json recipe.json
COPY Cargo.toml Cargo.lock ./
COPY vendor/ ./vendor/
COPY proto/ ./proto/
COPY services/boardcast/Cargo.toml ./services/boardcast/Cargo.toml
COPY services/idgen/Cargo.toml ./services/idgen/Cargo.toml
COPY services/auth/Cargo.toml ./services/auth/Cargo.toml
COPY services/scheduler/Cargo.toml ./services/scheduler/Cargo.toml
COPY services/tools/Cargo.toml ./services/tools/Cargo.toml
COPY services/otel/Cargo.toml ./services/otel/Cargo.toml
COPY services/auth_rust/Cargo.toml ./services/auth_rust/Cargo.toml
# Only deps change → this layer is cached
RUN cargo chef cook --release --recipe-path recipe.json

# Copy real source and build only the changed business code
COPY services/otel/ ./services/otel/
COPY services/boardcast/ ./services/boardcast/
RUN cargo build --release -p boardcast

# ── Stage 4: minimal runtime image ───────────────────────────────────────────
FROM --platform=linux/${TARGETARCH} debian:bookworm-slim

RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=builder /app/target/release/boardcast /app/boardcast

ENV GRPC_ADDR=[::]:50003
ENV HTTP_ADDR=[::]:9003
EXPOSE 50003 9003

ENTRYPOINT ["/app/boardcast"]
