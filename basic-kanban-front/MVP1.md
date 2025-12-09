# MVP 1 - Kanban App Frontend

## Features Implementadas

### 1️⃣ Autenticação (Auth)
- **LoginComponent**: Tela de login com validação de email/senha
- **RegisterComponent**: Tela de cadastro com validação de senha forte
- **AuthService**: Gerencia JWT token, armazenamento local e estado de autenticação
- **AuthInterceptor**: Injeta automaticamente token nos headers das requisições
- **authGuard**: Protege rotas que requerem autenticação

### 2️⃣ Boards (Quadros)
- **BoardsListComponent**: 
  - Lista todos os boards do usuário
  - Criar novo board (título + descrição)
  - Deletar board com confirmação
  - Navegar para detalhe do board
  
- **BoardDetailComponent**:
  - Visualizar detalhes do board
  - Voltar para lista
  - Placeholder para card lists (próximo MVP)

- **BoardService**: CRUD completo (Get, Create, Update, Delete)

### 3️⃣ Routing & Layout
```
/auth/login          → LoginComponent
/auth/register       → RegisterComponent
/boards              → BoardsListComponent (protegido)
/boards/:id          → BoardDetailComponent (protegido)
```

### 4️⃣ Estilos
- Design responsivo e moderno
- Gradiente roxo/azul
- Cards com hover effects
- Form validations com mensagens de erro

## Como Testar Localmente

### 1. Certifique-se que a API está rodando
```powershell
docker compose up -d
```

### 2. Inicie o frontend
```powershell
cd basic-kanban-front
npm start
```

### 3. Abra no navegador
```
http://localhost:4200
```

### 4. Fluxo de Teste Completo

**Opção A: Novo Usuário**
1. Clique em "Sign up" na página de login
2. Preencha: Full Name, Email, Senha (min 8 chars, 1 uppercase, 1 digit)
3. Clique "Sign Up" → Redireciona para boards automaticamente

**Opção B: Usuário Existente**
1. Use email/senha válidos que já cadastrou
2. Clique "Login" → Redireciona para boards

**Uma vez logado:**
1. Veja a lista de boards
2. Clique "+ New Board" para criar um board
3. Digite título e descrição
4. Clique "Create"
5. Clique no board para ver detalhes
6. Clique "Delete" para remover (com confirmação)

## Estrutura do Código

```
src/
├── app/
│   ├── core/
│   │   ├── models/
│   │   │   ├── auth.model.ts
│   │   │   └── board.model.ts
│   │   ├── services/
│   │   │   ├── auth.service.ts
│   │   │   └── board.service.ts
│   │   ├── interceptors/
│   │   │   └── auth.interceptor.ts
│   │   └── guards/
│   │       └── auth.guard.ts
│   ├── features/
│   │   ├── auth/
│   │   │   ├── login.component.ts
│   │   │   └── register.component.ts
│   │   └── boards/
│   │       ├── boards-list.component.ts
│   │       └── board-detail.component.ts
│   ├── app.ts
│   ├── app.routes.ts
│   └── app.config.ts
├── environments/
│   └── environment.ts
└── main.ts
```

## Próximas Features (MVP 2)
- [ ] Card Lists (Colunas/Categorias)
- [ ] Cards (Tarefas)
- [ ] Drag & Drop entre listas
- [ ] Editar board
- [ ] Compartilhar board com outros usuários
- [ ] Board members/permissions

## Notas de Desenvolvimento
- Angular 17+ Standalone Components (sem módulos)
- Reactive Forms para validação
- RxJS para state management simples (BehaviorSubject)
- localStorage para persistência de JWT
- JWT Bearer token em Authorization header
