# GymGo · Deuda Técnica y Pendientes

Documento de referencia para funcionalidades planificadas, decisiones de diseño diferidas y deuda técnica identificada durante el desarrollo. **Actualizar en el mismo PR/commit que genere o resuelva cada ítem.**

| Versión | Fecha      | Cambios                                         |
|---------|------------|-------------------------------------------------|
| 1.0     | 2026-04-22 | Documento inicial tras Sprint 0-1 + módulos Members y MembershipPlans. |
| 1.1     | 2026-04-22 | Ítem 6 resuelto: `POST /auth/login` con JWT. |

---

## Índice

- [Módulo: Asignaciones de Membresía](#módulo-asignaciones-de-membresía)
- [Módulo: Autenticación y Usuarios](#módulo-autenticación-y-usuarios)
- [Módulo: Pagos](#módulo-pagos)
- [Módulo: Clases y Horarios](#módulo-clases-y-horarios)
- [Módulo: Instructores](#módulo-instructores)
- [Módulo: Equipamiento](#módulo-equipamiento)
- [Módulo: Notificaciones](#módulo-notificaciones)
- [Infraestructura y Arquitectura](#infraestructura-y-arquitectura)
- [Base de Datos](#base-de-datos)
- [App Móvil](#app-móvil)

---

## Módulo: Asignaciones de Membresía

> **Próximo a implementar.** Conecta `Members` con `MembershipPlans`.

| # | Ítem | Prioridad | Sprint |
|---|------|:---------:|:------:|
| ~~1~~ | ~~Tabla `MembershipAssignments`~~| ~~Alta~~ | ✅ Resuelto 2026-04-22 |
| ~~2~~ | ~~Endpoints de asignación~~ | ~~Alta~~ | ✅ Resuelto 2026-04-22 |
| ~~3~~ | ~~Lógica de morosidad~~ | ~~Alta~~ | ✅ Resuelto 2026-04-22 (`MarkOverdue` + `RegisterPayment` reactiva socio) |
| ~~4~~ | ~~Historial de membresías~~ | ~~Media~~ | ✅ Resuelto 2026-04-22 (`GetMemberAssignments`) |
| ~~5~~ | ~~Soporte de congelamiento~~ | ~~Media~~ | ✅ Resuelto 2026-04-22 (`Freeze`/`Unfreeze` con extensión de `EndDate`) |

---

## Módulo: Autenticación y Usuarios

| # | Ítem | Prioridad | Sprint |
|---|------|:---------:|:------:|
| ~~6~~ | ~~Implementar endpoints de Login (`POST /auth/login`) con generación de JWT.~~ | ~~Alta~~ | ✅ Resuelto 2026-04-22 (`LoginCommand` + `POST /api/v1/auth/login`, JWT con claims estándar) |
| 7 | Endpoint de registro de socio desde app móvil (`POST /auth/register`) — sólo rol `Member`, requiere header `X-Tenant-Id`. | Alta | Sprint 4 |
| 8 | Vinculación `Member ↔ User`: agregar columna nullable `UserId` en `Members` para asociar un socio con su cuenta de acceso a la app móvil. | Media | Sprint 4 |
| 9 | Refresh tokens: actualmente el JWT no tiene mecanismo de renovación sin re-login. | Media | Sprint 4 |
| 10 | Endpoint de cambio de contraseña (`POST /auth/change-password`). | Baja | Sprint 4 |

---

## Módulo: Pagos

| # | Ítem | Prioridad | Sprint |
|---|------|:---------:|:------:|
| 11 | Tabla `Payments`: registrar cada pago asociado a una asignación de membresía (`MembershipAssignmentId`, monto, fecha, medio de pago). | Alta | Sprint 3 |
| 12 | Endpoint `POST /payments` — registrar pago manual desde recepción. | Alta | Sprint 3 |
| 13 | Listado de socios morosos (`GET /members?status=Delinquent`). Ya soportado en query filter, falta lógica de marcado automático. | Alta | Sprint 3 |
| 14 | Integración Transbank/Webpay para pagos en línea (Chile). | Baja | Post-MVP |
| 15 | Integración MercadoPago para pagos en línea (resto de LATAM). | Baja | Post-MVP |

---

## Módulo: Clases y Horarios

| # | Ítem | Prioridad | Sprint |
|---|------|:---------:|:------:|
| 16 | Tabla `Classes`: definición de clases dictadas en el gimnasio (nombre, instructor, capacidad máxima, sala). | Alta | Sprint 3 |
| 17 | Tabla `ClassSchedules`: horario recurrente de cada clase (día de semana, hora inicio/fin). | Alta | Sprint 3 |
| 18 | Tabla `ClassReservations`: reservas de socios a clases específicas. | Alta | Sprint 4 |
| 19 | Validación de capacidad máxima al reservar una clase. | Alta | Sprint 4 |
| 20 | Validación de que el socio tiene membresía vigente y con acceso al horario de la clase antes de permitir reserva. | Alta | Sprint 4 |
| 21 | Cancelación de reservas con política de tiempo mínimo de anticipación (configurable por tenant). | Media | Sprint 4 |

---

## Módulo: Instructores

| # | Ítem | Prioridad | Sprint |
|---|------|:---------:|:------:|
| 22 | CRUD de instructores (actualmente el rol `Instructor` existe en `Users` pero no hay un perfil extendido). Considerar tabla `InstructorProfiles` con especialidades, bio y foto. | Media | Sprint 4 |
| 23 | Asignación de instructores a clases (`ClassSchedules.InstructorId`). | Media | Sprint 4 |

---

## Módulo: Equipamiento

| # | Ítem | Prioridad | Sprint |
|---|------|:---------:|:------:|
| 24 | Tabla `Equipment`: inventario de equipos del gimnasio (nombre, categoría, número de serie, estado). | Media | Sprint 5 |
| 25 | Tabla `MaintenanceLogs`: registro de mantenciones por equipo (fecha, tipo, responsable, observaciones). | Media | Sprint 5 |
| 26 | Alertas de mantenimiento preventivo por vencimiento de fecha programada. | Baja | Sprint 5 |

---

## Módulo: Notificaciones

| # | Ítem | Prioridad | Sprint |
|---|------|:---------:|:------:|
| 27 | Notificación automática por email cuando una membresía está próxima a vencer (configurable: 3, 7, 15 días antes). | Alta | Sprint 6 |
| 28 | Push notification a la app móvil por vencimiento y clases reservadas. | Alta | Sprint 6 |
| 29 | Notificación al socio cuando es marcado como moroso. | Media | Sprint 6 |
| 30 | Integración con proveedor de email (SendGrid o SES). Actualmente no hay proveedor configurado. | Alta | Sprint 6 |

---

## Infraestructura y Arquitectura

| # | Ítem | Prioridad | Sprint |
|---|------|:---------:|:------:|
| 31 | CI/CD: pipeline de build + test automático en cada PR (GitHub Actions o Azure DevOps). Actualmente no configurado. | Alta | Sprint 0 (pendiente) |
| 32 | Ambiente de staging: clonar configuración de `appsettings` para ambiente `Staging` separado de `Dev`. | Alta | Sprint 0 (pendiente) |
| 33 | Publicación de dominio de eventos (`IDomainEvent`): la infraestructura de `AggregateRoot` acumula eventos pero no hay dispatcher implementado. Evaluar MediatR Notifications. | Media | Sprint 3 |
| 34 | Rate limiting en endpoints públicos (registro, login). Actualmente sin protección contra abuso. | Media | Sprint 4 |
| 35 | `CLAUDE.md`: documentar convenciones del proyecto para onboarding de nuevos desarrolladores. | Baja | Sprint 2 |

---

## Base de Datos

| # | Ítem | Prioridad | Sprint |
|---|------|:---------:|:------:|
| 36 | FK física `Members.TenantId → Tenants.Id`: actualmente sin FK explícita en el script SQL (igual que `Users`). Agregar al ejecutar el script de `Members`. | Alta | Sprint 2 |
| 37 | FK física `MembershipPlans.TenantId → Tenants.Id`: ídem anterior. | Alta | Sprint 2 |
| 38 | Script de seed para datos de prueba de `Members` y `MembershipPlans` (análogo a `02_seed/01_DefaultData.sql`). | Media | Sprint 2 |
| 39 | Script de rollback documentado para cada script de schema (en caso de necesitar revertir un deploy). | Baja | Sprint 7 |

---

## App Móvil

| # | Ítem | Prioridad | Sprint |
|---|------|:---------:|:------:|
| 40 | Decisión pendiente: React Native vs PWA. Evaluar experiencia del equipo y tiempo disponible antes de Sprint 4. | Alta | Sprint 3 |
| 41 | Congelar contrato OpenAPI (Swagger) antes de comenzar desarrollo móvil para evitar breaking changes. | Alta | Sprint 2 |
| 42 | Pantalla de login del socio con autenticación JWT. | Alta | Sprint 4 |
| 43 | Vista de membresía activa: plan vigente, días restantes, horario habilitado. | Alta | Sprint 4 |
| 44 | Registro de rutinas y progreso físico (peso, medidas) del socio. | Media | Sprint 5 |
| 45 | Vista de reservas de clases desde la app. | Media | Sprint 5 |

---

## Cómo usar este documento

- **Resolver un ítem**: eliminar la fila o moverla a una sección `## Resueltos` con la fecha y el PR/commit correspondiente.
- **Agregar un ítem**: incorporarlo en la sección correspondiente con número correlativo, prioridad (Alta / Media / Baja) y sprint estimado.
- **Prioridades**: Alta = bloquea funcionalidad core / Media = importante pero no urgente / Baja = mejora o nice-to-have.
