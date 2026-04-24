import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

/**
 * Normaliza un RUT chileno: elimina puntos y espacios, uppercase.
 * Acepta formatos: "12345678-9", "12.345.678-9", "12345678K"
 * Salida siempre: "12345678-9"
 */
export function normalizeRut(raw: string): string {
  const clean = raw.replace(/\./g, '').replace(/\s/g, '').toUpperCase();
  // Si no tiene guión y tiene al menos 2 chars, insertar guión antes del último
  if (!clean.includes('-') && clean.length >= 2) {
    return `${clean.slice(0, -1)}-${clean.slice(-1)}`;
  }
  return clean;
}

/**
 * Verifica si un RUT chileno es válido (algoritmo módulo 11).
 * El string debe estar normalizado ("12345678-9").
 */
export function isValidRut(rut: string): boolean {
  const normalized = normalizeRut(rut);

  // Formato mínimo: NNNNNNN-D (7 o 8 dígitos + guión + dígito o K)
  if (!/^\d{7,8}-[\dK]$/.test(normalized)) return false;

  const [body, dv] = normalized.split('-');
  const digits = body.split('').reverse().map(Number);

  let sum = 0;
  let multiplier = 2;

  for (const digit of digits) {
    sum += digit * multiplier;
    multiplier = multiplier === 7 ? 2 : multiplier + 1;
  }

  const remainder = 11 - (sum % 11);
  const expected =
    remainder === 11 ? '0' :
    remainder === 10 ? 'K' :
    String(remainder);

  return dv === expected;
}

/**
 * Validador Angular para RUT chileno.
 * Agrega el error `{ rutInvalid: true }` si el RUT no es válido.
 */
export const rutValidator: ValidatorFn = (control: AbstractControl): ValidationErrors | null => {
  const value: string = (control.value ?? '').toString().trim();
  if (!value) return null;           // dejar que `required` se encargue del vacío
  return isValidRut(value) ? null : { rutInvalid: true };
};
