import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { ForbiddenMessageService } from '../services/forbidden-message.service';
import { UserRole } from '../models/auth.models';

/**
 * Guard funcional de autorización por rol.
 *
 * Uso en rutas:
 *   canActivate: [roleGuard([UserRole.GymOwner, UserRole.PlatformAdmin])]
 *
 * Si el rol del usuario no está en la lista:
 *   1. Deja un mensaje en ForbiddenMessageService.
 *   2. Redirige a /app/dashboard donde el shell lo muestra.
 *
 * Nota: authGuard corre antes (en la ruta padre /app), por lo que
 * cuando este guard se ejecuta la sesión ya existe.
 */
export const roleGuard = (allowedRoles: UserRole[]): CanActivateFn => () => {
  const auth           = inject(AuthService);
  const router         = inject(Router);
  const forbiddenMsg   = inject(ForbiddenMessageService);

  const session = auth.session();

  // Seguridad defensiva: si por algún motivo no hay sesión, ir a login.
  if (!session) {
    return router.createUrlTree(['/login']);
  }

  if (allowedRoles.includes(session.role)) {
    return true;
  }

  forbiddenMsg.set('No tienes permiso para acceder a esa sección.');
  return router.createUrlTree(['/app/dashboard']);
};
