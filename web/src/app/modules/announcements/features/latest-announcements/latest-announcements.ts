import { Component, OnInit, inject, input, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { formatDate } from '../../../../shared/core/format';
import { AnnouncementItem } from '../../core/announcement';
import { AnnouncementsApi } from '../../infrastructure/announcements.api';

/**
 * The latest announcements, for the home page. A visitor only gets the public ones (the API decides), a member gets both.
 * `layout` chooses between the cards of the public page and the compact rows of the dashboard.
 */
@Component({
  selector: 'app-latest-announcements',
  imports: [RouterLink],
  template: `
    @if (failed()) {
      <p class="alert alert-error">
        <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg>
        <span><strong class="alert-title">Erreur.</strong> Les annonces n'ont pas pu être chargées.</span>
      </p>
    } @else if (!loaded()) {
      <div class="stack" aria-hidden="true">
        <span class="skeleton" style="height: 8rem"></span>
        <span class="skeleton" style="height: 8rem"></span>
      </div>
    } @else if (items().length === 0) {
      <p class="empty">
        <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M4 5h16v12H8l-4 4z"></path></svg>
        <strong>Aucune annonce</strong>
        <span>Les nouvelles du club s'afficheront ici.</span>
      </p>
    } @else {
      <!-- Same card as the announcements page: in the cards layout, news-list turns the rows into cards on a wide screen. -->
      <div [class]="layout() === 'cards' ? 'news-list' : 'stack stack-s'">
        @for (a of items(); track a.id) {
          <a class="news-row" [routerLink]="['/announcements', a.id]">
            @if (a.posterId) {
              <img [src]="api.fileUrl(a.id, a.posterId)" alt="" loading="lazy" />
            } @else {
              <span class="placeholder-img" aria-hidden="true">Annonce</span>
            }
            <span class="stack stack-s">
              @if (a.isPinned || a.kind === 'internal' || a.form === 'poll') {
                <span class="row" style="--row-gap: var(--space-5)">
                  @if (a.isPinned) { <span class="highlight highlight-primary">Épinglée</span> }
                  @if (a.form === 'poll') { <span class="highlight highlight-secondary">Sondage</span> }
                  @if (a.kind === 'internal') { <span class="highlight highlight-info">Adhérents</span> }
                </span>
              }
              <span class="news-title">{{ a.title }}</span>
              @if (a.summary) { <span class="small-text">{{ a.summary }}</span> }
              <span class="news-meta">
                @if (a.publishedAt) { <time>{{ date(a.publishedAt) }}</time> }
                @if (a.authorName) { <span>{{ a.publishedAt ? '· ' : '' }}{{ a.authorName }}</span> }
                @if (a.likeCount) { <span>· {{ a.likeCount }} j'aime</span> }
              </span>
            </span>
          </a>
        }
      </div>
    }
  `,
  styles: `:host { display: grid; gap: var(--space-15); }`,
})
export class LatestAnnouncements implements OnInit {
  readonly limit = input(3);
  readonly layout = input<'cards' | 'rows'>('rows');
  protected readonly api = inject(AnnouncementsApi);
  protected readonly items = signal<AnnouncementItem[]>([]);
  protected readonly loaded = signal(false);
  protected readonly failed = signal(false);
  protected readonly date = formatDate;

  async ngOnInit(): Promise<void> {
    try {
      // The board also sees its drafts in the list: the home page only shows what everybody sees.
      const page = await this.api.list({ status: 'published', page: 1, pageSize: this.limit() });
      this.items.set(page.items.filter((a) => !a.isExpired).slice(0, this.limit()));
    } catch {
      this.failed.set(true);
    } finally {
      this.loaded.set(true);
    }
  }
}
