import { Component, effect, inject, input, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { apiErrorMessage } from '../../../../shared/core/format';
import { AnnouncementComment } from '../../core/announcement';
import { AnnouncementsApi } from '../../infrastructure/announcements.api';

const PAGE_SIZE = 20;
const MAX_LENGTH = 1000;

/**
 * Conversation under a published announcement, chat style: members and the board write; an answer quotes the message it answers
 * (click the quote to jump to it); the author or the board deletes a message (the answers to it stay, quoting "Message supprimé").
 */
@Component({
  selector: 'app-announcement-comments',
  imports: [FormsModule],
  template: `
    <section class="stack" aria-label="Commentaires">
      <div class="block-head">
        <h2 id="comments-title">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M4 5h16v11H9l-5 4z"></path></svg>
          Commentaires @if (loaded()) { <span class="small-text">({{ total() }})</span> }
        </h2>
      </div>

      @if (error()) {
        <p class="alert alert-error">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg>
          <span>{{ error() }}</span>
        </p>
      }

      @if (!loaded()) {
        <div class="stack" aria-hidden="true"><span class="skeleton" style="height: 7rem"></span><span class="skeleton" style="height: 7rem"></span></div>
      } @else {
        @if (comments().length === 0) {
          <p class="empty">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M4 5h16v11H9l-5 4z"></path></svg>
            <strong>Aucun commentaire</strong>
            <span>Lancez la discussion ci-dessous.</span>
          </p>
        }
        <ol class="thread">
          @for (c of comments(); track c.id) {
            <li [id]="'comment-' + c.id" [class.mine]="c.isMine" [class.flash]="flashing() === c.id">
              <article class="comment" [class.comment-mine]="c.isMine">
                <p class="news-meta" style="margin: 0">
                  <strong style="color: var(--color-text)">{{ c.isMine ? 'Vous' : c.authorName }}</strong>
                  <time>{{ when(c.createdAt) }}</time>
                </p>
                @if (c.replyTo; as q) {
                  <button type="button" class="quote quote-link" [disabled]="q.isDeleted" (click)="jumpTo(q.id)"
                          [attr.aria-label]="q.isDeleted ? 'Message supprimé' : 'Voir le message cité de ' + q.authorName">
                    @if (q.isDeleted) { <em>Message supprimé</em> } @else { <strong>{{ q.authorName }} :</strong> {{ q.excerpt }} }
                  </button>
                }
                <p class="text">{{ c.text }}</p>
                <p class="form-actions">
                  <button type="button" class="btn btn-ghost btn-sm" [disabled]="busy() !== null" (click)="startReply(c)">Répondre</button>
                  @if (c.canDelete) {
                    @if (confirming() === c.id) {
                      <button type="button" class="btn btn-danger btn-sm" [class.is-busy]="busy() === c.id" [disabled]="busy() !== null" (click)="remove(c.id)">Confirmer la suppression</button>
                      <button type="button" class="btn btn-ghost btn-sm" [disabled]="busy() !== null" (click)="confirming.set(null)">Annuler</button>
                    } @else {
                      <button type="button" class="btn btn-ghost btn-sm" [disabled]="busy() !== null" (click)="confirming.set(c.id)">Supprimer</button>
                    }
                  }
                </p>
              </article>
            </li>
          }
        </ol>
        @if (comments().length < total()) {
          <p><button type="button" class="btn btn-outline btn-sm" [class.is-busy]="busy() === 'more'" [disabled]="busy() !== null" (click)="more()">Voir plus de commentaires</button></p>
        }

        <form class="card stack stack-s" (ngSubmit)="add()">
          @if (replyingTo(); as r) {
            <p class="quote reply-head">
              <span><strong>Réponse à {{ r.authorName }} :</strong> {{ short(r.text) }}</span>
              <button type="button" class="btn btn-ghost btn-icon btn-sm" (click)="replyingTo.set(null)" aria-label="Annuler la réponse">
                <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><path d="M6 6l12 12M18 6L6 18"></path></svg>
              </button>
            </p>
          }
          <div class="field">
            <label class="label" for="comment">Votre commentaire</label>
            <textarea class="textarea input" id="comment" name="comment" rows="3" [maxlength]="max" [(ngModel)]="text" placeholder="Écrivez votre commentaire…"></textarea>
          </div>
          <p class="form-actions">
            <button class="btn" type="submit" [class.is-busy]="busy() === 'add'" [disabled]="busy() !== null || !text.trim()">
              {{ replyingTo() ? 'Envoyer la réponse' : 'Publier le commentaire' }}
            </button>
            <span class="small-text">{{ text.length }} / {{ max }}</span>
          </p>
        </form>
      }
    </section>
  `,
  styles: `
    :host { display: block; }
    h2 { display: flex; align-items: center; gap: var(--space-10); font-size: var(--text-h4); }
    .thread { display: grid; gap: var(--space-15); list-style: none; }
    /* My own messages lean to the right, like a conversation; the others stay on the left. */
    .mine .comment { background: var(--primary-tint); margin-left: var(--space-40); }
    @container vp (min-width: 76.8rem) { .mine .comment { margin-left: 20%; } .thread li:not(.mine) .comment { margin-right: 20%; } }
    .text { white-space: pre-wrap; overflow-wrap: anywhere; }
    .quote-link { display: block; width: 100%; text-align: left; border: 0; border-left: 0.3rem solid var(--color-border-strong); cursor: pointer; font: inherit; }
    .quote-link:disabled { cursor: default; }
    .reply-head { display: flex; align-items: center; justify-content: space-between; gap: var(--space-10); }
    .flash .comment { outline: var(--focus-ring); outline-offset: var(--focus-ring-offset); }
  `,
})
export class AnnouncementComments {
  readonly announcementId = input.required<string>();

  private readonly api = inject(AnnouncementsApi);
  protected readonly max = MAX_LENGTH;
  protected readonly comments = signal<AnnouncementComment[]>([]);
  protected readonly total = signal(0);
  protected readonly loaded = signal(false);
  protected readonly busy = signal<string | null>(null);
  protected readonly confirming = signal<string | null>(null);
  /** The message the next comment answers (shown above the field, WhatsApp style). */
  protected readonly replyingTo = signal<AnnouncementComment | null>(null);
  protected readonly flashing = signal<string | null>(null);
  protected readonly error = signal('');
  protected text = '';
  private page = 1;

  constructor() {
    effect(() => {
      const id = this.announcementId();
      untracked(() => void this.reload(id));
    });
  }

  protected when(iso: string): string {
    return new Date(iso).toLocaleString('fr-FR', { dateStyle: 'long', timeStyle: 'short' });
  }

  protected short(text: string): string {
    return text.length > 120 ? text.slice(0, 120).trimEnd() + '…' : text;
  }

  protected startReply(comment: AnnouncementComment): void {
    this.replyingTo.set(comment);
    document.getElementById('comment')?.focus();
  }

  /** Scrolls to the quoted message when it is on the page; otherwise loads the next pages until it appears. */
  protected async jumpTo(commentId: string): Promise<void> {
    while (!document.getElementById('comment-' + commentId) && this.comments().length < this.total()) {
      await this.more();
    }
    const element = document.getElementById('comment-' + commentId);
    if (element) {
      element.scrollIntoView({ behavior: 'smooth', block: 'center' });
      this.flashing.set(commentId);
      setTimeout(() => this.flashing.set(null), 1500);
    }
  }

  protected async add(): Promise<void> {
    await this.act('add', async () => {
      await this.api.addComment(this.announcementId(), this.text.trim(), this.replyingTo()?.id);
      this.text = '';
      this.replyingTo.set(null);
      await this.reload(this.announcementId());
    });
  }

  protected async remove(commentId: string): Promise<void> {
    await this.act(commentId, async () => {
      await this.api.deleteComment(this.announcementId(), commentId);
      this.confirming.set(null);
      if (this.replyingTo()?.id === commentId) {
        this.replyingTo.set(null);
      }
      await this.reload(this.announcementId());
    });
  }

  protected async more(): Promise<void> {
    await this.act('more', async () => {
      const next = await this.api.listComments(this.announcementId(), { sort: 'createdAt', direction: 'asc', page: this.page + 1, pageSize: PAGE_SIZE });
      this.page = next.page;
      this.comments.update((all) => [...all, ...next.items]);
      this.total.set(next.totalCount);
    });
    await new Promise((resolve) => setTimeout(resolve));
  }

  /** Reloads from the first page up to the page already shown, so that nothing jumps after an addition or a deletion. */
  private async reload(id: string): Promise<void> {
    const shown = Math.max(this.page, 1);
    const all: AnnouncementComment[] = [];
    let totalCount = 0;
    try {
      for (let p = 1; p <= shown; p++) {
        const page = await this.api.listComments(id, { sort: 'createdAt', direction: 'asc', page: p, pageSize: PAGE_SIZE });
        all.push(...page.items);
        totalCount = page.totalCount;
        this.page = p;
        if (all.length >= totalCount) {
          break;
        }
      }
      this.comments.set(all);
      this.total.set(totalCount);
      this.loaded.set(true);
    } catch (e) {
      this.error.set(apiErrorMessage(e, 'Impossible de charger les commentaires.'));
      this.loaded.set(true);
    }
  }

  private async act(action: string, call: () => Promise<void>): Promise<void> {
    this.error.set('');
    this.busy.set(action);
    try {
      await call();
    } catch (e) {
      this.error.set(apiErrorMessage(e, "L'action a échoué."));
    } finally {
      this.busy.set(null);
    }
  }
}
