import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../services/auth.service';
import { UserRole } from '../models/auth.models';

export const authGuard: CanActivateFn = () => {
  const auth    = inject(AuthService);
  const router  = inject(Router);
  const session = auth.session();

  if (!session) {
    return router.createUrlTree(['/login']);
  }

  // UserRole.Member es para socios del gimnasio (app móvil/portal).
  // El panel web es exclusivo de staff — redirigir con mensaje informativo.
  if (session.role === UserRole.Member) {
    return router.createUrlTree(['/login'], {
      queryParams: { error: 'no-access' },
    });
  }

  return true;
};
