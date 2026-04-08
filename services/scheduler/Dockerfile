# Build context: repo root
#   docker build -f services/scheduler/Dockerfile -t scheduler .
#   docker build -f services/scheduler/Dockerfile --build-arg TARGETARCH=amd64 -t scheduler .
ARG TARGETARCH=arm64

# ── Stage 1: cargo-chef base (pre-built image, no need to compile cargo-chef) ─
FROM --platform=linux/${TARGETARCH} lukemathwalker/cargo-chef:latest-rust-1-bookworm AS chef
RUN apt-get update && apt-get install -y protobuf-compiler && rm -rf /var/lib/apt/lists/*

# ── Stage 2: generate recipe.json (only needs Cargo files, no source) ────────
FROM chef AS planner
WORKDIR /app
# Real workspace Cargo.toml/lock (includes [patch.crates-io] for vendor/apalis-redis)
COPY Cargo.toml Cargo.lock ./
COPY vendor/ ./vendor/
# Scheduler Cargo.toml
COPY services/scheduler/Cargo.toml ./services/scheduler/Cargo.toml
# Stubs for other workspace members so the workspace resolves correctly
COPY services/idgen/Cargo.toml ./services/idgen/Cargo.toml
COPY services/auth/Cargo.toml ./services/auth/Cargo.toml
COPY services/boardcast/Cargo.toml ./services/boardcast/Cargo.toml
COPY services/tools/Cargo.toml ./services/tools/Cargo.toml
COPY services/otel/Cargo.toml ./services/otel/Cargo.toml
COPY services/auth_rust/Cargo.toml ./services/auth_rust/Cargo.toml
RUN for svc in idgen auth boardcast tools scheduler; do \
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
COPY services/scheduler/Cargo.toml ./services/scheduler/Cargo.toml
COPY services/idgen/Cargo.toml ./services/idgen/Cargo.toml
COPY services/auth/Cargo.toml ./services/auth/Cargo.toml
COPY services/boardcast/Cargo.toml ./services/boardcast/Cargo.toml
COPY services/tools/Cargo.toml ./services/tools/Cargo.toml
COPY services/otel/Cargo.toml ./services/otel/Cargo.toml
COPY services/auth_rust/Cargo.toml ./services/auth_rust/Cargo.toml
# Only deps change → this layer is cached. Use cargo cache mount to avoid
# re-downloading crates even when this layer is invalidated.
RUN --mount=type=cache,target=/root/.cargo/registry \
    --mount=type=cache,target=/root/.cargo/git \
    cargo chef cook --release --recipe-path recipe.json

# Proto and real source are only needed to compile the scheduler itself (build.rs)
COPY proto/ ./proto/
COPY services/otel/ ./services/otel/
COPY services/scheduler/ ./services/scheduler/
RUN --mount=type=cache,target=/root/.cargo/registry \
    --mount=type=cache,target=/root/.cargo/git \
    cargo build --release -p scheduler

# ── Stage 4: minimal runtime image ───────────────────────────────────────────
FROM --platform=linux/${TARGETARCH} debian:bookworm-slim

RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=builder /app/target/release/scheduler /app/scheduler

ENV SCHEDULER_PORT=9004
ENV SCHEDULER_GRPC_PORT=50004
EXPOSE 9004 50004

ENTRYPOINT ["/app/scheduler"]
