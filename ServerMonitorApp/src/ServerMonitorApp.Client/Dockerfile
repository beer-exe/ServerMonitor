# Stage 1: Build React App
FROM node:20-alpine AS build
WORKDIR /app

# Copy file cấu hình package và cài đặt dependencies
COPY package*.json ./
RUN npm install

# Copy source code
COPY . .

# Nhận biến môi trường lúc build (URL của Backend API)
ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL

# Build frontend
RUN npm run build

# Stage 2: Serve bằng Nginx
FROM nginx:alpine
# Copy file build từ stage 1 sang thư mục của Nginx
COPY --from=build /app/dist /usr/share/nginx/html

# Cấu hình Nginx để hỗ trợ React Router (Tránh lỗi 404 khi f5 trang SPA)
RUN echo 'server { \
    listen 80; \
    location / { \
        root /usr/share/nginx/html; \
        index index.html index.htm; \
        try_files $uri $uri/ /index.html; \
    } \
}' > /etc/nginx/conf.d/default.conf

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]