import { Component, effect, inject, signal, untracked } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ListState, Page } from '../../../../shared/core/page';
import { formatDate } from '../../../../shared/core/format';
import { AdminActivity } from '../../../../shared/infrastructure/admin-activity';
import { Pager, SearchBox, SortHeader } from '../../../../shared/ui/list-controls';
import { AnnouncementItem } from '../../core/announcement';
import { AnnouncementsApi } from '../../infrastructure/announcements.api';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

/** Board page: every announcement (drafts and finished ones included) with the quick actions. */
@Component({
  selector: 'app-manage-announcements',
  imports: [RouterLink, SearchBox, SortHeader, Pager, PageHead],
  providers: [AdminActivity],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Gérer les annonces" />
      <p class="actions"><a class="btn" routerLink="/admin/announcements/new">Nouvelle annonce</a> <a class="btn btn-ghost" routerLink="/admin/announcements/new-poll">Nouveau sondage</a></p>

      @if (activity.message()) { <p class="success">{{ activity.message() }}</p> }
      @if (activity.error()) { <p class="error">{{ activity.error() }}</p> }

      <div class="filters">
        <app-search-box label="Rechercher une annonce…" (searched)="list.setSearch($event)" />
        <select class="select input" aria-label="Filtrer par état" [value]="status()" (change)="setStatus($any($event.target).value)">
          <option value="">Tous les états</option>
          <option value="published">Publiées</option>
          <option value="draft">Brouillons</option>
        </select>
        <select class="select input" aria-label="Filtrer par type" [value]="kind()" (change)="setKind($any($event.target).value)">
          <option value="">Tous les types</option>
          <option value="internal">Internes</option>
          <option value="public">Publiques</option>
        </select>
      </div>

      @if (!loaded()) {
        <p>Chargement…</p>
      } @else if (announcements().items.length === 0) {
        <p>Aucune annonce.</p>
      } @else {
        <div class="scroll">
          <table>
            <thead>
              <tr>
                <th><app-sort-header [state]="list" field="title" label="Titre" /></th>
                <th>Type</th>
                <th><app-sort-header [state]="list" field="publishedAt" label="État" /></th>
                <th>Fin</th>
                <th>👍</th>
                <th>Auteur</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              @for (a of announcements().items; track a.id) {
                <tr>
                  <td><a [routerLink]="['/announcements', a.id]">{{ a.title }}</a> @if (a.isPinned) { <span title="Épinglée">📌</span> } @if (a.form === 'poll') { <span title="Sondage">📊</span> }</td>
                  <td>{{ a.kind === 'internal' ? 'Interne' : 'Publique' }}</td>
                  <td>
                    {{ a.isPublished ? 'Publiée' : 'Brouillon' }}
                    @if (a.publishedAt) { <span class="meta">le {{ date(a.publishedAt) }}</span> }
                  </td>
                  <td [class.expired]="a.isExpired">{{ date(a.expiresOn) }}@if (a.isExpired) { <span class="meta"> (terminée)</span> }</td>
                  <td>{{ a.likeCount ?? 0 }}</td>
                  <td>{{ a.authorName }}</td>
                  <td>
                    <div class="actions">
                      @if (activity.isConfirming(a.id)) {
                        <span>Supprimer ?</span>
                        <button class="btn btn-danger btn-sm" [class.is-busy]="activity.busy() === a.id" [disabled]="activity.busy() !== null" (click)="remove(a)">Oui</button>
                        <button class="btn btn-ghost btn-sm" [disabled]="activity.busy() !== null" (click)="activity.confirming.set(null)">Non</button>
                      } @else {
                        <a class="btn btn-ghost btn-sm" [routerLink]="['/admin/announcements', a.id, 'edit']">Modifier</a>
                        <button class="btn btn-ghost btn-sm" [class.is-busy]="activity.busy() === a.id" [disabled]="activity.busy() !== null" (click)="togglePublished(a)">{{ a.isPublished ? 'Dépublier' : 'Publier' }}</button>
                        <button class="btn btn-ghost btn-sm" [disabled]="activity.busy() !== null" (click)="togglePinned(a)">{{ a.isPinned ? 'Désépingler' : 'Épingler' }}</button>
                        <button class="btn btn-danger btn-sm" [disabled]="activity.busy() !== null" (click)="activity.confirming.set(a.id + ':delete')">Supprimer</button>
                      }
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        </div>
        <app-pager [page]="announcements().page" [totalPages]="announcements().totalPages" [totalCount]="announcements().totalCount" (pageChange)="list.goTo($event)" />
        <p class="meta">Une annonce terminée disparaît du site le lendemain de sa date de fin, puis elle est supprimée automatiquement. Pour la garder, modifiez sa date de fin.</p>
      }
    </main>
  `,
  styles: `
    .scroll { overflow-x: auto; }
    .meta { color: var(--color-text-muted); font-size: var(--text-small); }
    .expired { color: #a11; }
  `,
})
export class ManageAnnouncementsPage {
  protected readonly trail: Crumb[] = [{ label: 'Administration', path: '/admin' }, { label: 'Annonces' }];
  private readonly api = inject(AnnouncementsApi);
  protected readonly activity = inject(AdminActivity);
  protected readonly list = new ListState('publishedAt', 'desc', 25);
  protected readonly status = signal('');
  protected readonly kind = signal('');
  protected readonly announcements = signal<Page<AnnouncementItem>>({ items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 });
  protected readonly loaded = signal(false);
  protected readonly date = formatDate;

  constructor() {
    // Loads at start, after every action, and when the search, the filters, the sort or the page change.
    effect(() => {
      this.activity.changed();
      const query = { ...this.list.query(), status: this.status(), kind: this.kind() };
      untracked(() => void this.reload(query));
    });
  }

  protected setStatus(value: string): void {
    this.status.set(value);
    this.list.goTo(1);
  }

  protected setKind(value: string): void {
    this.kind.set(value);
    this.list.goTo(1);
  }

  protected async togglePublished(a: AnnouncementItem): Promise<void> {
    await this.activity.run(a.id, () => (a.isPublished ? this.api.unpublish(a.id) : this.api.publish(a.id)),
      a.isPublished ? `« ${a.title} » n'est plus publiée.` : `« ${a.title} » est publiée.`);
  }

  protected async togglePinned(a: AnnouncementItem): Promise<void> {
    await this.activity.run(a.id, () => (a.isPinned ? this.api.unpin(a.id) : this.api.pin(a.id)),
      a.isPinned ? `« ${a.title} » n'est plus épinglée.` : `« ${a.title} » est épinglée en haut de la liste.`);
  }

  protected async remove(a: AnnouncementItem): Promise<void> {
    await this.activity.run(a.id, () => this.api.remove(a.id), `Annonce « ${a.title} » supprimée.`);
  }

  private async reload(query: Parameters<AnnouncementsApi['list']>[0]): Promise<void> {
    try {
      const page = await this.api.list(query);
      if (page.items.length === 0 && page.page > 1) {
        this.list.goTo(page.totalPages); // the last page disappeared
        return;
      }
      this.announcements.set(page);
      this.loaded.set(true);
    } catch {
      this.activity.error.set('Impossible de charger les annonces.');
    }
  }
}
