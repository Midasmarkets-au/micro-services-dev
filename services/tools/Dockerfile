# Build context: repo root
#   docker build -f services/tools/Dockerfile -t tools .
#   docker build -f services/tools/Dockerfile --build-arg TARGETARCH=amd64 -t tools .
ARG TARGETARCH=arm64

FROM --platform=linux/${TARGETARCH} rust:1-bookworm AS builder

WORKDIR /app

COPY Cargo.lock ./
COPY services/tools/ ./services/tools/

# Generate minimal workspace containing only tools to avoid cargo resolving other members
RUN printf '[workspace]\nmembers = ["services/tools"]\nresolver = "1"\n\n[workspace.dependencies]\ntokio = { version = "1", features = ["full"] }\nreqwest = { version = "0.11", features = ["blocking", "rustls-tls", "json"] }\nserde = { version = "1", features = ["derive"] }\nserde_json = "1"\nchrono = { version = "0.4", features = ["serde"] }\n' > Cargo.toml

RUN cargo build --release -p tools

FROM --platform=linux/${TARGETARCH} debian:bookworm-slim

RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=builder /app/target/release/tools /app/tools

ENTRYPOINT ["/app/tools"]
