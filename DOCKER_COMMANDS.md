# Docker Commands for Kanban App

## Production Build (com Nginx + Final Images)

### Build e rodar tudo junto
```powershell
docker compose up -d --build
```

### Parar tudo
```powershell
docker compose down
```

---

## Development (com source code mounting + live reload)

### Rodar frontend e backend em desenvolvimento
```powershell
docker compose -f docker-compose.dev.yml up -d --build
```

### Ver logs do frontend (live reload)
```powershell
docker compose -f docker-compose.dev.yml logs -f frontend
```

### Ver logs do backend
```powershell
docker compose -f docker-compose.dev.yml logs -f api
```

### Parar tudo
```powershell
docker compose -f docker-compose.dev.yml down
```

---

## Build Separados (sem Docker)

### Frontend apenas (Angular dev server)
```powershell
cd basic-kanban-front
npm start
# Acessa em http://localhost:4200
```

### Backend apenas (.NET dev server)
```powershell
cd basic-kanban-api/basic-kanban-api
dotnet run
# Acessa em http://localhost:5000
```

---

## Docker - Apenas Frontend
```powershell
# Build imagem
docker build -f basic-kanban-front/Dockerfile.dev -t kanban-frontend:dev basic-kanban-front

# Rodar container
docker run -p 4200:4200 -v $(pwd)/basic-kanban-front/src:/app/src kanban-frontend:dev

# Parar
docker stop <container_id>
```

---

## Docker - Apenas Backend
```powershell
# Build imagem
docker build -f basic-kanban-api/basic-kanban-api/Dockerfile.dev -t kanban-api:dev basic-kanban-api

# Rodar container (sem BD, para testes rápidos)
docker run -p 5000:8080 \
  -e ASPNETCORE_ENVIRONMENT=Development \
  -e ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=kanban_db;Username=kanban_user;Password=kanban_pass" \
  kanban-api:dev

# Parar
docker stop <container_id>
```

---

## Troubleshooting

### Limpar tudo (containers, images, volumes)
```powershell
docker compose down -v
docker compose -f docker-compose.dev.yml down -v
docker system prune -a
```

### Verificar containers rodando
```powershell
docker ps
docker ps -a  # incluindo parados
```

### Ver logs de um container
```powershell
docker logs <container_name>
docker logs -f <container_name>  # follow (live)
```

### Entrar em um container
```powershell
docker exec -it <container_name> sh
docker exec -it kanban-api-dev bash
```
