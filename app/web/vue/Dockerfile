FROM node:20-alpine AS builder

ARG PROJECT_NAME=client

WORKDIR /app

COPY package*.json ./

RUN --mount=type=cache,target=/root/.npm \
    npm ci --legacy-peer-deps

COPY . .

ENV NODE_OPTIONS=--max_old_space_size=4096

RUN ./node_modules/.bin/cross-env PROJECT_NAME=${PROJECT_NAME} DISABLE_FORK_TS_CHECKER=true DOCKER_BUILD=true ./node_modules/.bin/vue-cli-service build --mode production && \
    mv /app/dist/${PROJECT_NAME} /app/dist/app

FROM nginx:alpine

COPY --from=builder /app/dist/app /usr/share/nginx/html

COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
