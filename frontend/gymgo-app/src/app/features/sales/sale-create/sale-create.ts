import {
  Component,
  OnInit,
  OnDestroy,
  inject,
  signal,
  computed,
} from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { Subject, debounceTime, distinctUntilChanged, takeUntil } from 'rxjs';

import {
  SaleItemType,
  SaleDiscountType,
  SalePaymentMethod,
  SALE_ITEM_TYPE_LABELS,
  SALE_PAYMENT_METHOD_OPTIONS,
  SALE_ITEM_TYPE_OPTIONS,
  CheckoutSaleRequest,
} from '../models/sale.models';
import { SaleService } from '../services/sale.service';
import { MemberService } from '../../members/services/member.service';
import { MembershipPlanService } from '../../membership-plans/services/membership-plan.service';
import { MemberSummaryDto, MemberStatus } from '../../members/models/member.models';
import { MembershipPlanSummaryDto } from '../../membership-plans/models/membership-plan.models';
import { DialogService } from '../../../core/services/dialog.service';

/** Ítem de venta mantenido solo en el formulario hasta que se presiona "Cobrar". */
export interface LocalSaleItem {
  id:               string; // id temporal, solo para track/eliminar en la UI
  itemType:         SaleItemType;
  description:      string;
  membershipPlanId: string | null;
  unitPrice:        number;
  quantity:         number;
  subtotal:         number;
}

@Component({
  selector: 'app-sale-create',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './sale-create.html',
})
export class SaleCreateComponent implements OnInit, OnDestroy {
  private readonly saleService   = inject(SaleService);
  private readonly memberService = inject(MemberService);
  private readonly planService   = inject(MembershipPlanService);
  private readonly dialog        = inject(DialogService);
  private readonly router        = inject(Router);
  private readonly destroy$      = new Subject<void>();

  // ── Estado general ────────────────────────────────────────────────────────
  readonly globalError = signal<string | null>(null);

  // ── Búsqueda de socio ─────────────────────────────────────────────────────
  memberSearch             = '';
  readonly memberLoading   = signal(false);
  readonly memberResults   = signal<MemberSummaryDto[]>([]);
  readonly selectedMember  = signal<MemberSummaryDto | null>(null);
  private readonly memberSearch$ = new Subject<string>();

  // ── Pago ──────────────────────────────────────────────────────────────────
  paymentMethod = SalePaymentMethod.Efectivo;
  saleNotes     = '';

  // ── Ítems (solo en el formulario, no persistidos hasta Cobrar) ────────────
  readonly items           = signal<LocalSaleItem[]>([]);
  readonly itemFormVisible = signal(false);
  readonly itemError       = signal<string | null>(null);

  newItemType        = SaleItemType.Plan;
  newItemDescription = '';
  newItemPlanId      = '';
  newItemUnitPrice   = 0;
  newItemQuantity    = 1;

  readonly plans        = signal<MembershipPlanSummaryDto[]>([]);
  readonly plansLoading = signal(false);

  // ── Descuento local (se aplica al cobrar) ─────────────────────────────────
  discountAmount = 0;

  // ── Confirmar ─────────────────────────────────────────────────────────────
  readonly confirming = signal(false);

  // ── Constantes ────────────────────────────────────────────────────────────
  readonly SaleItemType           = SaleItemType;
  readonly SalePaymentMethod      = SalePaymentMethod;
  readonly MemberStatus           = MemberStatus;
  readonly PAYMENT_METHOD_OPTIONS = SALE_PAYMENT_METHOD_OPTIONS;
  readonly ITEM_TYPE_OPTIONS      = SALE_ITEM_TYPE_OPTIONS;
  readonly SALE_ITEM_TYPE_LABELS  = SALE_ITEM_TYPE_LABELS;

  // ── Computed ──────────────────────────────────────────────────────────────
  readonly hasPlanItem = computed(() =>
    this.items().some(i => i.itemType === SaleItemType.Plan),
  );

  readonly subtotalAmount = computed(() =>
    this.items().reduce((sum, i) => sum + i.subtotal, 0),
  );

  /** Total mostrado en pantalla: subtotal local menos descuento local */
  readonly localTotal = computed(() =>
    Math.max(0, this.subtotalAmount() - this.discountAmount),
  );

  readonly canCobrar = computed(() =>
    this.selectedMember() != null && this.items().length > 0 && !this.confirming(),
  );

