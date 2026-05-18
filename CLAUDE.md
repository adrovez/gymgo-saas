# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

### Backend
```bash
dotnet restore
dotnet build
dotnet run --project src/GymGo.API

# Run all tests
dotnet test

# Run a single test project
dotnet test tests/GymGo.UnitTests
dotnet test tests/GymGo.IntegrationTests

# Run a specific test class or method
dotnet test --filter "FullyQualifiedName~MemberTests"
```

### Frontend
```bash
cd frontend/gymgo-app
npm install
npm run start    # dev server at http://localhost:4200
npm run build
npm run lint
npm run test
```

### Database setup (run in order)
```
database/sql/00_database/01_CreateDatabase.sql
database/sql/01_schema/01_Tenants.sql
database/sql/01_schema/02_Users.sql
database/sql/01_schema/03_Members.sql
database/sql/01_schema/04_MembershipPlans.sql
database/sql/01_schema/05_MembershipAssignments.sql
database/sql/01_schema/06_GymClasses.sql
database/sql/01_schema/07_ClassAttendances.sql
database/sql/01_schema/08_GymEntries.sql
database/sql/01_schema/08b_GymEntries_AddExitedAt.sql
database/sql/01_schema/09_ClassReservations.sql
database/sql/01_schema/10_Equipment.sql
database/sql/01_schema/11_MaintenanceRecords.sql
database/sql/01_schema/13_WorkoutRoutines.sql   ← replaces 12 (drops old tables first)
database/sql/02_seed/01_DefaultData.sql
```

### JWT secret (development only)
```bash
cd src/GymGo.API
dotnet user-secrets set "JwtSettings:Secret" "<min-32-char-secret>"
```

## Architecture

### Backend — Clean Architecture (.NET 8)

```
src/
├── GymGo.Domain/         # Entities, value objects, domain exceptions, interfaces
├── GymGo.Application/    # MediatR commands/queries, DTOs, FluentValidation, contracts
├── GymGo.Infrastructure/ # EF Core, JWT, BCrypt, tenant/user services
└── GymGo.API/            # Minimal API endpoints, middleware, DI wiring
```

**Dependency direction:** Domain ← Application ← Infrastructure ← API. Never skip layers or call Infrastructure from API directly.

**CQRS with MediatR — vertical slices.** Every feature lives in `Application/<Feature>/Commands/<CommandName>/` or `Application/<Feature>/Queries/<QueryName>/` with three files: `*Command.cs` (record), `*CommandValidator.cs` (FluentValidation), `*CommandHandler.cs`. Endpoints dispatch via `ISender.Send()` — no business logic in endpoint classes.

**Minimal API endpoints** are registered as extension methods on `IEndpointRouteBuilder` (see `MemberEndpoints.cs`) and wired in `Program.cs`. All endpoints call `RequireAuthorization()` by default.

**MediatR pipeline behaviors** (ordered): `ValidationBehavior` → `LoggingBehavior`.

### Multi-tenancy

`ICurrentTenant` resolves `TenantId` from:
1. JWT claim `tenant_id` (authenticated requests)
2. `X-Tenant-Id` request header (public flows like login)
3. `null` — PlatformAdmin global scope

Every tenant-scoped entity implements `ITenantScoped` (exposes `Guid TenantId`). `ApplicationDbContext` applies `HasQueryFilter` per entity so all queries are automatically filtered. A `TenantScopeSaveChangesInterceptor` stamps `TenantId` on insert. **Never bypass these filters** without explicit, validated justification.

Entities with `ISoftDeletable` are filtered by `!IsDeleted`. Audit-trail entities (GymEntry, ClassAttendance, ClassReservation, MaintenanceRecord) are immutable — no soft delete.

`PlatformAdmin` role has global visibility (`CurrentHasTenant == false`); all other roles are scoped to their tenant.

### Database evolution

No EF migrations — schema is managed via versioned SQL scripts in `database/sql/`. When adding or changing a persisted model: update EF entity configuration in `Infrastructure/Persistence/Configurations/` **and** add an incremental SQL script under `database/sql/01_schema/` or `database/sql/03_alterations/`.

### Frontend — Angular (standalone, feature-based)

```
frontend/gymgo-app/src/app/
├── core/        # AuthService, StorageService, AuthInterceptor, AuthGuard, validators
└── features/    # One folder per domain (members, classes, assignments, …)
    └── <feature>/
        ├── models/        # TypeScript interfaces
        ├── services/      # HTTP calls, returns Observables
        └── <component>/   # Standalone component (.ts + .html + .scss)
```

All routes under `/app/*` are protected by `authGuard`. Every feature route is lazy-loaded (`loadComponent`). `AuthInterceptor` attaches `Authorization: Bearer` and `X-Tenant-Id` headers automatically.

Use Signals for local component state, reactive forms for forms, strict typing — avoid `any`.

## Adding a new feature (checklist)

1. **Domain** — entity in `src/GymGo.Domain/<Feature>/`, implement `ITenantScoped` + optionally `ISoftDeletable`.
2. **Application** — commands and/or queries with validators and handlers; DTOs and mappings.
3. **Infrastructure** — EF `IEntityTypeConfiguration<T>` in `Persistence/Configurations/`; add `DbSet<T>` to `ApplicationDbContext`; add tenant `HasQueryFilter`.
4. **API** — `*Endpoints.cs` static class; wire in `Program.cs`.
5. **Database** — SQL script for new/altered table.
6. **Frontend** — model interface, service, components under `features/<feature>/`; add route to `app.routes.ts`.
7. **Tests** — unit tests for domain and handlers; integration test if endpoint is non-trivial.

