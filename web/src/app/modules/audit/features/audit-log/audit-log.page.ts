import { Component, effect, inject, signal, untracked } from '@angular/core';
import { ListState, Page } from '../../../../shared/core/page';
import { Pager, SearchBox, SortHeader } from '../../../../shared/ui/list-controls';
import { AuditEntry, describeAction, targetLabel } from '../../core/audit-actions';
import { AuditApi } from '../../infrastructure/audit.api';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

@Component({
  selector: 'app-audit-log',
  imports: [SearchBox, SortHeader, Pager, PageHead],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Journal des actions" />
      <app-search-box label="Rechercher une personne ou une action…" (searched)="list.setSearch($event)" />
      @if (error()) {
        <p class="error">{{ error() }}</p>
      } @else if (!loaded()) {
        <p>Chargement…</p>
      } @else if (entries().items.length === 0) {
        <p>{{ list.search() ? 'Aucun résultat.' : 'Aucune action enregistrée pour le moment.' }}</p>
      } @else {
        <table>
          <thead>
            <tr>
              <th><app-sort-header [state]="list" field="date" label="Date" /></th>
              <th><app-sort-header [state]="list" field="actor" label="Par" /></th>
              <th><app-sort-header [state]="list" field="action" label="Action" /></th>
              <th>Personne concernée</th>
            </tr>
          </thead>
          <tbody>
            @for (e of entries().items; track e.id) {
              <tr>
                <td>{{ date(e.occurredAt) }}</td>
                <td>{{ e.actor }}</td>
                <td>{{ describe(e.action) }}</td>
                <td>{{ person(e) }}</td>
              </tr>
            }
          </tbody>
        </table>
        <app-pager [page]="entries().page" [totalPages]="entries().totalPages" [totalCount]="entries().totalCount" (pageChange)="list.goTo($event)" />
        <p class="small-text">Le journal ne peut être ni modifié ni effacé. Seuls des identifiants sont conservés pour les personnes concernées : si leur compte est supprimé, leur nom n'apparaît plus.</p>
      }
    </main>
  `,
})
export class AuditLogPage {
  protected readonly trail: Crumb[] = [{ label: 'Administration', path: '/admin' }, { label: 'Journal' }];
  private readonly api = inject(AuditApi);
  protected readonly list = new ListState('date', 'desc');
  protected readonly entries = signal<Page<AuditEntry>>({ items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 });
  protected readonly loaded = signal(false);
  protected readonly error = signal('');

  constructor() {
    effect(() => {
      const query = this.list.query();
      untracked(() => void this.load(query));
    });
  }

  private async load(query: ReturnType<ListState['query']>): Promise<void> {
    try {
      const page = await this.api.auditLog(query);
      if (page.items.length === 0 && page.page > 1) {
        this.list.goTo(page.totalPages); // the last page disappeared
        return;
      }
      this.entries.set(page);
      this.error.set('');
      this.loaded.set(true);
    } catch {
      this.error.set('Impossible de charger le journal.');
    }
  }

  protected date(iso: string): string {
    return new Date(iso).toLocaleString('fr-FR', { dateStyle: 'short', timeStyle: 'short' });
  }

  protected describe = describeAction;
  protected person = targetLabel;
}
