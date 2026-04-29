# GymGo SaaS

Plataforma **SaaS multi-tenant** para gestión de gimnasios con backend en **.NET 8** y frontend en **Angular (standalone + feature-based)**.

## 1) Arquitectura del sistema

### Backend (.NET 8 · Clean Architecture)

```text
src/
├── GymGo.Domain/         # Entidades, Value Objects, reglas y excepciones de dominio
├── GymGo.Application/    # Casos de uso (MediatR), DTOs, validación, contratos
├── GymGo.Infrastructure/ # EF Core, auth, implementación de repos/servicios
└── GymGo.API/            # Endpoints, middleware, DI, observabilidad
```

**Reglas clave:**
- `Domain` no depende de otras capas.
- `Application` define contratos y casos de uso.
- `Infrastructure` implementa contratos de `Application`.
- `API` solo orquesta (sin reglas de negocio pesadas).

### Frontend (Angular)

```text
frontend/gymgo-app/src/app/
├── core/      # Servicios transversales, guards, interceptors, modelos base
└── features/  # Módulos de negocio lazy-loaded
```

**Reglas clave:**
- Componentes standalone.
- `core/` no depende de `features/`.
- Auth y tenant headers centralizados en interceptor.

---

## 2) Multi-tenancy (criterio SaaS)

Cada request queda asociada a tenant por:
1. Claim `tenant_id` en JWT, o
2. Header `X-Tenant-Id` para flujos públicos (ej: login).

### Garantías esperadas
- Filtrado automático por tenant sobre entidades `ITenantScoped`.
- Interceptor de persistencia para asignar `TenantId` en altas y bloquear cruces de tenant.
- `PlatformAdmin` con visibilidad global; resto de roles acotados a tenant.

---

## 3) Estructura del repositorio

```text
gymgo-saas/
├── src/                    # Proyectos backend
├── frontend/gymgo-app/     # SPA Angular principal
├── tests/                  # Unit + Integration tests
├── database/sql/           # Scripts SQL versionados (schema + seed)
├── docs/                   # Documentación funcional y técnica
└── AGENTS.md               # Guía operativa para contribuciones
```

---

## 4) Stack tecnológico

| Capa | Tecnología |
|---|---|
| Backend | .NET 8, ASP.NET Core, MediatR, FluentValidation |
| Persistencia | EF Core 8 + SQL Server |
| Seguridad | JWT Bearer + BCrypt |
| Frontend | Angular (standalone), RxJS, Signals |
| Logging | Serilog |
| Testing | xUnit, FluentAssertions, Moq, WebApplicationFactory |

---

## 5) Setup local

## Prerrequisitos
- .NET SDK 8
- Node.js LTS + npm
- SQL Server (local/dev)

### 5.1 Backend

```bash
dotnet restore
dotnet build
```

### 5.2 Base de datos

Ejecutar scripts SQL en orden:

```text
database/sql/00_database/01_CreateDatabase.sql
database/sql/01_schema/01_Tenants.sql
database/sql/01_schema/02_Users.sql
database/sql/02_seed/01_DefaultData.sql
```

### 5.3 Secrets (desarrollo)

```bash
cd src/GymGo.API
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:Secret" "TU_SECRET_REAL_MIN_32_CHARS"
```

### 5.4 Ejecutar API

```bash
dotnet run --project src/GymGo.API
```

Endpoints típicos:
- Swagger: `https://localhost:<puerto>/swagger`
- Health: `https://localhost:<puerto>/health`

### 5.5 Frontend Angular

```bash
cd frontend/gymgo-app
npm install
npm run start
```

---

## 6) Calidad y validación

### Backend
```bash
dotnet test
```

### Frontend
```bash
cd frontend/gymgo-app
npm run lint
npm run test
npm run build
```

---

## 7) Convenciones operativas

- Mantener aislamiento multi-tenant como requisito no negociable.
- No introducir cambios de esquema sin script SQL asociado.
- Evitar lógica de negocio en controladores/endpoints/componentes de infraestructura.
- Documentar cambios de arquitectura en `docs/` y/o `AGENTS.md` cuando aplique.

---

## 8) Documentación complementaria

- `AGENTS.md`: guía operativa para cambios de código y seguridad SaaS.
- `docs/arquitectura/`: documentación técnica de backend/frontend.
- `database/README.md`: operación de base de datos y seed.
