# Arquitectura · GymGo

## Clean Architecture

```
┌─────────────────────────────────────────────┐
│                GymGo.API                    │  ASP.NET Core, endpoints, middleware
├─────────────────────────────────────────────┤
│            GymGo.Infrastructure             │  EF Core, JWT, BCrypt, repos
├─────────────────────────────────────────────┤
│             GymGo.Application               │  Casos de uso (MediatR), validaciones
├─────────────────────────────────────────────┤
│               GymGo.Domain                  │  Entidades, VOs, eventos (sin deps)
└─────────────────────────────────────────────┘
```

Las flechas de dependencia apuntan **hacia adentro**: `API → Infrastructure → Application → Domain`. `Domain` no conoce a nadie. `Application` define interfaces (`IApplicationDbContext`, `ICurrentTenant`, `ICurrentUser`, `IPasswordHasher`, `IJwtTokenGenerator`) que `Infrastructure` implementa.

## Multi-tenancy

### Modelo
- Un **Tenant** = un gimnasio cliente del SaaS.
- Toda entidad de negocio implementa `ITenantScoped` y persiste su `TenantId`.
- `PlatformAdmin` es un usuario sin tenant (admin del SaaS).

### Resolución del tenant actual
`CurrentTenantService` lo resuelve, en orden:
1. Claim `tenant_id` del JWT.
2. Header `X-Tenant-Id` (login y endpoints públicos).
3. `null` (sólo PlatformAdmin o endpoints anónimos).

### Aislamiento en lectura
`ApplicationDbContext.OnModelCreating` aplica un `HasQueryFilter` **explícito por entidad** sobre cada `ITenantScoped`:

```csharp
modelBuilder.Entity<User>().HasQueryFilter(u =>
    (!CurrentHasTenant || u.TenantId == CurrentTenantIdOrEmpty)
    && !u.IsDeleted);
```

Sin tenant → no filtra (PlatformAdmin ve todo). Con tenant → sólo filas de ese tenant. `IsDeleted = 1` siempre se excluye.

> ⚠️ **Importante:** cuando agregues una nueva entidad `ITenantScoped` o `ISoftDeletable`, **declarar su `HasQueryFilter` manualmente en `ApplicationDbContext.OnModelCreating`**. No se aplica por convención: se hizo explícito porque las query filters dinámicas con `Expression.Constant(this)` rompen el caché de modelos de EF Core (el primer DbContext se queda capturado y los siguientes requests usan el tenant equivocado).

### Aislamiento en escritura
`TenantScopeSaveChangesInterceptor`:
- En `Added`: si `TenantId` está vacío, lo completa; si viene con uno distinto al actual, **lanza excepción** (defensa en profundidad).
- En `Modified`: bloquea cualquier intento de reasignar `TenantId`.

## Soft delete y auditoría

- `IAuditable` → `AuditableEntitySaveChangesInterceptor` completa `CreatedAtUtc/By` y `ModifiedAtUtc/By`.
- `ISoftDeletable` → mismo interceptor convierte `Deleted` en `Modified` con `IsDeleted=1`. El `HasQueryFilter` los excluye automáticamente.

> **Excepción — `ClassAttendance`:** esta entidad implementa `IAuditable` e `ITenantScoped` pero **no** `ISoftDeletable`. Los registros de asistencia son inmutables por diseño: una vez registrado un check-in no se borra físicamente ni lógicamente. Su `HasQueryFilter` en `ApplicationDbContext` sólo aplica el filtro de tenant, sin filtro de `IsDeleted`.

## Pipeline de MediatR

Todo Command/Query pasa por:

```
LoggingBehavior → ValidationBehavior → Handler
```

- **LoggingBehavior**: loggea inicio, fin, duración y errores.
- **ValidationBehavior**: ejecuta todos los `IValidator<TRequest>` y, si falla, lanza `FluentValidation.ValidationException` (traducida a HTTP 400 por `GlobalExceptionHandler`).

## Manejo de errores

`GlobalExceptionHandler` (registrado vía `AddExceptionHandler<>`) traduce excepciones a `ProblemDetails`:

| Excepción                              | HTTP |
|----------------------------------------|------|
| `FluentValidation.ValidationException` | 400  |
| `UnauthorizedAccessException`          | 401  |
| `NotFoundException`                    | 404  |
| `BusinessRuleViolationException`       | 422  |
| Cualquier otra                         | 500  |

## Logging

Serilog leído desde `appsettings.json`. Cada request enriquecido con `TenantId` y `UserId` vía `LogContext` en `TenantResolutionMiddleware`. Sinks: Console + File diario (retención 14 días en `logs/gymgo-YYYYMMDD.log`).

## Base de datos

**Sin EF Migrations**. Schema gestionado con scripts T-SQL versionados en `database/sql/`. Las configuraciones Fluent API (`IEntityTypeConfiguration<T>`) deben mantenerse alineadas con los scripts. Ver `database/README.md`.
