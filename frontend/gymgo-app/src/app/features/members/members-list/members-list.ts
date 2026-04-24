import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Subject, debounceTime, distinctUntilChanged } from 'rxjs';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import {
  MemberSummaryDto,
  MemberStatus,
  MEMBER_STATUS_LABELS,
  MEMBER_STATUS_OPTIONS,
} from '../models/member.models';
import { MemberService } from '../services/member.service';
import { DialogService } from '../../../core/services/dialog.service';

const PAGE_SIZE = 20;

@Component({
  selector: 'app-members-list',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './members-list.html',
})
export class MembersListComponent implements OnInit {
  private readonly memberService = inject(MemberService);
  private readonly dialog        = inject(DialogService);
  private readonly router        = inject(Router);

  readonly loading     = signal(false);
  readonly error       = signal<string | null>(null);
  readonly members     = signal<MemberSummaryDto[]>([]);
  readonly currentPage = signal(1);
  readonly totalPages  = signal(1);
  readonly totalCount  = signal(0);

  readonly MemberStatus        = MemberStatus;
  readonly statusLabels        = MEMBER_STATUS_LABELS;
  readonly statusFilterOptions = MEMBER_STATUS_OPTIONS;

  private searchValue  = '';
  private statusFilter: MemberStatus | null = null;

  private readonly searchSubject = new Subject<string>();

  constructor() {
    this.searchSubject
      .pipe(debounceTime(350), distinctUntilChanged(), takeUntilDestroyed())
      .subscribe((value) => {
        this.searchValue = value;
        this.currentPage.set(1);
        this.loadMembers();
      });
  }

  ngOnInit(): void {
    this.loadMembers();
  }

  loadMembers(): void {
    this.loading.set(true);
    this.error.set(null);

    this.memberService
      .getMembers({
        search:   this.searchValue,
        status:   this.statusFilter,
        page:     this.currentPage(),
        pageSize: PAGE_SIZE,
      })
      .subscribe({
        next: (result) => {
          this.members.set(result.items);
          this.totalPages.set(result.totalPages);
          this.totalCount.set(result.totalCount);
          this.loading.set(false);
        },
        error: () => {
          this.error.set('No se pudo cargar la lista de socios. Intenta nuevamente.');
          this.loading.set(false);
        },
      });
  }

  onSearch(value: string): void {
    this.searchSubject.next(value);
  }

  onStatusFilterChange(value: string): void {
    this.statusFilter = value === '' ? null : (Number(value) as MemberStatus);
    this.currentPage.set(1);
    this.loadMembers();
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages()) return;
    this.currentPage.set(page);
    this.loadMembers();
  }

  async editMember(member: MemberSummaryDto): Promise<void> {
    const result = await this.dialog.confirmAction(
      'Editar socio',
      `¿Deseas editar los datos de ${member.fullName}?`,
      'Ir a editar',
    );
    if (result.isConfirmed) {
      this.router.navigate(['/app/members', member.id, 'edit']);
    }
  }

  async deleteMember(member: MemberSummaryDto): Promise<void> {
    const result = await this.dialog.confirmDanger(
      '¿Dar de baja al socio?',
      `Esta acción dará de baja a ${member.fullName} (RUT ${member.rut}). El registro se conserva pero no estará activo.`,
      'Sí, dar de baja',
    );
    if (!result.isConfirmed) return;

    this.memberService.deleteMember(member.id).subscribe({
      next: () => {
        this.dialog.toast(`${member.fullName} fue dado de baja`, 'success');
        this.loadMembers();
      },
      error: () => this.error.set('No se pudo dar de baja al socio. Intenta nuevamente.'),
    });
  }
}
