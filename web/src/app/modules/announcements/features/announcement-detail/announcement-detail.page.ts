import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { apiErrorMessage, formatDate } from '../../../../shared/core/format';
import { AuthService } from '../../../../shared/infrastructure/auth.service';
import { Announcement, AnnouncementFile } from '../../core/announcement';
import { formatSize, splitBody } from '../../core/media-links';
import { AnnouncementsApi } from '../../infrastructure/announcements.api';
import { SharePanel } from '../../../../shared/ui/share-panel';
import { AnnouncementComments } from '../announcement-comments/announcement-comments';
import { AnnouncementPoll } from '../announcement-poll/announcement-poll';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

/** A part of the text: HTML, or a video with its player address. */
type PartView = { kind: 'html'; html: string } | { kind: 'video'; url: string; provider: string; player: SafeResourceUrl };

/** One announcement: text, poster, videos, links, attachments, like button, and the board's actions. */
@Component({
  selector: 'app-announcement-detail',
  imports: [RouterLink, AnnouncementPoll, AnnouncementComments, SharePanel, PageHead],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60); max-width: 84rem">
      @switch (state()) {
        @case ('loading') {
          <app-page-head [trail]="trail" title="Annonce" />
          <span class="skeleton" style="height: 24rem" aria-hidden="true"></span>
        }
        @case ('missing') {
          <app-page-head [trail]="trail" title="Annonce introuvable" />
          <p>Cette annonce n'existe pas, n'est plus affichée ou est réservée aux adhérents.</p>
          @if (!auth.signedIn()) {
            <p><button class="btn" [class.is-busy]="auth.busy()" [disabled]="auth.busy()" (click)="auth.signIn(location.pathname)">Se connecter</button></p>
          }
        }
        @case ('failed') {
          <app-page-head [trail]="trail" title="Annonce" />
          <p class="alert alert-error">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 8v5M12 16h.01"></path><circle cx="12" cy="12" r="9"></circle></svg>
            <span>Impossible de charger l'annonce.</span>
          </p>
        }
        @case ('ready') {
          @if (announcement(); as a) {
            <app-page-head [trail]="trail" [title]="a.title" />
            <p class="badges">
              @if (a.isPinned) { <span class="highlight highlight-primary">📌 Épinglée</span> }
              <span class="highlight highlight-neutral">{{ a.kind === 'internal' ? 'Réservée aux adhérents' : 'Publique' }}</span>
              @if (!a.isPublished) { <span class="highlight highlight-warning">Brouillon (invisible du public)</span> }
              @if (a.isExpired) { <span class="highlight highlight-warning">Terminée : sera supprimée automatiquement</span> }
            </p>
            <p class="meta">
              {{ a.publishedAt ? 'Publiée le ' + date(a.publishedAt) : 'Non publiée' }}
              · affichée jusqu'au {{ date(a.expiresOn) }}
              @if (a.authorName) { · par {{ a.authorName }} }
            </p>

            @if (poster(); as p) {
              <img class="poster" [src]="api.fileUrl(a.id, p.id)" [alt]="'Affiche : ' + a.title" />
            }

            <div class="text rich-text">
              @for (part of parts(); track $index) {
                @if (part.kind === 'html') {
                  <div [innerHTML]="part.html"></div>
                } @else {
                  <div class="video">
                    @if (isPlaying(part.url)) {
                      <iframe [src]="part.player" [title]="'Vidéo ' + part.provider" allowfullscreen loading="lazy" referrerpolicy="strict-origin-when-cross-origin"
                              allow="encrypted-media; picture-in-picture; fullscreen"></iframe>
                    } @else {
                      <p>🎬 Vidéo {{ part.provider }} : <a [href]="part.url" target="_blank" rel="noopener noreferrer">{{ part.url }}</a></p>
                      <button class="btn btn-ghost" (click)="play(part.url)">Lire la vidéo</button>
                      <p class="small-text">En lisant la vidéo, des données sont échangées avec {{ part.provider }}.</p>
                    }
                  </div>
                }
              }
            </div>

            @if (a.poll; as poll) {
              <app-announcement-poll [announcementId]="a.id" [poll]="poll" (changed)="refresh()" />
            }

            @if (attachments().length > 0) {
              <h2>Pièces jointes</h2>
              <ul class="files">
                @for (f of attachments(); track f.id) {
                  <li>📎 <a [href]="api.fileUrl(a.id, f.id)" download>{{ f.fileName }}</a> <span class="meta">({{ size(f.sizeBytes) }})</span></li>
                }
              </ul>
            }

            @if (a.isPublished) {
              <div class="row" style="--row-gap: var(--space-20)">
                @if (a.likeCount !== null) {
                  <button class="btn btn-outline like" [attr.aria-pressed]="a.likedByMe" [class.is-busy]="busy() === 'like'" [disabled]="busy() !== null" (click)="toggleLike(a)">
                    <svg class="ic" viewBox="0 0 24 24" aria-hidden="true" [style.fill]="a.likedByMe ? 'var(--current)' : 'none'">
                      <path d="M12 20s-7-4.5-7-10a4 4 0 0 1 7-2.5A4 4 0 0 1 19 10c0 5.5-7 10-7 10z"></path>
                    </svg>
                    J'aime · {{ a.likeCount }}
                  </button>
                }
                <app-share-panel [url]="shareUrl(a.id)" [title]="a.title" [note]="a.kind === 'internal' ? 'Réservée aux adhérents : les personnes qui reçoivent ce lien devront se connecter avec leur compte du club.' : ''" />
              </div>
            }

            @if (a.likeCount !== null && a.isPublished && a.commentsEnabled) {
              <app-announcement-comments [announcementId]="a.id" />
            }

            @if (auth.isBoard()) {
              <hr />
              <div class="form-actions">
                <a class="btn btn-ghost" [routerLink]="['/admin/announcements', a.id, 'edit']">Modifier</a>
                <button class="btn btn-ghost" [class.is-busy]="busy() === 'publish'" [disabled]="busy() !== null" (click)="run('publish', a.isPublished ? api.unpublish(a.id) : api.publish(a.id))">
                  {{ a.isPublished ? 'Dépublier' : 'Publier' }}
                </button>
                <button class="btn btn-ghost" [class.is-busy]="busy() === 'pin'" [disabled]="busy() !== null" (click)="run('pin', a.isPinned ? api.unpin(a.id) : api.pin(a.id))">
                  {{ a.isPinned ? 'Désépingler' : 'Épingler en haut' }}
                </button>
                @if (confirmDelete()) {
                  <span>Supprimer définitivement cette annonce ?</span>
                  <button class="btn btn-danger" [class.is-busy]="busy() === 'delete'" [disabled]="busy() !== null" (click)="remove(a)">Oui, supprimer</button>
                  <button class="btn btn-ghost" [disabled]="busy() !== null" (click)="confirmDelete.set(false)">Annuler</button>
                } @else {
                  <button class="btn btn-danger" [disabled]="busy() !== null" (click)="confirmDelete.set(true)">Supprimer</button>
                }
              </div>
            }
            @if (error()) {
              <p class="alert alert-error">
                <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 8v5M12 16h.01"></path><circle cx="12" cy="12" r="9"></circle></svg>
                <span>{{ error() }}</span>
              </p>
            }
          }
        }
      }
    </main>
  `,
  styles: `
    .meta { color: var(--color-text-muted); font-size: var(--text-small); }
    .poster { display: block; max-width: 100%; max-height: 51.2rem; border-radius: var(--radius-m); }
    .text { overflow-wrap: anywhere; }
    .text blockquote { margin: var(--space-10) 0; padding-left: var(--space-15); border-left: 0.3rem solid var(--color-border-strong); color: var(--color-text-muted); }
    .video { padding: var(--space-15); background: var(--color-surface-subtle); border-radius: var(--radius-m); }
    .video iframe { width: 100%; aspect-ratio: 16 / 9; border: 0; border-radius: var(--radius-m); }
    .files { list-style: none; display: grid; gap: var(--space-10); }
  `,
})
export class AnnouncementDetailPage {
  protected readonly trail: Crumb[] = [{ label: 'Annonces', path: '/announcements' }, { label: 'Détail' }];
  protected readonly api = inject(AnnouncementsApi);
  protected readonly auth = inject(AuthService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly sanitizer = inject(DomSanitizer);
  protected readonly location = location;

  protected readonly announcement = signal<Announcement | null>(null);
  protected readonly state = signal<'loading' | 'ready' | 'missing' | 'failed'>('loading');
  /** Which action is running ("like", "publish", "pin", "delete"); the buttons are disabled meanwhile. */
  protected readonly busy = signal<string | null>(null);
  protected readonly error = signal('');
  protected readonly confirmDelete = signal(false);
  private readonly playing = signal<string[]>([]);

  protected readonly poster = computed<AnnouncementFile | undefined>(() => this.announcement()?.files.find((f) => f.isPoster));
  protected readonly attachments = computed<AnnouncementFile[]>(() => this.announcement()?.files.filter((f) => !f.isPoster) ?? []);
  /** The text, cut where a video link stands alone on its line (the player address is built from a checked identifier on a known host). */
  protected readonly parts = computed<PartView[]>(() =>
    splitBody(this.announcement()?.bodyHtml ?? '').map((part) =>
      part.kind === 'html'
        ? part
        : { kind: 'video', url: part.video.url, provider: part.video.provider, player: this.sanitizer.bypassSecurityTrustResourceUrl(part.video.embedUrl) },
    ),
  );

  protected readonly date = formatDate;
  protected readonly size = formatSize;

  /** Address of the announcement to share (the same page for everybody; the API decides who can read it). */
  protected shareUrl(id: string): string {
    return `${location.origin}/announcements/${id}`;
  }

  constructor() {
    this.route.paramMap.pipe(takeUntilDestroyed(inject(DestroyRef))).subscribe((params) => void this.load(params.get('id') ?? ''));
  }

  /** Reloads the announcement (after an answer to the poll). */
  protected async refresh(): Promise<void> {
    try {
      this.announcement.set(await this.api.get(this.announcement()!.id));
    } catch {
      this.error.set("Impossible d'actualiser l'annonce.");
    }
  }

  protected isPlaying(url: string): boolean {
    return this.playing().includes(url);
  }

  /** The player is only loaded when the person asks for it (nothing is sent to YouTube / Vimeo before). */
  protected play(url: string): void {
    this.playing.update((urls) => [...urls, url]);
  }

  protected async toggleLike(a: Announcement): Promise<void> {
    await this.run('like', a.likedByMe ? this.api.unlike(a.id) : this.api.like(a.id));
  }

  protected async remove(a: Announcement): Promise<void> {
    if (await this.run('delete', this.api.remove(a.id), false)) {
      await this.router.navigateByUrl('/announcements');
    }
  }

  /** Runs an action, then reloads the announcement. Returns true if it succeeded. */
  protected async run(action: string, call: Promise<unknown>, reload = true): Promise<boolean> {
    this.error.set('');
    this.busy.set(action);
    try {
      await call;
      if (reload) {
        this.announcement.set(await this.api.get(this.announcement()!.id));
      }
      return true;
    } catch (e) {
      this.error.set(apiErrorMessage(e, "L'action a échoué. Rechargez la page et réessayez."));
      return false;
    } finally {
      this.busy.set(null);
    }
  }

  private async load(id: string): Promise<void> {
    this.state.set('loading');
    this.confirmDelete.set(false);
    this.playing.set([]);
    try {
      this.announcement.set(await this.api.get(id));
      this.state.set('ready');
    } catch (e) {
      this.state.set(e instanceof HttpErrorResponse && (e.status === 404 || e.status === 400) ? 'missing' : 'failed');
    }
  }
}
