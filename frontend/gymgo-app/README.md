# GymGo Frontend — Angular 21

## Setup

```bash
cd frontend/gymgo-app
npm install
npm start
```

La app corre en `http://localhost:4200` y apunta a la API en `http://localhost:5126/api/v1`.

## Estructura

```
src/app/
├── core/
│   ├── models/          # Interfaces TypeScript (auth.models.ts)
│   ├── services/        # AuthService, StorageService
│   ├── interceptors/    # authInterceptor (Bearer + X-Tenant-Id)
│   └── guards/          # authGuard
├── features/
│   ├── auth/login/      # Pantalla de login
│   ├── shell/           # Layout principal (sidebar + navbar)
│   └── dashboard/       # Dashboard placeholder
└── environments/        # environment.ts / environment.prod.ts
```

## Flujo de autenticación

1. El usuario ingresa email + contraseña + (opcional) Tenant ID.
2. La app llama `POST /api/v1/auth/login` con `X-Tenant-Id` en el header si fue provisto.
3. El JWT y los datos de sesión se guardan en `localStorage`.
4. El `authInterceptor` inyecta `Authorization: Bearer <token>` y `X-Tenant-Id` en cada request.
5. El `authGuard` protege todas las rutas bajo `/app/**`.
