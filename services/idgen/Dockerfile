# Build context: repo root
#   docker build -f services/idgen/Dockerfile -t idgen .
#   docker build -f services/idgen/Dockerfile --build-arg TARGETARCH=amd64 -t idgen .
ARG TARGETARCH=arm64

FROM --platform=linux/${TARGETARCH} rust:1-bookworm AS builder

WORKDIR /app
RUN apt-get update && apt-get install -y protobuf-compiler && rm -rf /var/lib/apt/lists/*

COPY Cargo.toml Cargo.lock ./
COPY services/idgen/ ./services/idgen/
COPY proto/ ./proto/

RUN cargo build --release -p idgen

FROM --platform=linux/${TARGETARCH} debian:bookworm-slim

RUN apt-get update && apt-get install -y ca-certificates && rm -rf /var/lib/apt/lists/*
WORKDIR /app
COPY --from=builder /app/target/release/idgen /app/idgen

ENV GRPC_ADDR=[::1]:50051
ENV HTTP_ADDR=[::]:8080
EXPOSE 50051 8080

ENTRYPOINT ["/app/idgen"]
