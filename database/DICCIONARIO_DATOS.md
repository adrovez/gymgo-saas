# GymGo · Diccionario de datos

Documento de referencia de todas las tablas de la base `GymGoDb_Dev`. Cada vez que se agregue o modifique una tabla, **actualizar este archivo en el mismo PR/commit que el script SQL correspondiente**.

| Versión | Fecha       | Cambios                                              |
|---------|-------------|------------------------------------------------------|
| 1.0     | 2026-04-22  | Esquema inicial: `Tenants`, `Users`.                 |
| 1.1     | 2026-04-22  | Módulo de socios: tabla `Members`.                   |
| 1.2     | 2026-04-22  | Módulo de planes: tabla `MembershipPlans`.            |
| 1.3     | 2026-04-22  | Módulo de asignaciones: tabla `MembershipAssignments`.|
| 1.4     | 2026-04-27  | Módulo de clases: tabla `GymClasses`.                |
| 1.5     | 2026-04-27  | Módulo de clases: tabla `ClassSchedules`.            |
| 1.6     | 2026-04-27  | Módulo de asistencia: tabla `ClassAttendances` (check-in manual y QR). |
| 1.7     | 2026-04-27  | Módulo de ingreso: tabla `GymEntries`.               |
| 1.8     | 2026-04-27  | Módulo de reservas: tabla `ClassReservations`.       |
| 1.9     | 2026-04-28  | Módulo de maquinaria: tabla `Equipment`.             |
| 2.0     | 2026-04-28  | Módulo de mantención: tabla `MaintenanceRecords`.    |

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
| [`dbo.GymClasses`](#dbogymclasses) | Catálogo de tipos de clase del gimnasio | ✓ | ✓ |
| [`dbo.ClassSchedules`](#dboclassschedules) | Horarios semanales recurrentes de cada clase | ✓ | ✓ |
| [`dbo.ClassAttendances`](#dboclassattendances) | Registro de check-in de socios a sesiones de clase | ✓ | — |
| [`dbo.GymEntries`](#dbogympentries) | Registro de ingresos al gimnasio (acceso a instalaciones) | ✓ | — |
| [`dbo.ClassReservations`](#dboclassreservations) | Reservas de socios a sesiones de clase | ✓ | — |
| [`dbo.Equipment`](#dboequipment) | Catálogo de maquinaria del gimnasio | ✓ | ✓ |
| [`dbo.MaintenanceRecords`](#dbomaintenancerecords) | Mantenciones preventivas y correctivas de maquinaria | ✓ | — |

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

---

## `dbo.GymClasses`

**Propósito de negocio.** Es el **catálogo de tipos de clase** que ofrece el gimnasio (Yoga, Spinning, Box Funcional, etc.). Funciona como plantilla de la que se generan los horarios semanales recurrentes (`ClassSchedules`). Desactivar una clase la oculta para nuevos horarios pero no elimina los ya existentes.

**Origen de datos.** Alta y gestión exclusiva desde el panel del GymOwner/GymStaff.

### Columnas

| Columna           | Tipo            | Null | Default | Descripción                                                                                           |
|-------------------|-----------------|:----:|---------|-------------------------------------------------------------------------------------------------------|
| `Id`              | UNIQUEIDENTIFIER| No   | —       | Identificador único de la clase. Generado por la aplicación.                                          |
| `TenantId`        | UNIQUEIDENTIFIER| No   | —       | Gimnasio al que pertenece.                                                                            |
| `Name`            | NVARCHAR(100)   | No   | —       | Nombre de la clase. Ej: `"Yoga"`, `"Spinning"`, `"Box Funcional"`.                                    |
| `Description`     | NVARCHAR(500)   | Sí   | —       | Descripción opcional visible al socio.                                                                |
| `Category`        | INT             | No   | `0`     | Categoría de la clase. Ver tabla de valores más abajo.                                                |
| `Color`           | NVARCHAR(7)     | Sí   | —       | Color hex para el calendario de clases. Ej: `"#3B82F6"`. Siempre empieza con `#`.                    |
| `DurationMinutes` | INT             | No   | `60`    | Duración estándar en minutos. Debe ser mayor a cero.                                                  |
| `MaxCapacity`     | INT             | No   | `20`    | Capacidad máxima estándar. Puede sobreescribirse por horario en `ClassSchedules.MaxCapacity`.         |
| `IsActive`        | BIT             | No   | `1`     | `0` = clase desactivada, no disponible para nuevos horarios.                                          |
| `CreatedAtUtc`    | DATETIME2(3)    | No   | —       | Cuándo se creó la clase.                                                                              |
| `CreatedBy`       | NVARCHAR(200)   | Sí   | —       | Usuario que la creó.                                                                                  |
| `ModifiedAtUtc`   | DATETIME2(3)    | Sí   | —       | Última modificación.                                                                                  |
| `ModifiedBy`      | NVARCHAR(200)   | Sí   | —       | Quién modificó.                                                                                       |
| `IsDeleted`       | BIT             | No   | `0`     | Soft delete.                                                                                          |
| `DeletedAtUtc`    | DATETIME2(3)    | Sí   | —       | Cuándo se eliminó lógicamente.                                                                        |
| `DeletedBy`       | NVARCHAR(200)   | Sí   | —       | Quién hizo el borrado lógico.                                                                         |

### Valores válidos de `Category`

| Valor | Enum         | Descripción                          |
|:-----:|--------------|--------------------------------------|
| `0`   | `Other`      | Otro / sin categoría (default).      |
| `1`   | `Cardio`     | Cardio / aeróbico.                   |
| `2`   | `Strength`   | Fuerza / musculación.                |
| `3`   | `Flexibility`| Flexibilidad / movilidad.            |
| `4`   | `Martial`    | Artes marciales / combate.           |
| `5`   | `Dance`      | Baile.                               |
| `6`   | `Aquatic`    | Acuático.                            |
| `7`   | `Mind`       | Mente y cuerpo (Yoga, Pilates…).     |

### Índices y constraints

| Nombre                              | Tipo        | Columnas                    | Notas                                                              |
|-------------------------------------|-------------|-----------------------------|--------------------------------------------------------------------|
| `PK_GymClasses`                     | PRIMARY KEY | `Id`                        |                                                                    |
| `FK_GymClasses_Tenants`             | FK          | `TenantId → Tenants.Id`     |                                                                    |
| `IX_GymClasses_TenantId`            | NONCLUSTERED| `TenantId`                  | Filtrado por tenant.                                               |
| `IX_GymClasses_TenantId_IsActive`   | NONCLUSTERED| `TenantId`, `IsActive`      | Filtrado: `WHERE IsDeleted=0`. Optimiza listado de clases activas. |
| `CK_GymClasses_Category`            | CHECK       | `Category`                  | `BETWEEN 0 AND 7`.                                                 |
| `CK_GymClasses_DurationMinutes`     | CHECK       | `DurationMinutes`           | `> 0`.                                                             |
| `CK_GymClasses_MaxCapacity`         | CHECK       | `MaxCapacity`               | `> 0`.                                                             |

### Relaciones

| Tipo      | Tabla destino    | Columna(s) | Notas                                                   |
|-----------|------------------|------------|---------------------------------------------------------|
| Saliente  | `Tenants`        | `TenantId` | FK física. La clase siempre pertenece a un tenant.      |
| Entrante  | `ClassSchedules` | `GymClassId`| Un tipo de clase puede tener múltiples horarios semanales. |

---

---

## `dbo.ClassSchedules`

**Propósito de negocio.** Define los **slots semanales recurrentes** de una clase. Cada registro representa "esta clase se dicta todos los Lunes a las 07:00". El calendario del gimnasio se construye cruzando estos slots con las fechas concretas de cada semana. Un horario inactivo no aparece en el calendario pero conserva su historial de asistencias.

**Origen de datos.** Alta y gestión exclusiva desde el panel del GymOwner/GymStaff, dentro de la ficha de cada clase.

### Columnas

| Columna          | Tipo            | Null | Default | Descripción                                                                                                     |
|------------------|-----------------|:----:|---------|----------------------------------------------------------------------------------------------------------------|
| `Id`             | UNIQUEIDENTIFIER| No   | —       | Identificador único del horario. Generado por la aplicación.                                                   |
| `TenantId`       | UNIQUEIDENTIFIER| No   | —       | Gimnasio al que pertenece.                                                                                     |
| `GymClassId`     | UNIQUEIDENTIFIER| No   | —       | Clase padre a la que pertenece este horario. FK a `GymClasses`.                                                |
| `DayOfWeek`      | INT             | No   | —       | Día de la semana: `0`=Domingo … `6`=Sábado (convención .NET `DayOfWeek`).                                      |
| `StartTime`      | TIME(0)         | No   | —       | Hora de inicio de la clase (sin segundos).                                                                     |
| `EndTime`        | TIME(0)         | No   | —       | Hora de término (calculada desde `StartTime` + `DurationMinutes` de la clase o definida explícitamente). Debe ser posterior a `StartTime`. |
| `InstructorName` | NVARCHAR(100)   | Sí   | —       | Nombre del instructor que dicta el horario (sin relación FK a `Users` por ahora).                              |
| `Room`           | NVARCHAR(100)   | Sí   | —       | Sala o espacio donde se dicta la clase.                                                                        |
| `MaxCapacity`    | INT             | Sí   | —       | Capacidad máxima para este horario específico. Si es `NULL`, hereda `GymClasses.MaxCapacity`.                  |
| `IsActive`       | BIT             | No   | `1`     | `0` = horario desactivado, no aparece en el calendario.                                                        |
| `CreatedAtUtc`   | DATETIME2(3)    | No   | —       | Cuándo se creó el horario.                                                                                     |
| `CreatedBy`      | NVARCHAR(200)   | Sí   | —       | Usuario que lo creó.                                                                                           |
| `ModifiedAtUtc`  | DATETIME2(3)    | Sí   | —       | Última modificación.                                                                                           |
| `ModifiedBy`     | NVARCHAR(200)   | Sí   | —       | Quién modificó.                                                                                                |
| `IsDeleted`      | BIT             | No   | `0`     | Soft delete.                                                                                                   |
| `DeletedAtUtc`   | DATETIME2(3)    | Sí   | —       | Cuándo se eliminó lógicamente.                                                                                 |
| `DeletedBy`      | NVARCHAR(200)   | Sí   | —       | Quién hizo el borrado lógico.                                                                                  |

### Índices y constraints

| Nombre                                     | Tipo        | Columnas                            | Notas                                                                                  |
|--------------------------------------------|-------------|-------------------------------------|----------------------------------------------------------------------------------------|
| `PK_ClassSchedules`                        | PRIMARY KEY | `Id`                                |                                                                                        |
| `FK_ClassSchedules_Tenants`                | FK          | `TenantId → Tenants.Id`             |                                                                                        |
| `FK_ClassSchedules_GymClasses`             | FK          | `GymClassId → GymClasses.Id`        |                                                                                        |
| `IX_ClassSchedules_TenantId`               | NONCLUSTERED| `TenantId`                          | Filtrado por tenant.                                                                   |
| `IX_ClassSchedules_GymClassId`             | NONCLUSTERED| `GymClassId`                        | Listar horarios de una clase específica.                                               |
| `IX_ClassSchedules_TenantId_DayOfWeek`     | NONCLUSTERED| `TenantId`, `DayOfWeek`, `StartTime`| Filtrado: `WHERE IsDeleted=0 AND IsActive=1`. Optimiza la consulta del calendario semanal. |
| `CK_ClassSchedules_DayOfWeek`              | CHECK       | `DayOfWeek`                         | `BETWEEN 0 AND 6`.                                                                     |
| `CK_ClassSchedules_TimeRange`              | CHECK       | `StartTime`, `EndTime`              | `EndTime > StartTime`.                                                                 |
| `CK_ClassSchedules_MaxCapacity`            | CHECK       | `MaxCapacity`                       | `IS NULL OR MaxCapacity > 0`.                                                          |

### Relaciones

| Tipo     | Tabla destino      | Columna(s)      | Notas                                                              |
|----------|--------------------|-----------------|--------------------------------------------------------------------|
| Saliente | `Tenants`          | `TenantId`      | FK física.                                                         |
| Saliente | `GymClasses`       | `GymClassId`    | FK física. El horario siempre pertenece a un tipo de clase.        |
| Entrante | `ClassAttendances` | `ClassScheduleId`| Un horario puede tener múltiples registros de asistencia.          |

---

---

## `dbo.ClassAttendances`

**Propósito de negocio.** Registra el **check-in de un socio a una sesión concreta de clase**. Una sesión queda identificada por la combinación `(ClassScheduleId + SessionDate)`: el horario semanal recurrente más la fecha exacta en que ocurre. Permite a la recepcionista registrar la asistencia manualmente (por nombre o RUT) o mediante el escaneo del código QR del socio.

**Decisiones de diseño:**
- `MemberFullName` es un **snapshot** del nombre completo al momento del check-in. Así el historial se mantiene legible aunque el socio cambie de nombre en el futuro.
- **No implementa soft delete**: los registros de asistencia son inmutables una vez creados. Si un check-in fue un error, se documenta por otra vía operativa.
- La restricción `UNIQUE (MemberId, ClassScheduleId, SessionDate)` garantiza un único check-in por socio por sesión, tanto a nivel de base de datos como en la regla de negocio del dominio.

**Origen de datos.** Creado exclusivamente por GymStaff o GymOwner desde el módulo de recepción, nunca por el socio directamente.

### Columnas

| Columna           | Tipo            | Null | Default | Descripción                                                                                                          |
|-------------------|-----------------|:----:|---------|----------------------------------------------------------------------------------------------------------------------|
| `Id`              | UNIQUEIDENTIFIER| No   | —       | Identificador único del check-in. Generado por la aplicación.                                                        |
| `TenantId`        | UNIQUEIDENTIFIER| No   | —       | Gimnasio al que pertenece.                                                                                           |
| `MemberId`        | UNIQUEIDENTIFIER| No   | —       | Socio que realizó el check-in. FK a `Members`.                                                                       |
| `ClassScheduleId` | UNIQUEIDENTIFIER| No   | —       | Horario semanal al que asistió. FK a `ClassSchedules`.                                                               |
| `SessionDate`     | DATE            | No   | —       | Fecha concreta de la sesión (sin hora, en UTC). Junto con `ClassScheduleId` identifica la sesión.                    |
| `CheckedInAtUtc`  | DATETIME2(3)    | No   | —       | Timestamp exacto del momento del check-in (UTC).                                                                     |
| `CheckInMethod`   | INT             | No   | `0`     | Método usado: `0`=Manual, `1`=QR.                                                                                    |
| `MemberFullName`  | NVARCHAR(200)   | No   | —       | Nombre completo del socio en el momento del check-in (snapshot). Evita JOIN al mostrar historial.                    |
| `Notes`           | NVARCHAR(500)   | Sí   | —       | Observaciones opcionales de la recepcionista.                                                                        |
| `CreatedAtUtc`    | DATETIME2(3)    | No   | —       | Cuándo se creó el registro.                                                                                          |
| `CreatedBy`       | NVARCHAR(200)   | Sí   | —       | Usuario (recepcionista) que realizó el check-in.                                                                     |
| `ModifiedAtUtc`   | DATETIME2(3)    | Sí   | —       | Última modificación técnica.                                                                                         |
| `ModifiedBy`      | NVARCHAR(200)   | Sí   | —       | Quién hizo la modificación.                                                                                          |

> ⚠️ **Sin soft delete**: esta tabla no tiene columnas `IsDeleted / DeletedAtUtc / DeletedBy`. Los registros de asistencia son inmutables. Ver sección de Arquitectura para la justificación.

### Valores válidos de `CheckInMethod`

| Valor | Enum     | Descripción                                                             |
|:-----:|----------|-------------------------------------------------------------------------|
| `0`   | `Manual` | La recepcionista buscó al socio por nombre o RUT y registró manualmente.|
| `1`   | `QR`     | El socio escaneó su código QR personal en recepción.                    |

### Índices y constraints

| Nombre                                          | Tipo        | Columnas                                    | Notas                                                            |
|-------------------------------------------------|-------------|---------------------------------------------|------------------------------------------------------------------|
| `PK_ClassAttendances`                           | PRIMARY KEY | `Id`                                        |                                                                  |
| `FK_ClassAttendances_Tenants`                   | FK          | `TenantId → Tenants.Id`                     |                                                                  |
| `FK_ClassAttendances_Members`                   | FK          | `MemberId → Members.Id`                     | `ON DELETE RESTRICT` — no se puede borrar un socio con check-ins. |
| `FK_ClassAttendances_ClassSchedules`            | FK          | `ClassScheduleId → ClassSchedules.Id`       | `ON DELETE RESTRICT`.                                            |
| `UQ_ClassAttendances_Member_Schedule_Date`      | UNIQUE      | `MemberId`, `ClassScheduleId`, `SessionDate`| Garantiza un único check-in por socio por sesión.                |
| `IX_ClassAttendances_Schedule_Date`             | NONCLUSTERED| `TenantId`, `ClassScheduleId`, `SessionDate`| Consulta principal: lista de asistentes de una sesión.           |
| `IX_ClassAttendances_Member`                    | NONCLUSTERED| `TenantId`, `MemberId`, `SessionDate DESC`  | Historial de asistencia de un socio.                             |
| `CK_ClassAttendances_CheckInMethod`             | CHECK       | `CheckInMethod`                             | `IN (0, 1)`.                                                     |

### Relaciones

| Tipo     | Tabla destino    | Columna(s)        | Notas                                                                   |
|----------|------------------|-------------------|-------------------------------------------------------------------------|
| Saliente | `Tenants`        | `TenantId`        | FK física.                                                              |
| Saliente | `Members`        | `MemberId`        | FK física con `RESTRICT`. El socio no puede eliminarse mientras tenga check-ins. |
| Saliente | `ClassSchedules` | `ClassScheduleId` | FK física con `RESTRICT`.                                               |

### Endpoints REST relacionados

| Método | Ruta                                                   | Descripción                                                  |
|--------|--------------------------------------------------------|--------------------------------------------------------------|
| `POST` | `/api/v1/attendances`                                  | Registra un check-in (manual o QR). Retorna `201` con el Id. |
| `GET`  | `/api/v1/schedules/{scheduleId}/attendances?sessionDate=` | Lista los check-ins de un horario para una fecha dada.    |

---

---

## `dbo.GymEntries`

**Propósito de negocio.** Registra cada **ingreso aprobado al gimnasio** (acceso a las instalaciones). Un ingreso se crea sólo cuando el socio cumple todas las condiciones: estado Active, membresía vigente (Status=Active, EndDate≥hoy), día habilitado por el plan y horario permitido.

**Decisiones de diseño:**
- No tiene soft delete: los ingresos son registros de auditoría inmutables.
- `MemberFullName` es un snapshot del nombre al momento del ingreso.
- `MembershipAssignmentId` vincula el ingreso a la membresía vigente al momento de acceder.

### Columnas

| Columna | Tipo | Null | Default | Descripción |
|---|---|:---:|---|---|
| `Id` | UNIQUEIDENTIFIER | No | — | PK generado por la aplicación. |
| `TenantId` | UNIQUEIDENTIFIER | No | — | Gimnasio al que pertenece. |
| `MemberId` | UNIQUEIDENTIFIER | No | — | Socio que ingresó. FK → `Members`. |
| `MembershipAssignmentId` | UNIQUEIDENTIFIER | No | — | Membresía vigente al ingresar. FK → `MembershipAssignments`. |
| `EntryMethod` | INT | No | `0` | Método: `0`=Manual, `1`=QR, `2`=Badge. |
| `EnteredAtUtc` | DATETIME2(3) | No | — | Timestamp exacto del ingreso (UTC). |
| `MemberFullName` | NVARCHAR(200) | No | — | Snapshot del nombre al momento del ingreso. |
| `Notes` | NVARCHAR(500) | Sí | — | Observaciones opcionales. |
| `CreatedAtUtc` | DATETIME2(3) | No | — | Cuándo se creó el registro. |
| `CreatedBy` | NVARCHAR(200) | Sí | — | Recepcionista que registró el ingreso. |
| `ModifiedAtUtc` | DATETIME2(3) | Sí | — | Última modificación. |
| `ModifiedBy` | NVARCHAR(200) | Sí | — | Quién modificó. |

> ⚠️ **Sin soft delete.**

### Valores válidos de `EntryMethod`

| Valor | Enum | Descripción |
|:---:|---|---|
| `0` | `Manual` | Registro manual por recepcionista. |
| `1` | `QR` | Código QR escaneado. |
| `2` | `Badge` | Tarjeta o llavero RFID/NFC. |

### Índices y relaciones

| Nombre | Tipo | Columnas |
|---|---|---|
| `PK_GymEntries` | PRIMARY KEY | `Id` |
| `FK_GymEntries_Tenants` | FK | `TenantId → Tenants.Id` |
| `FK_GymEntries_Members` | FK | `MemberId → Members.Id` (RESTRICT) |
| `FK_GymEntries_MembershipAssignments` | FK | `MembershipAssignmentId → MembershipAssignments.Id` (RESTRICT) |
| `IX_GymEntries_TenantId_EnteredAt` | NONCLUSTERED | `TenantId`, `EnteredAtUtc DESC` |
| `IX_GymEntries_MemberId` | NONCLUSTERED | `TenantId`, `MemberId`, `EnteredAtUtc DESC` |

---

## `dbo.ClassReservations`

**Propósito de negocio.** Registra las **reservas de socios a sesiones concretas de clase**. Una sesión queda identificada por `(ClassScheduleId + SessionDate)`. La unicidad de reservas activas (un socio por sesión) se valida en Application antes de persistir.

**Decisiones de diseño:**
- No tiene soft delete: las reservas son registros de auditoría inmutables.
- `MemberFullName` es un snapshot del nombre al momento de la reserva.
- Un socio puede tener a lo sumo una reserva `Active` por sesión.

### Columnas

| Columna | Tipo | Null | Default | Descripción |
|---|---|:---:|---|---|
| `Id` | UNIQUEIDENTIFIER | No | — | PK generado por la aplicación. |
| `TenantId` | UNIQUEIDENTIFIER | No | — | Gimnasio al que pertenece. |
| `MemberId` | UNIQUEIDENTIFIER | No | — | Socio que reservó. FK → `Members`. |
| `ClassScheduleId` | UNIQUEIDENTIFIER | No | — | Horario reservado. FK → `ClassSchedules`. |
| `SessionDate` | DATE | No | — | Fecha concreta de la sesión. |
| `ReservedAtUtc` | DATETIME2(3) | No | — | Timestamp de la reserva (UTC). |
| `Status` | INT | No | `0` | Estado de la reserva. Ver tabla de valores. |
| `CancelledAtUtc` | DATETIME2(3) | Sí | — | Cuándo se canceló (si aplica). |
| `CancelReason` | NVARCHAR(500) | Sí | — | Motivo de cancelación. |
| `MemberFullName` | NVARCHAR(200) | No | — | Snapshot del nombre al reservar. |
| `CreatedAtUtc` | DATETIME2(3) | No | — | Cuándo se creó el registro. |
| `CreatedBy` | NVARCHAR(200) | Sí | — | Quién creó la reserva. |
| `ModifiedAtUtc` | DATETIME2(3) | Sí | — | Última modificación. |
| `ModifiedBy` | NVARCHAR(200) | Sí | — | Quién modificó. |

> ⚠️ **Sin soft delete.**

### Valores válidos de `Status`

| Valor | Enum | Descripción |
|:---:|---|---|
| `0` | `Active` | Reserva vigente, lugar confirmado. |
| `1` | `CancelledByMember` | El socio anuló su reserva. |
| `2` | `CancelledByStaff` | El staff anuló la reserva. |
| `3` | `NoShow` | El socio no se presentó. |

### Índices y relaciones

| Nombre | Tipo | Columnas |
|---|---|---|
| `PK_ClassReservations` | PRIMARY KEY | `Id` |
| `FK_ClassReservations_Tenants` | FK | `TenantId → Tenants.Id` |
| `FK_ClassReservations_Members` | FK | `MemberId → Members.Id` (RESTRICT) |
| `FK_ClassReservations_ClassSchedules` | FK | `ClassScheduleId → ClassSchedules.Id` (RESTRICT) |
| `IX_ClassReservations_Schedule_Date` | NONCLUSTERED | `TenantId`, `ClassScheduleId`, `SessionDate` |
| `IX_ClassReservations_Member` | NONCLUSTERED | `TenantId`, `MemberId`, `SessionDate DESC` |
| `CK_ClassReservations_Status` | CHECK | `Status BETWEEN 0 AND 3` |

---

## `dbo.Equipment`

**Propósito de negocio.** Catálogo de **máquinas y equipos físicos** del gimnasio. Cada máquina puede tener múltiples registros de mantención a lo largo del tiempo. Desactivar una máquina la oculta para nuevas mantenciones pero conserva su historial.

**Origen de datos.** Alta y gestión desde el panel del GymOwner/GymStaff (módulo Maquinaria).

### Columnas

| Columna | Tipo | Null | Default | Descripción |
|---|---|:---:|---|---|
| `Id` | UNIQUEIDENTIFIER | No | — | PK generado por la aplicación. |
| `TenantId` | UNIQUEIDENTIFIER | No | — | Gimnasio al que pertenece. FK → `Tenants`. |
| `Name` | NVARCHAR(100) | No | — | Nombre de la máquina. Ej: `"Cinta de correr 1"`. |
| `Brand` | NVARCHAR(100) | Sí | — | Marca. Ej: `"Life Fitness"`. |
| `Model` | NVARCHAR(100) | Sí | — | Modelo. Ej: `"T5-0"`. |
| `SerialNumber` | NVARCHAR(50) | Sí | — | Número de serie del fabricante. |
| `PurchaseDate` | DATE | Sí | — | Fecha de compra. |
| `ImageUrl` | NVARCHAR(500) | Sí | — | URL de imagen de la máquina. |
| `IsActive` | BIT | No | `1` | `0` = desactivada, no disponible para nuevas mantenciones. |
| `CreatedAtUtc` | DATETIME2(3) | No | — | Auditoría. |
| `CreatedBy` | NVARCHAR(200) | Sí | — | Auditoría. |
| `ModifiedAtUtc` | DATETIME2(3) | Sí | — | Auditoría. |
| `ModifiedBy` | NVARCHAR(200) | Sí | — | Auditoría. |
| `IsDeleted` | BIT | No | `0` | Soft delete. |
| `DeletedAtUtc` | DATETIME2(3) | Sí | — | Auditoría de borrado. |
| `DeletedBy` | NVARCHAR(200) | Sí | — | Auditoría de borrado. |

### Índices y relaciones

| Nombre | Tipo | Columnas |
|---|---|---|
| `PK_Equipment` | PRIMARY KEY | `Id` |
| `FK_Equipment_Tenants` | FK | `TenantId → Tenants.Id` |
| `IX_Equipment_TenantId` | NONCLUSTERED | `TenantId` |
| `IX_Equipment_TenantId_IsActive` | NONCLUSTERED | `TenantId`, `IsActive` |

### Relaciones

| Tipo | Tabla destino | Columna(s) | Notas |
|---|---|---|---|
| Saliente | `Tenants` | `TenantId` | FK física. |
| Entrante | `MaintenanceRecords` | `EquipmentId` | Una máquina puede tener N mantenciones. ON DELETE RESTRICT. |

---

## `dbo.MaintenanceRecords`

**Propósito de negocio.** Registra las **mantenciones preventivas y correctivas** de las máquinas del gimnasio. Gestiona el ciclo de vida: Pending → InProgress → Completed (o Cancelled). Permite asignar un responsable interno o externo, registrar costo y observaciones al cerrar.

**Origen de datos.** Alta desde el panel del GymOwner/GymStaff (módulo Mantención).

### Columnas

| Columna | Tipo | Null | Default | Descripción |
|---|---|:---:|---|---|
| `Id` | UNIQUEIDENTIFIER | No | — | PK generado por la aplicación. |
| `TenantId` | UNIQUEIDENTIFIER | No | — | Gimnasio al que pertenece. |
| `EquipmentId` | UNIQUEIDENTIFIER | No | — | Máquina a mantener. FK → `Equipment`. |
| `Type` | INT | No | — | Tipo: `0`=Preventive, `1`=Corrective. |
| `Status` | INT | No | `0` | Estado: `0`=Pending, `1`=InProgress, `2`=Completed, `3`=Cancelled. |
| `ScheduledDate` | DATE | No | — | Fecha programada de la mantención. |
| `StartedAtUtc` | DATETIME2(3) | Sí | — | Timestamp real de inicio (al ejecutar "Iniciar"). |
| `CompletedAtUtc` | DATETIME2(3) | Sí | — | Timestamp real de cierre (Completada o Cancelada). |
| `Description` | NVARCHAR(500) | No | — | Qué se va a hacer / qué se hizo. |
| `Notes` | NVARCHAR(1000) | Sí | — | Observaciones de cierre o motivo de cancelación. |
| `Cost` | DECIMAL(10,2) | Sí | — | Costo incurrido (se registra al completar). |
| `ResponsibleType` | INT | No | — | `0`=Internal, `1`=External. |
| `ResponsibleUserId` | UNIQUEIDENTIFIER | Sí | — | FK → `Users` (solo si ResponsibleType=Internal). |
| `ExternalProviderName` | NVARCHAR(200) | Sí | — | Nombre del proveedor externo. |
| `ExternalProviderContact` | NVARCHAR(200) | Sí | — | Teléfono o email del proveedor. |
| `CreatedAtUtc` | DATETIME2(3) | No | — | Auditoría. |
| `CreatedBy` | NVARCHAR(200) | Sí | — | Auditoría. |
| `ModifiedAtUtc` | DATETIME2(3) | Sí | — | Auditoría. |
| `ModifiedBy` | NVARCHAR(200) | Sí | — | Auditoría. |

> ⚠️ **Sin soft delete.** Los registros de mantención son inmutables una vez completados o cancelados. El ciclo de vida se gestiona via transiciones de estado.

### Valores válidos de `Type`

| Valor | Enum | Descripción |
|:---:|---|---|
| `0` | `Preventive` | Mantención programada / periódica. |
| `1` | `Corrective` | Reparación ante falla o daño reportado. |

### Valores válidos de `Status`

| Valor | Enum | Transición permitida |
|:---:|---|---|
| `0` | `Pending` | → InProgress, → Cancelled |
| `1` | `InProgress` | → Completed, → Cancelled |
| `2` | `Completed` | Estado final |
| `3` | `Cancelled` | Estado final |

### Valores válidos de `ResponsibleType`

| Valor | Enum | Campos que aplican |
|:---:|---|---|
| `0` | `Internal` | `ResponsibleUserId` (opcional) |
| `1` | `External` | `ExternalProviderName` (requerido), `ExternalProviderContact` (opcional) |

### Índices y relaciones

| Nombre | Tipo | Columnas |
|---|---|---|
| `PK_MaintenanceRecords` | PRIMARY KEY | `Id` |
| `FK_MaintenanceRecords_Tenants` | FK | `TenantId → Tenants.Id` |
| `FK_MaintenanceRecords_Equipment` | FK | `EquipmentId → Equipment.Id` (RESTRICT) |
| `FK_MaintenanceRecords_Users` | FK | `ResponsibleUserId → Users.Id` (RESTRICT, nullable) |
| `IX_MR_TenantId` | NONCLUSTERED | `TenantId` |
| `IX_MR_EquipmentId` | NONCLUSTERED | `EquipmentId` |
| `IX_MR_TenantId_Status` | NONCLUSTERED | `TenantId`, `Status` |
| `CK_MR_Type` | CHECK | `Type IN (0, 1)` |
| `CK_MR_Status` | CHECK | `Status BETWEEN 0 AND 3` |
| `CK_MR_ResponsibleType` | CHECK | `ResponsibleType IN (0, 1)` |

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
