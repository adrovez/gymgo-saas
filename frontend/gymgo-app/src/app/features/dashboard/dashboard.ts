import { Component, inject } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  template: `
    <div class="space-y-6">
      <div>
        <h1 class="text-2xl font-bold text-slate-800">Dashboard</h1>
        <p class="text-slate-500 mt-1">Bienvenido, {{ session()?.fullName }}.</p>
      </div>
      <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
        <div class="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
          <p class="text-sm text-slate-500">Socios activos</p>
          <p class="text-3xl font-bold text-slate-800 mt-1">—</p>
        </div>
        <div class="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
          <p class="text-sm text-slate-500">Planes vigentes</p>
          <p class="text-3xl font-bold text-slate-800 mt-1">—</p>
        </div>
        <div class="bg-white rounded-xl border border-slate-200 p-5 shadow-sm">
          <p class="text-sm text-slate-500">Membresías activas</p>
          <p class="text-3xl font-bold text-slate-800 mt-1">—</p>
        </div>
      </div>
    </div>
  `,
})
export class DashboardComponent {
  readonly session = inject(AuthService).session;
}