## Módulo Rutinas de Entrenamiento

### Modelo de datos (5 tablas, script 13)

```
WorkoutPlans              → Rutina asignada al socio
  ├─ Objective, StartDate, EndDate
  ├─ InitialWeightKg, InitialHeightCm, InitialBodyFatPercentage
  ├─ Status: 0=Active, 1=Completed, 2=Cancelled
  └─ WorkoutPlanDays      → Días de la semana (DayOfWeek: 1=Lunes…7=Domingo)
       └─ WorkoutPlanExercises → Ejercicios planificados
            PlannedSets, PlannedReps, PlannedWeightKg,
            PlannedDurationMinutes, PlannedDistanceMeters

WorkoutLogs               → Sesión real del socio
  ├─ WorkoutPlanId + WorkoutPlanDayId (qué día de la rutina realizó)
  ├─ Date (fecha real de la sesión)
  ├─ Status: 0=Draft, 1=Completed
  └─ WorkoutLogExercises  → Ejercicios realizados
       ActualSets, ActualReps, ActualWeightKg,
       ActualDurationMinutes, ActualDistanceMeters,
       WorkoutPlanExerciseId (nullable — null si IsExtra=true)
       IsExtra (ejercicio no planificado agregado en el momento)
```

### Reglas de negocio del módulo

- Un socio solo puede tener **una rutina activa** a la vez (`WorkoutPlanStatus.Active`).
- Los días de la rutina se identifican por **día de semana** (`WorkoutDayOfWeek`), únicos por rutina.
- Al registrar una sesión, el socio elige qué día de la rutina realizó (puede hacerlo cualquier día del calendario).
- Los ejercicios del log referencian los ejercicios planificados para comparar planificado vs real.
- `IsExtra = true` para ejercicios que el socio hizo sin estar en el plan.

### Estructura de archivos backend

```
Domain/WorkoutLogs/
  WorkoutPlan.cs, WorkoutPlanDay.cs, WorkoutPlanExercise.cs
  WorkoutLog.cs, WorkoutLogExercise.cs
  WorkoutPlanStatus.cs, WorkoutDayOfWeek.cs, WorkoutLogStatus.cs, MuscleGroup.cs

Infrastructure/Persistence/Configurations/
  WorkoutPlanConfiguration.cs, WorkoutPlanDayConfiguration.cs
  WorkoutPlanExerciseConfiguration.cs
  WorkoutLogConfiguration.cs, WorkoutLogExerciseConfiguration.cs

Application/WorkoutPlans/    ← Commands: Create/Update/Delete Plan, Add/Remove Day, Add/Update/Remove PlanExercise
Application/WorkoutLogs/     ← Commands: Create/Update/Delete Log, Add/Update/Remove LogExercise, Complete

API/Endpoints/
  WorkoutPlanEndpoints.cs    ← /api/v1/workout-plans/*
  WorkoutLogEndpoints.cs     ← /api/v1/workout-logs/*
```

### API endpoints del módulo

| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/v1/workout-plans` | Crear rutina |
| GET | `/api/v1/workout-plans?memberId=&status=` | Listar rutinas |
| GET | `/api/v1/workout-plans/{id}` | Detalle con días y ejercicios |
| PUT | `/api/v1/workout-plans/{id}` | Editar cabecera |
| DELETE | `/api/v1/workout-plans/{id}` | Eliminar (soft) |
| POST | `/api/v1/workout-plans/{id}/days` | Agregar día |
| DELETE | `/api/v1/workout-plans/{id}/days/{dayId}` | Eliminar día |
| POST | `/api/v1/workout-plans/days/{dayId}/exercises` | Agregar ejercicio al día |
| PUT | `/api/v1/workout-plans/days/{dayId}/exercises/{exId}` | Editar ejercicio |
| DELETE | `/api/v1/workout-plans/days/{dayId}/exercises/{exId}` | Eliminar ejercicio |
| POST | `/api/v1/workout-logs` | Registrar sesión (necesita planId + dayId) |
| GET | `/api/v1/workout-logs?memberId=&workoutPlanId=&from=&to=` | Historial |
| GET | `/api/v1/workout-logs/{id}` | Detalle con ejercicios |
| PUT | `/api/v1/workout-logs/{id}` | Editar notas |
| PATCH | `/api/v1/workout-logs/{id}/complete` | Completar sesión |
| DELETE | `/api/v1/workout-logs/{id}` | Eliminar |
| POST | `/api/v1/workout-logs/{id}/exercises` | Registrar ejercicio |
| PUT | `/api/v1/workout-logs/{id}/exercises/{exId}` | Editar ejercicio |
| DELETE | `/api/v1/workout-logs/{id}/exercises/{exId}` | Eliminar ejercicio |

---

## Key conventions

- Logs must include `TenantId`, `UserId`, `CorrelationId` — use Serilog enrichment, never log secrets or full payloads.
- Domain errors (`DomainException`, `NotFoundException`, `BusinessRuleViolationException`) are translated to HTTP responses by `GlobalExceptionHandler`.
- All IO is async end-to-end (`async`/`await`, `CancellationToken` propagated).
- RUT validation lives in `core/validators/rut.validator.ts` (frontend) — reuse it.
