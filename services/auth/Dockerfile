# Build context: repo root
#   docker build -f services/auth/Dockerfile -t auth .
ARG TARGETARCH=arm64

FROM --platform=linux/${TARGETARCH} rust:1-bookworm AS builder

WORKDIR /app

# Copy workspace manifest, lock file, and patched vendor crates
COPY Cargo.toml Cargo.lock ./
COPY vendor/ ./vendor/

# Copy all workspace members needed to resolve the workspace
COPY services/auth/     ./services/auth/
COPY services/auth_old/ ./services/auth_old/
COPY services/otel/     ./services/otel/

# Stub out other workspace members so Cargo can resolve the workspace
# without copying their full source
COPY services/idgen/Cargo.toml     ./services/idgen/Cargo.toml
COPY services/boardcast/Cargo.toml ./services/boardcast/Cargo.toml
COPY services/scheduler/Cargo.toml ./services/scheduler/Cargo.toml
COPY services/tools/Cargo.toml     ./services/tools/Cargo.toml

RUN mkdir -p services/idgen/src services/boardcast/src services/scheduler/src services/tools/src && \
    echo 'fn main(){}' > services/idgen/src/main.rs && \
    echo 'fn main(){}' > services/boardcast/src/main.rs && \
    echo 'fn main(){}' > services/scheduler/src/main.rs && \
    echo 'fn main(){}' > services/tools/src/main.rs

# Install protoc for tonic-build
RUN apt-get update && apt-get install -y protobuf-compiler && rm -rf /var/lib/apt/lists/*

# Copy proto definitions
COPY proto/ ./proto/

RUN cargo build --release -p auth

# ── Runtime image ─────────────────────────────────────────────────────────────
FROM --platform=linux/${TARGETARCH} debian:bookworm-slim

RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=builder /app/target/release/auth /app/auth

ENV HTTP_ADDR=[::]:9001
ENV GRPC_ADDR=[::]:50006
ENV RUST_LOG=info

EXPOSE 9001
EXPOSE 50006

ENTRYPOINT ["/app/auth"]
