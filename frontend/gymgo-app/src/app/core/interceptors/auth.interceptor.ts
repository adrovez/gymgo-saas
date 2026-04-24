import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { StorageService } from '../services/storage.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const storage = inject(StorageService);
  const token = storage.getToken();
  const tenantId = storage.getTenantId();

  if (!token) return next(req);

  let headers = req.headers.set('Authorization', `Bearer ${token}`);

  if (tenantId) {
    headers = headers.set('X-Tenant-Id', tenantId);
  }

  return next(req.clone({ headers }));
};