  ngOnInit(): void {
    this.memberSearch$
      .pipe(debounceTime(350), distinctUntilChanged(), takeUntil(this.destroy$))
      .subscribe(q => this.searchMembers(q));

    this.loadPlans();
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // ── Socio ─────────────────────────────────────────────────────────────────

  onMemberInput(value: string): void {
    this.memberSearch = value;
    if (value.trim().length < 2) {
      this.memberResults.set([]);
      return;
    }
    this.memberSearch$.next(value.trim());
  }

  private searchMembers(q: string): void {
    this.memberLoading.set(true);
    this.memberService.getMembers({ search: q, page: 1, pageSize: 10 }).subscribe({
      next: res => { this.memberResults.set(res.items); this.memberLoading.set(false); },
      error: ()  => { this.memberResults.set([]);        this.memberLoading.set(false); },
    });
  }

  selectMember(m: MemberSummaryDto): void {
    this.selectedMember.set(m);
    this.memberSearch = '';
    this.memberResults.set([]);
  }

  clearMember(): void {
    this.selectedMember.set(null);
    this.memberSearch = '';
    this.memberResults.set([]);
    this.items.set([]);
    this.discountAmount = 0;
    this.globalError.set(null);
    this.itemFormVisible.set(false);
  }

  // ── Planes ────────────────────────────────────────────────────────────────

  private loadPlans(): void {
    this.plansLoading.set(true);
    this.planService.getMembershipPlans({ isActive: true }).subscribe({
      next: plans => { this.plans.set(plans); this.plansLoading.set(false); },
      error: ()    => this.plansLoading.set(false),
    });
  }

  onPlanSelected(planId: string): void {
    const plan = this.plans().find(p => p.id === planId);
    if (plan) {
      this.newItemDescription = plan.name;
      this.newItemUnitPrice   = plan.amount;
    }
  }

  // ── Ítems (estado local del formulario) ────────────────────────────────────

  showItemForm(): void {
    this.resetItemForm();
    this.itemFormVisible.set(true);
    this.itemError.set(null);
  }

  hideItemForm(): void {
    this.itemFormVisible.set(false);
    this.itemError.set(null);
  }

  onItemTypeChange(): void {
    this.newItemPlanId      = '';
    this.newItemDescription = '';
    this.newItemUnitPrice   = 0;
    this.newItemQuantity    = 1;
  }

  addItem(): void {
    if (!this.newItemDescription.trim()) {
      this.itemError.set('La descripción es obligatoria.');
      return;
    }
    if (this.newItemUnitPrice <= 0) {
      this.itemError.set('El precio debe ser mayor a 0.');
      return;
    }
    if (this.newItemQuantity < 1) {
      this.itemError.set('La cantidad debe ser al menos 1.');
      return;
    }
    if (this.newItemType === SaleItemType.Plan) {
      if (!this.newItemPlanId) {
        this.itemError.set('Debes seleccionar un plan de membresía.');
        return;
      }
      if (this.hasPlanItem()) {
        this.itemError.set('Solo puede existir un ítem de tipo Plan por venta.');
        return;
      }
    }

    const newItem: LocalSaleItem = {
      id:               crypto.randomUUID(),
      itemType:         this.newItemType,
      description:      this.newItemDescription.trim(),
      membershipPlanId: this.newItemType === SaleItemType.Plan ? this.newItemPlanId : null,
      unitPrice:        this.newItemUnitPrice,
      quantity:         this.newItemQuantity,
      subtotal:         this.newItemUnitPrice * this.newItemQuantity,
    };

    this.items.update(items => [...items, newItem]);
    this.itemError.set(null);
    this.itemFormVisible.set(false);
  }

  removeItem(itemId: string): void {
    this.items.update(items => items.filter(i => i.id !== itemId));
  }

  // ── Cobrar ────────────────────────────────────────────────────────────────

  async onCobrar(): Promise<void> {
    const member = this.selectedMember();
    if (!member || !this.canCobrar()) return;

    const result = await this.dialog.confirmAction(
      'Confirmar cobro',
      `Total a cobrar: ${this.formatAmount(this.localTotal())}. ¿Confirmar la venta?`,
      'Sí, cobrar',
    );
    if (!result.isConfirmed) return;

    this.confirming.set(true);
    this.globalError.set(null);

    const request: CheckoutSaleRequest = {
      memberId:      member.id,
      date:          this.today(),
      paymentMethod: this.paymentMethod,
      notes:         this.saleNotes.trim() || null,
      items: this.items().map(i => ({
        itemType:         i.itemType,
        description:      i.description,
        membershipPlanId: i.membershipPlanId,
        unitPrice:        i.unitPrice,
        quantity:         i.quantity,
      })),
      discount: this.discountAmount > 0
        ? { discountType: SaleDiscountType.MontoFijo, description: 'Descuento', value: this.discountAmount }
        : null,
    };

    this.saleService.checkoutSale(request).subscribe({
      next: ({ id }) => {
        this.confirming.set(false);
        this.dialog.toast('Venta confirmada correctamente', 'success');
        this.router.navigate(['/app/sales', id]);
      },
      error: err => {
        const msg = err?.error?.detail ?? 'No se pudo confirmar la venta.';
        this.globalError.set(msg);
        this.confirming.set(false);
      },
    });
  }

  async cancelSale(): Promise<void> {
    if (this.selectedMember() || this.items().length > 0) {
      const result = await this.dialog.confirmDanger(
        '¿Cancelar venta?',
        'Se perderá el socio y los conceptos agregados.',
        'Sí, cancelar',
      );
      if (!result.isConfirmed) return;
    }

    this.router.navigate(['/app/sales']);
  }

  // ── Vigencia de membresía (calculada en frontend) ─────────────────────────

  vigenciaForItem(item: LocalSaleItem): string | null {
    if (item.itemType !== SaleItemType.Plan || !item.membershipPlanId) return null;
    const plan = this.plans().find(p => p.id === item.membershipPlanId);
    if (!plan) return null;
    const start = new Date();
    const end   = new Date();
    end.setDate(end.getDate() + plan.durationDays - 1);
    return `${this.formatDateShort(start)} - ${this.formatDateShort(end)}`;
  }

  // ── Helpers ───────────────────────────────────────────────────────────────

  private resetItemForm(): void {
    this.newItemType        = SaleItemType.Plan;
    this.newItemDescription = '';
    this.newItemPlanId      = '';
    this.newItemUnitPrice   = 0;
    this.newItemQuantity    = 1;
  }

  formatAmount(amount: number): string {
    return new Intl.NumberFormat('es-CL', { style: 'currency', currency: 'CLP' }).format(amount);
  }

  private formatDateShort(d: Date): string {
    return d.toLocaleDateString('es-CL', { day: '2-digit', month: '2-digit', year: 'numeric' });
  }

  private today(): string {
    return new Date().toISOString().split('T')[0];
  }
}
