# Build stage
FROM node:22-alpine AS builder

WORKDIR /app

# Copy Yarn and package files
COPY .yarn .yarn
COPY .yarnrc.yml package.json yarn.lock ./

# Install dependencies
RUN yarn install --immutable

# Copy source code (includes .env for Vite)
COPY . .

# Build the application
RUN yarn build

# Production stage
FROM nginx:alpine AS production

# Copy custom nginx config
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Copy built assets from builder stage
COPY --from=builder /app/dist /usr/share/nginx/html

# Expose port 80
EXPOSE 80

# Start nginx
CMD ["nginx", "-g", "daemon off;"]

