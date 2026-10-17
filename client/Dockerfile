# syntax=docker/dockerfile:1

# ------------------------------------------------------------------------------
# Build stage
# VITE_API_URL is deliberately NOT set: src/api/client.ts falls back to '' so
# every request is a same-origin relative path (/api/...), which the reverse
# proxy routes to the backend.
# ------------------------------------------------------------------------------
FROM node:24-alpine AS build
WORKDIR /app

COPY package.json package-lock.json ./
RUN npm ci

COPY . .
RUN npm run build

# ------------------------------------------------------------------------------
# Runtime stage: static files served by nginx
# ------------------------------------------------------------------------------
FROM nginx:alpine AS runtime

COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html

EXPOSE 80
