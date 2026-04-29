# AGENTS.md — GymGo SaaS (Guía operativa real)

> Alcance: **todo el repositorio** `gymgo-saas/`.
> Objetivo: que cualquier agente contribuya sin romper arquitectura, seguridad multi-tenant ni calidad de entrega.

## 1) Contexto del producto
GymGo es un SaaS multi-tenant para gestión de gimnasios.

- **Backend**: .NET 8 con Clean Architecture.
- **Frontend**: Angular standalone (feature-based, lazy loading).
- **Persistencia**: SQL Server con filtro por tenant.
- **Auth**: JWT + roles + `tenant_id`.

Principio rector: **nada puede exponer datos cross-tenant** por error de código.

---

## 2) Mapa de arquitectura (fuente de verdad)

### Backend
- `src/GymGo.Domain`: entidades, VOs, reglas, excepciones de dominio.
- `src/GymGo.Application`: casos de uso (MediatR), DTOs, validaciones, interfaces.
- `src/GymGo.Infrastructure`: EF Core, auth, servicios, implementación de interfaces.
- `src/GymGo.API`: endpoints, middleware, wiring DI, observabilidad.

### Frontend
- `frontend/gymgo-app/src/app/core`: servicios transversales, guards, interceptors, modelos base.
- `frontend/gymgo-app/src/app/features`: módulos de negocio lazy-loaded.
- `frontend/gymgo-app/src/environments`: configuración por ambiente.

### Otros
- `tests/`: unit + integration tests.
- `database/sql/`: esquema y seed manual versionado.
- `docs/arquitectura/`: documentación funcional/técnica vigente.

---

## 3) Reglas de diseño obligatorias

### Clean Architecture (.NET)
1. `Domain` **no** depende de `Application`, `Infrastructure` ni `API`.
2. `Application` no conoce EF/SQL/HTTP concretos; usa abstracciones.
3. `Infrastructure` implementa contratos de `Application`.
4. `API` orquesta; no contiene lógica de negocio compleja.

### Frontend Angular
1. Componentes standalone y módulos por feature.
2. `core/` no depende de `features/`.
3. Interceptor centraliza headers de autenticación/tenant.
4. Guards para proteger `/app/*`.

---

## 4) Multi-tenancy y seguridad (SaaS real)

### Reglas críticas
- Toda entidad tenant-scoped debe incluir `TenantId` y quedar sujeta a query filters.
- Nunca bypass de filtros multi-tenant sin justificación explícita y validada.
- `PlatformAdmin` puede tener visión global; otros roles siempre acotados a tenant.
- Requests autenticados deben propagar:
  - `Authorization: Bearer <token>`
  - `X-Tenant-Id` cuando aplique (no PlatformAdmin).

### Checklist antes de mergear
- [ ] Endpoint nuevo respeta autorización por rol.
- [ ] Consultas/updates no permiten lectura/escritura cross-tenant.
- [ ] Logs no exponen secretos, tokens ni PII sensible.
- [ ] Errores de dominio se traducen con handler global consistente.

---

## 5) Persistencia y base de datos
- Este repo usa scripts SQL en `database/sql/` como mecanismo principal de evolución.
- Si se agrega/ajusta modelo persistente:
  1. Actualizar código EF/configuración.
  2. Agregar script SQL incremental en carpeta correspondiente.
  3. Mantener idempotencia razonable para entornos de dev/QA.

No introducir cambios de esquema “silenciosos” sin script asociado.

---

## 6) Estándares de implementación

### Backend
- Preferir vertical slices por feature (command/query + validator + handler).
- Validación temprana (FluentValidation) y errores de dominio explícitos.
- Evitar lógica de negocio en controllers/endpoints.
- Asincronía end-to-end en acceso a IO.

### Frontend
- Tipado estricto (interfaces/modelos), evitar `any`.
- Estado local con Signals cuando corresponda.
- Formularios reactivos con validación de UX y mensajes claros.
- Rutas lazy para features nuevas.

---

## 7) Calidad mínima (Definition of Done)
Para cualquier cambio de código:

1. Compila backend/frontend afectados.
2. Tests relevantes pasan.
3. No rompe contratos de API existentes sin documentar.
4. Documentación mínima actualizada (`README` o `docs/arquitectura`).
5. Seguridad multi-tenant verificada manualmente en flujos críticos.

Comandos sugeridos:
- `dotnet restore && dotnet build`
- `dotnet test`
- En frontend: `npm run lint`, `npm run test`, `npm run build` (según proyecto impactado)

---

## 8) Convenciones de observabilidad y errores
- Logs estructurados (con `TenantId`, `UserId`, `CorrelationId` cuando exista).
- No loggear secretos ni payloads sensibles completos.
- Mantener formato uniforme de respuesta de error para frontend.

---

## 9) Qué evitar (anti-patrones)
- Saltar capas (API llamando directo a Infrastructure concretos sin pasar por Application).
- Duplicar lógica de autorización en múltiples puntos sin política central.
- Hardcodear tenant/role en frontend o backend.
- Mezclar reglas de negocio con lógica de presentación.

---

## 10) Si eres un agente automático, opera así
1. Ubica la capa correcta antes de editar.
2. Cambia lo mínimo necesario y conserva consistencia de estilo.
3. Ejecuta checks de compilación/tests del área impactada.
4. Resume impacto arquitectónico en el PR (capas tocadas, riesgo multi-tenant, mitigación).

Si hay conflicto entre velocidad y seguridad multi-tenant: **prioriza seguridad**.
