import { Injectable, signal } from '@angular/core';

/**
 * Servicio para transportar el mensaje de "acceso denegado" desde el
 * roleGuard hasta el ShellComponent, sin acoplar la guard al DOM.
 *
 * Flujo:
 *   roleGuard → set(mensaje) → redirige a /app/dashboard
 *   ShellComponent → lee message() → muestra banner → clear() al cerrar
 */
@Injectable({ providedIn: 'root' })
export class ForbiddenMessageService {
  private readonly _message = signal<string | null>(null);

  /** Mensaje actual. null = sin mensaje pendiente. */
  readonly message = this._message.asReadonly();

  /** Establece el mensaje que se mostrará tras la redirección. */
  set(msg: string): void {
    this._message.set(msg);
  }

  /** Limpia el mensaje (llamar al cerrarlo o tras mostrarlo). */
  clear(): void {
    this._message.set(null);
  }
}
