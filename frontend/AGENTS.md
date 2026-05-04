# AGENTS.md

## 🎯 Project Overview
Enterprise SaaS application.

Stack:
- Backend: .NET (Clean Architecture)
- Frontend: Angular
- Auth: JWT
- Multi-tenant: Supported
- API: REST

Goals:
- Scalability
- Maintainability
- Security
- Low token usage for AI agents

---

# 🧱 Architecture (STRICT ENFORCEMENT)

## Layers

- Domain → Core business logic (Entities, ValueObjects, Enums)
- Application → Use cases, interfaces, DTOs
- Infrastructure → External concerns (DB, JWT, services)
- Presentation → Controllers (API layer)

## Hard Rules

- NEVER reference Infrastructure from Domain
- Application depends only on Domain
- Infrastructure implements Application interfaces
- Controllers must NOT contain business logic
- DO NOT bypass layers

---

# 🏢 Multi-Tenant Architecture

## Tenant Rules

- Every request MUST include tenant context
- TenantId is REQUIRED in:
  - Entities
  - Queries
  - Commands

## Isolation

- NEVER mix data between tenants
- Always filter by TenantId
- Validate tenant ownership before operations

## Agent Constraints

- DO NOT create global/shared data unless explicitly allowed
- DO NOT skip tenant validation

---

# 🔐 Authentication & Authorization

## JWT

- Required for all protected endpoints
- Use existing JwtTokenGenerator
- DO NOT create new auth systems

## Claims

Token MUST include:
- userId
- tenantId
- roles
- permissions (if implemented)

## Authorization

- Role-based OR permission-based
- Validate in Application layer (not only controller)

---

# 👥 Roles & Permissions

## Rules

- Prefer permissions over hardcoded roles
- Roles map to permissions
- Avoid logic like: if (role == "Admin")

## Agent Constraints

- DO NOT hardcode roles
- Reuse existing authorization services
- Centralize permission checks

---

# 📊 Auditing & Logging

## Required for:

- Create
- Update
- Delete
- Authentication events

## Audit Fields

- CreatedAt
- CreatedBy
- UpdatedAt
- UpdatedBy

## Rules

- Use centralized logging
- DO NOT log sensitive data (passwords, tokens)

---

# 📁 Backend (.NET)

## Commands

- build: dotnet build
- run: dotnet run
- test: dotnet test

## Coding Rules

- Use Dependency Injection
- Use async/await everywhere
- Avoid .Result / .Wait
- Prefer interfaces

## Entities

- Must include:
  - Id
  - TenantId (if applicable)
  - Audit fields

## DTOs

- NEVER expose domain entities
- Always map Entity → DTO

## Controllers

- Thin controllers only
- Delegate to Application layer

---

# 🌐 Frontend (Angular)

## Commands

- install: npm install
- run: ng serve
- build: ng build
- test: npm run test

## Architecture

- Services handle API calls
- Components handle UI only
- Use reactive forms

## Auth

- Store JWT in localStorage
- Use HTTP interceptor
- Handle 401/403 globally

---

# 🔄 API Contracts

## Rules

- DO NOT break existing contracts
- DO NOT rename fields without instruction
- Keep backward compatibility

---

# 🧪 Testing Strategy

## Backend

- Unit tests for Application layer
- Mock dependencies
- Avoid testing Infrastructure unless needed

## Frontend

- Test services and critical flows
- Avoid excessive UI testing

---

# 🚫 Forbidden Actions

- DO NOT refactor entire project
- DO NOT introduce new frameworks
- DO NOT duplicate logic
- DO NOT modify unrelated files
- DO NOT bypass architecture
- DO NOT ignore TenantId
- DO NOT create “quick fixes”

---

# 🔍 Code Discovery Strategy

Before coding:

1. Search for existing implementations
2. Reuse patterns
3. Follow naming conventions

---

# ⚡ Token Optimization Rules

- Work on specific files ONLY
- Avoid scanning full repo
- Prefer incremental updates
- Do not generate unnecessary comments
- Do not explain obvious code

---

# 🧩 Naming Conventions

## Backend

- Services: SomethingService
- Interfaces: ISomethingService
- DTOs: SomethingDto

## Frontend

- services: something.service.ts
- components: something.component.ts

---

# 🔁 Standard Workflows

## New Feature

1. Create UseCase (Application)
2. Define interface
3. Implement in Infrastructure
4. Expose via Controller
5. Connect Angular service
6. Build UI

## New Endpoint

- Validate input
- Call Application layer
- Return standardized response

---

# 🧠 Agent Behavior

- Think before coding
- Make small safe changes
- Validate build after changes
- Run tests if available

---

# 🛑 When Uncertain

- Prefer minimal safe implementation
- Do NOT guess architecture
- Document assumptions

---

# ✅ Definition of Done

- Code builds successfully
- Tests pass (if exist)
- No architecture violations
- Tenant isolation respected
- No unrelated changes