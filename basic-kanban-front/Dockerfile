# Stage 1: Build
FROM node:20-alpine AS build

WORKDIR /app

# Copiar package.json e package-lock.json
COPY package*.json ./

# Instalar dependências
RUN npm ci

# Copiar código fonte
COPY . .

# Build da aplicação
RUN npm run build -- --configuration production

# Stage 2: Serve
FROM nginx:alpine

# Copiar build do Angular para o nginx
COPY --from=build /app/dist/basic-kanban-front/browser /usr/share/nginx/html

# Copiar configuração customizada do nginx (opcional)
COPY nginx.conf /etc/nginx/conf.d/default.conf

# Expor porta 80
EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]