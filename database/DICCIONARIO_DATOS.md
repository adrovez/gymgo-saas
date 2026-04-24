# GymGo · Diccionario de datos

Documento de referencia de todas las tablas de la base `GymGoDb_Dev`. Cada vez que se agregue o modifique una tabla, **actualizar este archivo en el mismo PR/commit que el script SQL correspondiente**.

| Versión | Fecha       | Cambios                                              |
|---------|-------------|------------------------------------------------------|
| 1.0     | 2026-04-22  | Esquema inicial: `Tenants`, `Users`.                 |
| 1.1     | 2026-04-22  | Módulo de socios: tabla `Members`.                   |
| 1.2     | 2026-04-22  | Módulo de planes: tabla `MembershipPlans`.            |
| 1.3     | 2026-04-22  | Módulo de asignaciones: tabla `MembershipAssignments`.|

---

## Convenciones generales

- **Schema**: todas las tablas viven en `dbo`.
- **PK**: `Id UNIQUEIDENTIFIER` (Guid generado por la aplicación). No usamos `IDENTITY` para evitar coordinación entre tenants y facilitar inserciones cross-region.
- **Multi-tenancy**: toda tabla de negocio incluye `TenantId UNIQUEIDENTIFIER NOT NULL`. La aplicación filtra automáticamente por tenant vía `HasQueryFilter` y bloquea cross-tenant writes vía interceptor.
- **Auditoría**: las tablas con datos relevantes incluyen las columnas:
  - `CreatedAtUtc DATETIME2(3) NOT NULL`
  - `CreatedBy NVARCHAR(200) NULL`
  - `ModifiedAtUtc DATETIME2(3) NULL`
  - `ModifiedBy NVARCHAR(200) NULL`
- **Soft delete**: las tablas que no se borran físicamente incluyen:
  - `IsDeleted BIT NOT NULL DEFAULT 0`
  - `DeletedAtUtc DATETIME2(3) NULL`
  - `DeletedBy NVARCHAR(200) NULL`
- **Fechas**: siempre UTC, `DATETIME2(3)` (precisión de milisegundos, suficiente y eficiente).
- **Strings**: siempre `NVARCHAR` (Unicode) para soportar caracteres especiales del español y otros idiomas.
- **Booleans**: `BIT NOT NULL` con `DEFAULT` explícito.

---

## Índice de tablas

