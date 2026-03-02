# Build context: repo root
#   docker build -f services/auth/Dockerfile -t auth .
#   docker build -f services/auth/Dockerfile --build-arg TARGETARCH=amd64 -t auth .
ARG TARGETARCH=arm64

FROM --platform=linux/${TARGETARCH} rust:1-bookworm AS builder

WORKDIR /app

COPY Cargo.toml Cargo.lock ./
COPY services/auth/ ./services/auth/

RUN cargo build --release -p auth

FROM --platform=linux/${TARGETARCH} debian:bookworm-slim

RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=builder /app/target/release/auth /app/auth

ENV HTTP_ADDR=[::]:8081
EXPOSE 8081

ENTRYPOINT ["/app/auth"]
