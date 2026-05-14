FROM node:24-alpine AS build
WORKDIR /app

COPY frontend/package*.json ./
RUN npm ci

COPY frontend/ ./
ARG VITE_API_BASE_URL=http://localhost:8080
ARG VITE_SIGNALR_HUB_URL=http://localhost:8080/hubs/business
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_SIGNALR_HUB_URL=$VITE_SIGNALR_HUB_URL
RUN npm run build

FROM nginx:1.27-alpine AS final
COPY frontend/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