| Tabla                     | Propósito                                       | Multi-tenant | Soft delete |
|---------------------------|-------------------------------------------------|:------------:|:-----------:|
| [`dbo.Tenants`](#dbotenants) | Gimnasios clientes del SaaS                  |       —      |      —      |
| [`dbo.Users`](#dbousers)     | Usuarios del sistema (todos los roles)        |       ✓      |      ✓      |
| [`dbo.Members`](#dbomembers) | Socios del gimnasio                          |       ✓      |      ✓      |
| [`dbo.MembershipPlans`](#dbomembershipplans) | Planes de membresía disponibles |  ✓  |   ✓   |
| [`dbo.MembershipAssignments`](#dbomembershipassignments) | Contratos socio-plan con control de pago | ✓ | ✓ |

---

## `dbo.Tenants`

**Propósito de negocio.** Representa a cada **gimnasio cliente** del SaaS. Es la entidad raíz del aislamiento multi-tenant: todas las demás tablas de negocio referencian un `TenantId` que apunta acá. Un Tenant agrupa sus propios usuarios, planes, socios, equipos, clases, pagos, etc. Dar de baja un Tenant (`IsActive = 0`) deshabilita el acceso de todos sus usuarios sin perder los datos.

**Origen de datos.** Alta manual desde el panel de PlatformAdmin durante el onboarding del cliente. No se crea desde la app móvil ni por endpoints públicos.

### Columnas

| Columna         | Tipo            | Null | Default | Descripción                                                                 |
|-----------------|-----------------|:----:|---------|-----------------------------------------------------------------------------|
| `Id`            | UNIQUEIDENTIFIER| No   | —       | Identificador único del tenant. Generado por la aplicación (`Guid.NewGuid()`). |
| `Name`          | NVARCHAR(150)   | No   | —       | Nombre comercial del gimnasio (mostrado en UI). Ej: *"CrossFit Palermo"*.   |
| `Slug`          | NVARCHAR(60)    | No   | —       | Identificador en URLs / headers. Único en toda la BD. Sólo `[a-z0-9-]`. Ej: `crossfit-palermo`. |
| `ContactEmail`  | NVARCHAR(200)   | Sí   | —       | Email de contacto comercial / facturación.                                  |
| `ContactPhone`  | NVARCHAR(40)    | Sí   | —       | Teléfono de contacto, formato libre con código país.                        |
| `IsActive`      | BIT             | No   | `1`     | Si está en `0`, el tenant está suspendido y sus usuarios no pueden loguearse. |
| `CreatedAtUtc`  | DATETIME2(3)    | No   | —       | Cuándo se dio de alta el tenant.                                            |
| `CreatedBy`     | NVARCHAR(200)   | Sí   | —       | Usuario (email) que dio de alta. Típicamente un PlatformAdmin.              |
| `ModifiedAtUtc` | DATETIME2(3)    | Sí   | —       | Última modificación.                                                        |
| `ModifiedBy`    | NVARCHAR(200)   | Sí   | —       | Usuario que hizo la última modificación.                                    |

### Índices y constraints

| Nombre               | Tipo                  | Columnas         | Notas                                  |
|----------------------|-----------------------|------------------|----------------------------------------|
| `PK_Tenants`         | PRIMARY KEY clustered | `Id`             |                                        |
| `UX_Tenants_Slug`    | UNIQUE nonclustered   | `Slug`           | Garantiza unicidad del slug en toda la BD. |
| `DF_Tenants_IsActive`| DEFAULT               | `IsActive`       | Default `1`.                           |

### Relaciones

| Tipo      | Tabla destino | Columna(s) | Notas                                                                       |
|-----------|---------------|------------|-----------------------------------------------------------------------------|
| Saliente  | —             | —          | No referencia a otras tablas.                                               |
| Entrantes | `Users` (lógica) | `TenantId` | Sin FK física para permitir `TenantId = Guid.Empty` en PlatformAdmin (ver nota en `Users`). En tablas de negocio futuras (Members, Plans, etc.) se agregará FK física. |

---

## `dbo.Users`

**Propósito de negocio.** Representa a **toda persona que se autentica** en el sistema, sin importar su rol. Eso incluye:

- **PlatformAdmin** (rol `0`) — soporte interno de GymGo. No pertenece a ningún tenant (`TenantId = 00000000-0000-0000-0000-000000000000`).
- **GymOwner** (rol `1`) — dueño del gimnasio, máximo nivel dentro del tenant.
- **GymStaff** (rol `2`) — personal administrativo (recepción, caja).
- **Instructor** (rol `3`) — entrenadores que dictan clases.
- **Member** (rol `4`) — socios del gimnasio que usan la app móvil.

**Decisiones de diseño:**
- Un mismo email **puede repetirse en distintos tenants** (Juan Pérez puede ser socio en CrossFit Palermo y owner en Boxing Belgrano), pero es único dentro de un tenant. La unicidad la garantiza el índice `UX_Users_TenantId_Email`.
- `PasswordHash` guarda BCrypt (no la clave en claro, no SHA, no MD5).
- El borrado siempre es lógico (`IsDeleted = 1`) para preservar trazabilidad histórica de pagos, asistencias y movimientos.

**Origen de datos.** Alta desde:
- Endpoint de registro público (sólo rol `Member`, requiere header `X-Tenant-Id`).
- Panel del GymOwner/GymStaff (alta de `Instructor`, `GymStaff`, `Member` manuales).
- Panel del PlatformAdmin (alta de `GymOwner` durante onboarding y de otros `PlatformAdmin`).

### Columnas

| Columna         | Tipo            | Null | Default | Descripción                                                                                              |
|-----------------|-----------------|:----:|---------|----------------------------------------------------------------------------------------------------------|
| `Id`            | UNIQUEIDENTIFIER| No   | —       | Identificador único del usuario. Generado por la aplicación.                                             |
| `TenantId`      | UNIQUEIDENTIFIER| No   | —       | Tenant al que pertenece. Para `PlatformAdmin` vale `Guid.Empty` (sentinel "sin tenant").                 |
| `Email`         | NVARCHAR(200)   | No   | —       | Email del usuario, normalizado a minúsculas. Único por tenant entre usuarios no eliminados.              |
| `PasswordHash`  | NVARCHAR(300)   | No   | —       | Hash BCrypt (work factor 12) de la contraseña. **Nunca** guardar la clave en claro.                      |
| `FullName`      | NVARCHAR(200)   | No   | —       | Nombre completo del usuario, mostrado en UI.                                                             |
| `Role`          | INT             | No   | —       | Rol del usuario. Valores válidos: `0..4` (ver tabla más abajo). Mapeado al enum `UserRole`.              |
| `IsActive`      | BIT             | No   | `1`     | Si está en `0`, el usuario no puede loguearse (suspensión sin borrado).                                  |
| `LastLoginUtc`  | DATETIME2(3)    | Sí   | —       | Timestamp del último login exitoso. Útil para detectar cuentas inactivas.                                |
| `CreatedAtUtc`  | DATETIME2(3)    | No   | —       | Cuándo se creó el usuario.                                                                               |
| `CreatedBy`     | NVARCHAR(200)   | Sí   | —       | Email/usuario que creó esta cuenta. `seed` para datos iniciales del script de seed.                      |
| `ModifiedAtUtc` | DATETIME2(3)    | Sí   | —       | Última modificación.                                                                                     |
| `ModifiedBy`    | NVARCHAR(200)   | Sí   | —       | Quién hizo la última modificación.                                                                       |
| `IsDeleted`     | BIT             | No   | `0`     | Soft delete. Si `1`, el usuario no aparece en queries normales (filtro automático en EF Core).           |
| `DeletedAtUtc`  | DATETIME2(3)    | Sí   | —       | Cuándo se eliminó lógicamente.                                                                           |
| `DeletedBy`     | NVARCHAR(200)   | Sí   | —       | Quién hizo el borrado lógico.                                                                            |

### Valores válidos de `Role`

| Valor | Enum (`UserRole`) | Descripción                                                       | TenantId       |
|:-----:|-------------------|-------------------------------------------------------------------|----------------|
| `0`   | `PlatformAdmin`   | Soporte interno de GymGo. Acceso transversal a todos los tenants. | `Guid.Empty`   |
| `1`   | `GymOwner`        | Dueño del gimnasio. Permisos completos dentro de su tenant.       | obligatorio    |
| `2`   | `GymStaff`        | Personal administrativo (recepción, caja).                        | obligatorio    |
| `3`   | `Instructor`      | Entrenador / profesor de clases.                                  | obligatorio    |
| `4`   | `Member`          | Socio del gimnasio (acceso a la app móvil).                       | obligatorio    |

### Índices y constraints

| Nombre                       | Tipo                  | Columnas              | Notas                                                                          |
|------------------------------|-----------------------|-----------------------|--------------------------------------------------------------------------------|
| `PK_Users`                   | PRIMARY KEY clustered | `Id`                  |                                                                                |
| `UX_Users_TenantId_Email`    | UNIQUE nonclustered   | `TenantId`, `Email`   | Filtrado: `WHERE IsDeleted = 0`. Permite recrear un usuario con email re-usable tras borrado lógico. |
| `IX_Users_TenantId`          | NONCLUSTERED          | `TenantId`            | Acelera queries filtradas por tenant (la mayoría de las consultas).            |
| `CK_Users_Role`              | CHECK                 | `Role`                | `Role BETWEEN 0 AND 4`. Defensa contra valores fuera del enum.                 |
| `DF_Users_IsActive`          | DEFAULT               | `IsActive`            | Default `1`.                                                                   |
| `DF_Users_IsDeleted`         | DEFAULT               | `IsDeleted`           | Default `0`.                                                                   |

### Relaciones

| Tipo     | Tabla destino | Columna(s) | Notas                                                                                                                              |
|----------|---------------|------------|------------------------------------------------------------------------------------------------------------------------------------|
| Saliente | `Tenants` (lógica) | `TenantId` | Sin FK física porque `PlatformAdmin` usa `Guid.Empty` (un Guid que NO existe en `Tenants`). Se valida en código en `User.Create`. |

---

---

## `dbo.Members`

**Propósito de negocio.** Representa a cada **socio** del gimnasio. Un socio es la persona que contrata servicios del gimnasio (clases, planes, acceso a instalaciones). No es lo mismo que un `User`: un socio puede existir sin cuenta de acceso digital (dado de alta manualmente en recepción), y sólo tiene cuenta `User` si usa la app móvil.

**Relación con Users.** La vinculación `Member ↔ User` se hará en una iteración posterior (Sprint 4) cuando se implemente el acceso móvil. Por ahora `Members` es independiente.

**Origen de datos.** Alta desde:
- Panel del GymOwner/GymStaff: alta manual en recepción.
- (Futuro Sprint 4) App móvil: auto-registro del socio con validación de RUT.

### Columnas

| Columna                  | Tipo            | Null | Default | Descripción                                                                                                  |
|--------------------------|-----------------|:----:|---------|--------------------------------------------------------------------------------------------------------------|
| `Id`                     | UNIQUEIDENTIFIER| No   | —       | Identificador único del socio. Generado por la aplicación.                                                   |
| `TenantId`               | UNIQUEIDENTIFIER| No   | —       | Gimnasio al que pertenece el socio.                                                                          |
| `Rut`                    | NVARCHAR(20)    | No   | —       | RUT chileno normalizado: sin puntos, con guión y mayúscula. Ej: `"12345678-9"`. Único por tenant entre activos.|
| `FirstName`              | NVARCHAR(100)   | No   | —       | Nombre(s) del socio.                                                                                         |
| `LastName`               | NVARCHAR(100)   | No   | —       | Apellido(s) del socio.                                                                                       |
| `BirthDate`              | DATE            | No   | —       | Fecha de nacimiento. Sólo fecha, sin hora.                                                                   |
| `Gender`                 | INT             | No   | `0`     | Género: `0`=NoEspecificado, `1`=Masculino, `2`=Femenino, `3`=Otro.                                           |
| `Email`                  | NVARCHAR(200)   | Sí   | —       | Email de contacto del socio. Normalizado a minúsculas en la aplicación.                                      |
| `Phone`                  | NVARCHAR(40)    | Sí   | —       | Número de celular con código de país. Ej: `"+56912345678"`.                                                  |
| `Address`                | NVARCHAR(300)   | Sí   | —       | Dirección de domicilio completa.                                                                              |
| `EmergencyContactName`   | NVARCHAR(200)   | Sí   | —       | Nombre de la persona a contactar en caso de emergencia.                                                      |
| `EmergencyContactPhone`  | NVARCHAR(40)    | Sí   | —       | Teléfono del contacto de emergencia.                                                                         |
| `Status`                 | INT             | No   | `0`     | Estado operacional del socio. Ver tabla de valores más abajo.                                                |
| `RegistrationDate`       | DATE            | No   | —       | Fecha en que el socio fue dado de alta en el gimnasio. Puede diferir de `CreatedAtUtc`.                      |
| `Notes`                  | NVARCHAR(1000)  | Sí   | —       | Observaciones internas del staff. Campo libre.                                                               |
| `CreatedAtUtc`           | DATETIME2(3)    | No   | —       | Cuándo se creó el registro técnicamente.                                                                     |
| `CreatedBy`              | NVARCHAR(200)   | Sí   | —       | Email/usuario que creó el registro.                                                                          |
| `ModifiedAtUtc`          | DATETIME2(3)    | Sí   | —       | Última modificación técnica.                                                                                 |
| `ModifiedBy`             | NVARCHAR(200)   | Sí   | —       | Quién hizo la última modificación.                                                                           |
| `IsDeleted`              | BIT             | No   | `0`     | Soft delete. Si `1`, el socio no aparece en queries normales.                                                |
| `DeletedAtUtc`           | DATETIME2(3)    | Sí   | —       | Cuándo se eliminó lógicamente.                                                                               |
| `DeletedBy`              | NVARCHAR(200)   | Sí   | —       | Quién hizo el borrado lógico.                                                                                |

### Valores válidos de `Status`

| Valor | Enum (`MemberStatus`) | Descripción                                                           |
|:-----:|-----------------------|-----------------------------------------------------------------------|
| `0`   | `Active`              | Socio activo con membresía vigente.                                   |
| `1`   | `Suspended`           | Suspendido manualmente por el staff (ej. incumplimiento de reglamento).|
| `2`   | `Delinquent`          | Moroso. Cuotas impagas. Puede ser marcado automáticamente por el sistema de pagos (Sprint 3). |

### Valores válidos de `Gender`

| Valor | Enum (`Gender`)    | Descripción                     |
|:-----:|--------------------|---------------------------------|
| `0`   | `NotSpecified`     | No especificado (valor default).|
| `1`   | `Male`             | Masculino.                      |
| `2`   | `Female`           | Femenino.                       |
| `3`   | `Other`            | Otro / prefiero no indicar.     |

### Índices y constraints

| Nombre                       | Tipo                  | Columnas              | Notas                                                                                    |
|------------------------------|-----------------------|-----------------------|------------------------------------------------------------------------------------------|
| `PK_Members`                 | PRIMARY KEY clustered | `Id`                  |                                                                                          |
| `UX_Members_TenantId_Rut`    | UNIQUE nonclustered   | `TenantId`, `Rut`     | Filtrado: `WHERE IsDeleted = 0`. Permite re-dar de alta un socio dado de baja con el mismo RUT. |
| `IX_Members_TenantId`        | NONCLUSTERED          | `TenantId`            | Acelera queries filtradas por tenant.                                                    |
| `IX_Members_TenantId_Status` | NONCLUSTERED          | `TenantId`, `Status`  | Filtrado: `WHERE IsDeleted = 0`. Optimiza filtros por estado en el listado de socios.    |
| `CK_Members_Status`          | CHECK                 | `Status`              | `Status BETWEEN 0 AND 2`.                                                                |
| `CK_Members_Gender`          | CHECK                 | `Gender`              | `Gender BETWEEN 0 AND 3`.                                                                |
| `DF_Members_Status`          | DEFAULT               | `Status`              | Default `0` (Active).                                                                    |
| `DF_Members_Gender`          | DEFAULT               | `Gender`              | Default `0` (NotSpecified).                                                              |
| `DF_Members_IsDeleted`       | DEFAULT               | `IsDeleted`           | Default `0`.                                                                             |

### Relaciones

| Tipo     | Tabla destino | Columna(s)  | Notas                                                                                                        |
|----------|---------------|-------------|--------------------------------------------------------------------------------------------------------------|
| Saliente | `Tenants`     | `TenantId`  | FK física: `Members.TenantId → Tenants.Id`. El socio siempre pertenece a un tenant existente.                |
| Futura   | `Users`       | (pendiente) | Vinculación `Member ↔ User` para acceso móvil. Se modelará en Sprint 4 con una columna `UserId` nullable.   |
| Futura   | `MembershipAssignments` | (pendiente) | Asignación de planes de membresía. Se modelará en Sprint 2 siguiente iteración.            |

---

---

## `dbo.MembershipPlans`

**Propósito de negocio.** Define los **productos de membresía** que el gimnasio ofrece. Cada plan establece la periodicidad (mensual, trimestral, etc.), las condiciones de asistencia (días y horario) y el precio total. Un plan puede asignarse a múltiples socios a lo largo del tiempo. Desactivar un plan lo oculta para nuevas asignaciones pero no afecta a los socios que ya lo tienen activo.

**Origen de datos.** Alta y gestión exclusiva desde el panel del GymOwner/GymStaff.

### Columnas

| Columna           | Tipo          | Null | Default | Descripción                                                                                                                      |
|-------------------|---------------|:----:|---------|----------------------------------------------------------------------------------------------------------------------------------|
| `Id`              | UNIQUEIDENTIFIER| No  | —       | Identificador único del plan.                                                                                                    |
| `TenantId`        | UNIQUEIDENTIFIER| No  | —       | Gimnasio al que pertenece el plan.                                                                                               |
| `Name`            | NVARCHAR(150) | No   | —       | Nombre comercial. Ej: `"Plan Mensual Full"`, `"Plan Trimestral L-M-V Mañana"`.                                                  |
| `Description`     | NVARCHAR(500) | Sí   | —       | Descripción opcional visible al socio.                                                                                           |
| `Periodicity`     | INT           | No   | —       | Ciclo del plan. Ver tabla de valores más abajo.                                                                                  |
| `DurationDays`    | INT           | No   | —       | Duración en días. Derivado de `Periodicity`: 30 / 90 / 180 / 365. Se usa para calcular la fecha de vencimiento al asignar.      |
| `DaysPerWeek`     | INT           | No   | —       | Número referencial de días de asistencia por semana (1–7).                                                                      |
| `FixedDays`       | BIT           | No   | `0`     | `0`=días libres (socio elige dentro del límite). `1`=días fijos definidos en Monday–Sunday.                                     |
| `Monday`          | BIT           | No   | `0`     | Lunes habilitado. Solo aplica si `FixedDays=1`.                                                                                  |
| `Tuesday`         | BIT           | No   | `0`     | Martes habilitado.                                                                                                               |
| `Wednesday`       | BIT           | No   | `0`     | Miércoles habilitado.                                                                                                            |
| `Thursday`        | BIT           | No   | `0`     | Jueves habilitado.                                                                                                               |
| `Friday`          | BIT           | No   | `0`     | Viernes habilitado.                                                                                                              |
| `Saturday`        | BIT           | No   | `0`     | Sábado habilitado.                                                                                                               |
| `Sunday`          | BIT           | No   | `0`     | Domingo habilitado.                                                                                                              |
| `FreeSchedule`    | BIT           | No   | `1`     | `1`=acceso a cualquier hora. `0`=acceso sólo entre `TimeFrom` y `TimeTo`.                                                       |
| `TimeFrom`        | TIME          | Sí   | —       | Hora de inicio de acceso. Obligatoria si `FreeSchedule=0`.                                                                      |
| `TimeTo`          | TIME          | Sí   | —       | Hora de fin de acceso. Obligatoria si `FreeSchedule=0`. Debe ser posterior a `TimeFrom`.                                        |
| `Amount`          | DECIMAL(18,2) | No   | —       | Precio total del plan en CLP. Es el valor completo del período, no mensual.                                                     |
| `AllowsFreezing`  | BIT           | No   | `0`     | Si el socio puede pausar (congelar) temporalmente su plan.                                                                      |
| `IsActive`        | BIT           | No   | `1`     | `0`=plan desactivado, no disponible para nuevas asignaciones.                                                                   |
| `DeactivatedAtUtc`| DATETIME2(3)  | Sí   | —       | Cuándo se desactivó el plan.                                                                                                    |
| `CreatedAtUtc`    | DATETIME2(3)  | No   | —       | Cuándo se creó el plan.                                                                                                         |
| `CreatedBy`       | NVARCHAR(200) | Sí   | —       | Usuario que lo creó.                                                                                                            |
| `ModifiedAtUtc`   | DATETIME2(3)  | Sí   | —       | Última modificación.                                                                                                            |
| `ModifiedBy`      | NVARCHAR(200) | Sí   | —       | Quién modificó.                                                                                                                 |
| `IsDeleted`       | BIT           | No   | `0`     | Soft delete.                                                                                                                    |
| `DeletedAtUtc`    | DATETIME2(3)  | Sí   | —       | Cuándo se eliminó lógicamente.                                                                                                  |
| `DeletedBy`       | NVARCHAR(200) | Sí   | —       | Quién hizo el borrado lógico.                                                                                                   |

### Valores válidos de `Periodicity`

| Valor | Enum            | Descripción          | DurationDays |
|:-----:|-----------------|----------------------|:------------:|
| `1`   | `Monthly`       | Mensual              | 30           |
| `2`   | `Quarterly`     | Trimestral           | 90           |
| `3`   | `Biannual`      | Semestral            | 180          |
| `4`   | `Annual`        | Anual                | 365          |

### Ejemplos de configuración

| Plan                          | Periodicity | DaysPerWeek | FixedDays | Días         | FreeSchedule | Horario       |
|-------------------------------|:-----------:|:-----------:|:---------:|--------------|:------------:|---------------|
| Plan Mensual Full             | 1           | 7           | 0         | —            | 1            | Libre         |
| Plan Mensual 3x Semana        | 1           | 3           | 0         | —            | 1            | Libre         |
| Plan Trimestral L-M-V Mañana  | 2           | 3           | 1         | Lun, Mar, Vie| 0            | 07:00 – 10:00 |
| Plan Anual Full               | 4           | 7           | 0         | —            | 1            | Libre         |

### Índices y constraints

| Nombre                            | Tipo                  | Columnas              | Notas                                                                 |
|-----------------------------------|-----------------------|-----------------------|-----------------------------------------------------------------------|
| `PK_MembershipPlans`              | PRIMARY KEY clustered | `Id`                  |                                                                       |
| `IX_MembershipPlans_TenantId`     | NONCLUSTERED          | `TenantId`            | Filtrado por tenant.                                                  |
| `IX_MembershipPlans_TenantId_IsActive` | NONCLUSTERED     | `TenantId`, `IsActive`| Filtrado: `WHERE IsDeleted=0`. Optimiza listado de planes activos.    |
| `CK_MP_Periodicity`               | CHECK                 | `Periodicity`         | `BETWEEN 1 AND 4`.                                                    |
| `CK_MP_DaysPerWeek`               | CHECK                 | `DaysPerWeek`         | `BETWEEN 1 AND 7`.                                                    |
| `CK_MP_DurationDays`              | CHECK                 | `DurationDays`        | `> 0`.                                                                |
| `CK_MP_Amount`                    | CHECK                 | `Amount`              | `> 0`.                                                                |
| `CK_MP_TimeRange`                 | CHECK                 | `FreeSchedule`, `TimeFrom`, `TimeTo` | Si `FreeSchedule=0`, ambas horas presentes y `TimeFrom < TimeTo`. |

### Relaciones

| Tipo    | Tabla destino          | Columna(s) | Notas                                                                            |
|---------|------------------------|------------|----------------------------------------------------------------------------------|
| Saliente| `Tenants`              | `TenantId` | FK física: plan siempre pertenece a un tenant.                                   |
| Futura  | `MembershipAssignments`| (pendiente)| Una asignación referenciará el plan elegido para un socio específico.            |

---

---

## `dbo.MembershipAssignments`

**Propósito de negocio.** Registra la relación entre un socio y el plan que contrató, tanto la asignación vigente como el historial completo. Es la tabla central del ciclo de vida de una membresía: desde que se asigna, pasando por el control de pago, hasta que vence o se cancela.

**Decisiones de diseño:**
- `AmountSnapshot` congela el precio al momento de la asignación. Si el gimnasio sube el precio del plan, las asignaciones ya existentes no se ven afectadas.
- Un socio puede tener una sola asignación en estado `Active` o `Frozen` a la vez. El historial de asignaciones anteriores se conserva.
- `FrozenDaysAccumulated` permite múltiples ciclos de congelamiento; cada descongelamiento extiende `EndDate`.

### Columnas

| Columna                | Tipo            | Null | Default | Descripción                                                                                                   |
|------------------------|-----------------|:----:|---------|---------------------------------------------------------------------------------------------------------------|
| `Id`                   | UNIQUEIDENTIFIER| No   | —       | Identificador único de la asignación.                                                                         |
| `TenantId`             | UNIQUEIDENTIFIER| No   | —       | Gimnasio al que pertenece. FK a `Tenants`.                                                                    |
| `MemberId`             | UNIQUEIDENTIFIER| No   | —       | Socio que recibe el plan. FK a `Members`.                                                                     |
| `MembershipPlanId`     | UNIQUEIDENTIFIER| No   | —       | Plan contratado. FK a `MembershipPlans`.                                                                      |
| `StartDate`            | DATE            | No   | —       | Inicio de la membresía.                                                                                       |
| `EndDate`              | DATE            | No   | —       | Vencimiento calculado: `StartDate + DurationDays`. Se extiende al descongelar.                                |
| `AmountSnapshot`       | DECIMAL(18,2)   | No   | —       | Precio del plan al momento de la asignación en CLP. Inmutable.                                               |
| `Status`               | INT             | No   | `0`     | Estado operacional. Ver tabla de valores.                                                                     |
| `PaymentStatus`        | INT             | No   | `0`     | Estado de pago. Ver tabla de valores.                                                                         |
| `PaidAtUtc`            | DATETIME2(3)    | Sí   | —       | Cuándo se registró el pago. Null si aún no se pagó.                                                          |
| `FrozenSince`          | DATE            | Sí   | —       | Inicio del congelamiento activo. Null si no está congelada.                                                   |
| `FrozenDaysAccumulated`| INT             | No   | `0`     | Total de días congelados (sumados a `EndDate` al descongelar).                                                |
| `Notes`                | NVARCHAR(500)   | Sí   | —       | Observaciones internas.                                                                                       |
| `CreatedAtUtc`         | DATETIME2(3)    | No   | —       | Cuándo se creó la asignación.                                                                                 |
| `CreatedBy`            | NVARCHAR(200)   | Sí   | —       | Usuario que realizó la asignación.                                                                            |
| `ModifiedAtUtc`        | DATETIME2(3)    | Sí   | —       | Última modificación.                                                                                          |
| `ModifiedBy`           | NVARCHAR(200)   | Sí   | —       | Quién modificó.                                                                                               |
| `IsDeleted`            | BIT             | No   | `0`     | Soft delete.                                                                                                  |
| `DeletedAtUtc`         | DATETIME2(3)    | Sí   | —       | Cuándo se eliminó lógicamente.                                                                                |
| `DeletedBy`            | NVARCHAR(200)   | Sí   | —       | Quién hizo el borrado lógico.                                                                                 |

### Valores válidos de `Status`

| Valor | Enum          | Descripción                                      |
|:-----:|---------------|--------------------------------------------------|
| `0`   | `Active`      | Membresía vigente.                               |
| `1`   | `Expired`     | Período vencido sin renovación.                  |
| `2`   | `Cancelled`   | Cancelada manualmente antes del vencimiento.     |
| `3`   | `Frozen`      | Pausada (solo si el plan lo permite).            |

### Valores válidos de `PaymentStatus`

| Valor | Enum      | Descripción                                                         |
|:-----:|-----------|---------------------------------------------------------------------|
| `0`   | `Pending` | Asignada, pago no registrado aún.                                   |
| `1`   | `Paid`    | Pago confirmado.                                                    |
| `2`   | `Overdue` | Morosa. Gatilla `Member.Status = Delinquent` automáticamente.       |

### Índices y constraints

| Nombre                                      | Tipo        | Columnas                       | Notas                                                       |
|---------------------------------------------|-------------|--------------------------------|-------------------------------------------------------------|
| `PK_MembershipAssignments`                  | PRIMARY KEY | `Id`                           |                                                             |
| `FK_MembershipAssignments_Members`          | FK          | `MemberId → Members.Id`        |                                                             |
| `FK_MembershipAssignments_MembershipPlans`  | FK          | `MembershipPlanId → MembershipPlans.Id` |                                                  |
| `FK_MembershipAssignments_Tenants`          | FK          | `TenantId → Tenants.Id`        |                                                             |
| `IX_MembershipAssignments_TenantId`         | NONCLUSTERED| `TenantId`                     | Filtrado por tenant.                                        |
| `IX_MembershipAssignments_MemberId`         | NONCLUSTERED| `MemberId`                     | Historial por socio.                                        |
| `IX_MembershipAssignments_MemberId_Status`  | NONCLUSTERED| `MemberId`, `Status`           | Filtrado: `WHERE IsDeleted=0`. Query de membresía activa.   |
| `IX_MembershipAssignments_TenantId_PaymentStatus` | NONCLUSTERED | `TenantId`, `PaymentStatus` | Filtrado: `WHERE IsDeleted=0`. Listado de morosos.    |
| `CK_MA_Status`                              | CHECK       | `Status`                       | `BETWEEN 0 AND 3`.                                          |
| `CK_MA_PaymentStatus`                       | CHECK       | `PaymentStatus`                | `BETWEEN 0 AND 2`.                                          |
| `CK_MA_DateRange`                           | CHECK       | `StartDate`, `EndDate`         | `EndDate > StartDate`.                                      |
| `CK_MA_AmountSnapshot`                      | CHECK       | `AmountSnapshot`               | `> 0`.                                                      |

---

## Apéndice: tipos canónicos

Para mantener consistencia entre tablas, usar los siguientes tipos según el dominio:

| Dominio                   | Tipo SQL          | Notas                                            |
|---------------------------|-------------------|--------------------------------------------------|
| Identificador (PK/FK)     | `UNIQUEIDENTIFIER`| Generado por aplicación, no `NEWID()` server-side. |
| Nombre / título corto     | `NVARCHAR(150)`   |                                                  |
| Nombre completo / razón   | `NVARCHAR(200)`   |                                                  |
| Descripción larga         | `NVARCHAR(1000)`  |                                                  |
| Email                     | `NVARCHAR(200)`   | Normalizado a lowercase en aplicación.            |
| Teléfono                  | `NVARCHAR(40)`    | Formato libre con código país.                   |
| Slug / código             | `NVARCHAR(60)`    | Sólo `[a-z0-9-]`.                                |
| Hash BCrypt               | `NVARCHAR(300)`   |                                                  |
| Booleano                  | `BIT NOT NULL`    | Siempre con DEFAULT explícito.                   |
| Monto / dinero            | `DECIMAL(18, 2)`  | Cuando se modele `Plans`/`Payments`.             |
| Fecha + hora UTC          | `DATETIME2(3)`    | Precisión de milisegundos.                       |
| Sólo fecha                | `DATE`            |                                                  |
| Enum                      | `INT NOT NULL` + `CHECK` | Persistir como int, validar rango.        |

---

## Cómo extender este documento

Cuando agregues una nueva tabla:

1. Crear el script en `database/sql/01_schema/NN_NombreTabla.sql`.
2. Crear/actualizar el `IEntityTypeConfiguration<T>` en `src/GymGo.Infrastructure/Persistence/Configurations/`.
3. Agregar la tabla al **Índice de tablas** de este documento.
4. Agregar la sección de la tabla con: propósito de negocio, origen de datos, columnas, índices/constraints, relaciones.
5. Si introducís un tipo de columna nuevo, agregarlo al **Apéndice de tipos canónicos**.
6. Bumpear la versión en la tabla de versiones del header.
