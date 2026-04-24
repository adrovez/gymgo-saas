# GymGo · SaaS multi-tenant para gestión de gimnasios

Backend en **.NET 8** con **Clean Architecture**, **SQL Server** y **multi-tenancy** por `HasQueryFilter`.

## Estructura

```
gymgo-saas/
├── src/
│   ├── GymGo.Domain/           Entidades, Value Objects, eventos de dominio (sin dependencias)
│   ├── GymGo.Application/      Casos de uso (MediatR), validación (FluentValidation), abstracciones
│   ├── GymGo.Infrastructure/   EF Core, repositorios, JWT, BCrypt, servicios de tenant/usuario
│   └── GymGo.API/              ASP.NET Core, endpoints, middleware, Swagger, Serilog
├── tests/
│   ├── GymGo.UnitTests/        xUnit + FluentAssertions
│   └── GymGo.IntegrationTests/ WebApplicationFactory + EF InMemory
├── database/
│   └── sql/                    Scripts T-SQL versionados (ejecución manual). Ver database/README.md
├── docs/                       Documentación de arquitectura y workflow
├── backlog/                    Documentos de planificación (Sprint 0, propuesta)
├── .azure-pipelines/           Pipelines de Azure DevOps (a configurar después)
└── GymGo.slnx                  Solution
```

## Stack

| Capa            | Tecnología                                                |
|-----------------|-----------------------------------------------------------|
| Lenguaje        | C# 12 (.NET 8 LTS)                                        |
| API             | ASP.NET Core 8 Minimal APIs + Controllers                 |
| ORM             | Entity Framework Core 8 + SQL Server provider             |
| BD              | SQL Server 2022 (LocalDB en desarrollo)                   |
| Auth            | JWT Bearer + BCrypt                                       |
| Mediator        | MediatR + FluentValidation + Mapster                      |
| Logs            | Serilog (Console + File)                                  |
| Tests           | xUnit + FluentAssertions + Moq + WebApplicationFactory    |

## Setup local (primera vez)

### 1. Restaurar paquetes y compilar

```powershell
cd C:\Adrovez\DevAzure\gymgo-saas
dotnet restore
dotnet build
```

### 2. Crear y poblar la base de datos local

Ver [`database/README.md`](database/README.md). En resumen, ejecutar en orden:

```
database/sql/00_database/01_CreateDatabase.sql
database/sql/01_schema/01_Tenants.sql
database/sql/01_schema/02_Users.sql
database/sql/02_seed/01_DefaultData.sql
```

> ⚠️ Antes de loguearte por primera vez, regenerá los hashes BCrypt del seed (instrucciones en `database/README.md`).

### 3. Configurar secrets de desarrollo

El `appsettings.Development.json` trae un JWT secret de juguete. Para no commitearlo, podés moverlo a User Secrets:

```powershell
cd src\GymGo.API
dotnet user-secrets init
dotnet user-secrets set "JwtSettings:Secret" "TU_SECRET_REAL_DE_AL_MENOS_32_CHARS"
```

### 4. Correr la API

```powershell
dotnet run --project src\GymGo.API
```

- Swagger UI: `https://localhost:7xxx/swagger`
- Health: `https://localhost:7xxx/health`
- Ping: `https://localhost:7xxx/api/v1/ping`

### 5. Correr los tests

```powershell
dotnet test
```

## Multi-tenancy

Cada request se asocia a un tenant a través de:
1. El claim `tenant_id` del JWT (cuando hay autenticación), **o**
2. El header `X-Tenant-Id` (para endpoints públicos como login)

`ApplicationDbContext` aplica un `HasQueryFilter` automático sobre toda entidad `ITenantScoped` para filtrar por el tenant actual. El `TenantScopeSaveChangesInterceptor` asegura que toda inserción lleve el `TenantId` correcto y bloquea cross-tenant updates.

`PlatformAdmin` (rol 0) no tiene tenant; ve y administra todos.

## Convenciones

- **Excepciones de dominio** (`DomainException`, `NotFoundException`, `BusinessRuleViolationException`) se traducen a HTTP en `GlobalExceptionHandler`.
- **Logs estructurados** con Serilog. Cada request se enriquece con `TenantId` y `UserId` vía `LogContext`.
- **Schema de BD** se gestiona con scripts T-SQL en `database/sql/`, no con `dotnet ef migrations`.
- **Soft delete**: implementar `ISoftDeletable` en la entidad; el interceptor convierte `DELETE` en `UPDATE IsDeleted=1` y el `HasQueryFilter` los excluye.
