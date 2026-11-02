import { Component, effect, inject, signal, untracked } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ListState, Page } from '../../../../shared/core/page';
import { formatDate } from '../../../../shared/core/format';
import { AuthService } from '../../../../shared/infrastructure/auth.service';
import { Pager, SearchBox, SortSelect } from '../../../../shared/ui/list-controls';
import { AnnouncementItem } from '../../core/announcement';
import { AnnouncementsApi } from '../../infrastructure/announcements.api';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

/** Public page: the announcements the visitor may read (the API decides), the pinned ones first. */
@Component({
  selector: 'app-announcement-list',
  imports: [RouterLink, SearchBox, SortSelect, Pager, PageHead],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Annonces" lead="Les nouvelles du club, les épinglées d'abord." />

      @if (auth.isBoard()) {
        <p class="row">
          <a class="btn" routerLink="/admin/announcements/new">
            <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 5v14M5 12h14"></path></svg>
            Nouvelle annonce
          </a>
          <a class="btn btn-ghost" routerLink="/admin/announcements">Gérer les annonces</a>
        </p>
      } @else if (!auth.signedIn()) {
        <p class="alert alert-info">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="9"></circle><path d="M12 11v5M12 8h.01"></path></svg>
          <span>
            Vous êtes adhérent ?
            <button class="btn btn-sm btn-outline" [class.is-busy]="auth.busy()" [disabled]="auth.busy()" (click)="auth.signIn('/announcements')">Se connecter</button>
            pour voir aussi les annonces réservées aux adhérents.
          </span>
        </p>
      }

      <div class="list-tools cols cols-2" style="align-items: end">
        <app-search-box label="Titre ou mot du texte" (searched)="list.setSearch($event)" />
        <app-sort-select [state]="list" [fields]="sortFields" />
      </div>

      @if (failed()) {
        <p class="alert alert-error">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg>
          <span><span class="alert-title">Erreur.</span> Les annonces n'ont pas pu être chargées.</span>
        </p>
      } @else if (!loaded()) {
        <div class="stack" aria-hidden="true"><span class="skeleton" style="height: 12rem"></span><span class="skeleton" style="height: 12rem"></span></div>
      } @else if (announcements().items.length === 0) {
        <p class="empty">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M4 5h16v12H8l-4 4z"></path></svg>
          <strong>Aucune annonce</strong>
          <span>{{ list.search() ? "Essayez une autre recherche." : "Les nouvelles du club s'afficheront ici." }}</span>
        </p>
      } @else {
        <ul class="news-list" aria-label="Annonces">
          @for (a of announcements().items; track a.id) {
            <li>
              <a class="news-row" [routerLink]="['/announcements', a.id]">
                @if (a.posterId) {
                  <img [src]="api.fileUrl(a.id, a.posterId)" alt="" loading="lazy" />
                } @else {
                  <span class="placeholder-img" aria-hidden="true">Annonce</span>
                }
                <span class="stack stack-s">
                  @if (a.isPinned || a.form === 'poll' || a.kind === 'internal' || a.isExpired || !a.isPublished) {
                    <span class="row" style="--row-gap: var(--space-5)">
                      @if (a.isPinned) { <span class="highlight highlight-primary">Épinglée</span> }
                      @if (a.form === 'poll') { <span class="highlight highlight-secondary">Sondage</span> }
                      @if (a.kind === 'internal') { <span class="highlight highlight-info">Réservée aux adhérents</span> }
                      @if (auth.isBoard() && !a.isPublished) { <span class="highlight highlight-warning">Brouillon</span> }
                      @if (a.isExpired) { <span class="highlight highlight-neutral">Terminée</span> }
                    </span>
                  }
                  <span class="news-title">{{ a.title }}</span>
                  <span class="small-text">{{ a.summary }}</span>
                  <span class="news-meta">
                    @if (a.publishedAt) { <time>{{ date(a.publishedAt) }}</time> } @else { <span>Non publiée</span> }
                    @if (a.authorName) { <span>· {{ a.authorName }}</span> }
                    @if (a.attachmentCount > 0) { <span>· {{ a.attachmentCount }} pièce{{ a.attachmentCount > 1 ? 's' : '' }} jointe{{ a.attachmentCount > 1 ? 's' : '' }}</span> }
                    @if (a.likeCount) { <span>· {{ a.likeCount }} j'aime</span> }
                  </span>
                </span>
              </a>
            </li>
          }
        </ul>
        <app-pager [page]="announcements().page" [totalPages]="announcements().totalPages" [totalCount]="announcements().totalCount" (pageChange)="list.goTo($event)" />
      }
    </main>
  `,
  styles: `
    ul { list-style: none; }
    .news-row img { width: 100%; height: 100%; object-fit: cover; border-radius: var(--radius-s); aspect-ratio: 1; }
    @container vp (min-width: 76.8rem) { .news-list .news-row img { aspect-ratio: 16 / 9; max-height: 18rem; } }
  `,
})
export class AnnouncementListPage {
  protected readonly trail: Crumb[] = [];
  protected readonly api = inject(AnnouncementsApi);
  protected readonly auth = inject(AuthService);
  protected readonly list = new ListState('publishedAt', 'desc', 10);
  protected readonly announcements = signal<Page<AnnouncementItem>>({ items: [], page: 1, pageSize: 10, totalCount: 0, totalPages: 0 });
  protected readonly loaded = signal(false);
  protected readonly failed = signal(false);
  protected readonly date = formatDate;
  protected readonly sortFields = [
    { field: 'publishedAt', label: 'Date de publication' },
    { field: 'title', label: 'Titre' },
  ];

  constructor() {
    effect(() => {
      const query = this.list.query();
      untracked(() => void this.reload(query));
    });
  }

  private async reload(query: ReturnType<ListState['query']>): Promise<void> {
    try {
      this.announcements.set(await this.api.list(query));
      this.failed.set(false);
      this.loaded.set(true);
    } catch {
      this.failed.set(true);
    }
  }
}
