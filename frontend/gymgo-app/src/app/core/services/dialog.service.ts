import { Injectable } from '@angular/core';
import Swal, { SweetAlertResult } from 'sweetalert2';

/**
 * Wrapper sobre SweetAlert2 con theming coherente con el design system de GymGo.
 * Centralizar aquí facilita cambiar la librería en un solo lugar.
 */
@Injectable({ providedIn: 'root' })
export class DialogService {

  /** Confirmación de acción destructiva (eliminar, dar de baja, etc.) */
  confirmDanger(title: string, text: string, confirmText = 'Sí, eliminar'): Promise<SweetAlertResult> {
    return Swal.fire({
      title,
      text,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: confirmText,
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#dc2626',   // --color-error
      cancelButtonColor:  '#475569',   // --color-muted
      focusCancel: true,
      reverseButtons: true,
    });
  }

  /** Confirmación neutra (editar, guardar cambios, etc.) */
  confirmAction(title: string, text: string, confirmText = 'Confirmar'): Promise<SweetAlertResult> {
    return Swal.fire({
      title,
      text,
      icon: 'question',
      showCancelButton: true,
      confirmButtonText: confirmText,
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#2563eb',   // --color-primary
      cancelButtonColor:  '#475569',
      reverseButtons: true,
    });
  }

  /** Notificación de éxito (no bloqueante — se cierra sola) */
  success(title: string, text = '', timer = 2500): Promise<SweetAlertResult> {
    return Swal.fire({
      title,
      text,
      icon: 'success',
      confirmButtonText: 'Aceptar',
      confirmButtonColor: '#16a34a',   // --color-success
      timer,
      timerProgressBar: true,
    });
  }

  /** Notificación de error */
  error(title: string, text = ''): Promise<SweetAlertResult> {
    return Swal.fire({
      title,
      text,
      icon: 'error',
      confirmButtonText: 'Cerrar',
      confirmButtonColor: '#dc2626',
    });
  }

  /** Toast ligero (esquina inferior derecha, sin modal) */
  toast(title: string, icon: 'success' | 'error' | 'warning' | 'info' = 'success'): Promise<SweetAlertResult> {
    return Swal.fire({
      toast: true,
      position: 'bottom-end',
      icon,
      title,
      showConfirmButton: false,
      timer: 3000,
      timerProgressBar: true,
    });
  }
}
