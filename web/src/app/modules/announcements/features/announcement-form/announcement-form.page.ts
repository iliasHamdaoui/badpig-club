import { Component, DestroyRef, computed, inject, signal } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { apiErrorMessage } from '../../../../shared/core/format';
import { Announcement, AnnouncementForm, AnnouncementInput, AnnouncementKind, PollAnnouncementInput } from '../../core/announcement';
import { formatSize } from '../../core/media-links';
import { AnnouncementsApi } from '../../infrastructure/announcements.api';
import { PollEditor } from '../poll-editor/poll-editor';
import { RichTextEditor } from '../rich-text-editor/rich-text-editor';

const MAX_POSTER_BYTES = 5 * 1024 * 1024;
const MAX_ATTACHMENT_BYTES = 10 * 1024 * 1024;

/** Creation and edition of an announcement (board). The poster and the attachments are added once the announcement exists. */
@Component({
  selector: 'app-announcement-form',
  imports: [FormsModule, RouterLink, RichTextEditor, PollEditor],
  template: `
    <section class="card">
      <h1>{{ id() ? (isPoll() ? 'Modifier le sondage' : "Modifier l'annonce") : (isPoll() ? 'Nouveau sondage' : 'Nouvelle annonce') }}</h1>
      <p><a routerLink="/admin/announcements">← Retour à la liste</a>@if (announcement(); as a) { · <a [routerLink]="['/announcements', a.id]">Voir l'annonce</a> }</p>

      @if (loading()) {
        <p>Chargement…</p>
      } @else if (loadFailed()) {
        <p class="error">Impossible de charger cette annonce.</p>
      } @else {
        @if (message()) { <p class="success">{{ message() }}</p> }
        @if (error()) { <p class="error">{{ error() }}</p> }

        <form (ngSubmit)="save()">
          <label for="title">{{ isPoll() ? 'Question' : 'Titre' }}</label>
          <input id="title" name="title" [(ngModel)]="title" required maxlength="150" autocomplete="off" />

          <div class="row">
            <div>
              <label for="kind">Visible par</label>
              <select id="kind" name="kind" [(ngModel)]="kind">
                <option value="internal">Les adhérents du club (interne)</option>
                <option value="public">Tout visiteur du site (publique)</option>
              </select>
            </div>
            <div>
              <label for="expiresOn">Dernier jour d'affichage</label>
              <input id="expiresOn" name="expiresOn" type="date" [(ngModel)]="expiresOn" [min]="today" />
              <span class="small-text">{{ id() ? 'Après cette date, l’annonce disparaît puis est supprimée.' : 'Vide = 90 jours à partir d’aujourd’hui.' }}</span>
            </div>
          </div>

          <p class="check">
            <label><input type="checkbox" name="commentsEnabled" [(ngModel)]="commentsEnabled" /> Autoriser les adhérents à commenter</label>
            <span class="small-text">Décoché : plus aucun commentaire ne peut être ajouté et ceux qui existent sont masqués du site.</span>
          </p>

          <label>{{ isPoll() ? 'Explication (facultative)' : 'Contenu' }}</label>
          <app-rich-text-editor [(html)]="bodyHtml" />
          <p class="small-text">Pour afficher une vidéo, collez son lien YouTube ou Vimeo seul sur une ligne. Un lien Instagram, Facebook ou autre se met avec le bouton « Lien ».</p>

          @if (isPoll()) {
            <h2>Sondage</h2>
            <app-poll-editor [(options)]="pollOptions" [(allowMultiple)]="allowMultiple" [(isAnonymous)]="isAnonymous" [(closesOn)]="closesOn"
                             [hasAnswers]="(announcement()?.poll?.participantCount ?? 0) > 0" [closed]="announcement()?.poll?.isClosed ?? false" />
          }

          <p class="actions">
            <button type="submit" [class.is-busy]="busy() === 'save'" [disabled]="busy() !== null || !canSave()">{{ id() ? 'Enregistrer' : 'Créer en brouillon' }}</button>
            @if (announcement(); as a) {
              <button type="button" class="btn btn-ghost" [class.is-busy]="busy() === 'publish'" [disabled]="busy() !== null" (click)="togglePublished(a)">
                {{ a.isPublished ? 'Dépublier' : 'Publier' }}
              </button>
            }
          </p>
        </form>

        @if (announcement(); as a) {
          <hr />
          <h2>Affiche</h2>
          @if (poster(); as p) {
            <img class="poster" [src]="api.fileUrl(a.id, p.id)" [alt]="'Affiche : ' + a.title" />
            <p class="actions">
              <button class="btn btn-ghost btn-sm" [disabled]="busy() !== null" (click)="removePoster(a)">Retirer l'affiche</button>
            </p>
          } @else {
            <p class="small-text">Aucune affiche.</p>
          }
          <label for="poster">{{ poster() ? "Remplacer l'affiche" : 'Ajouter une affiche' }} (JPEG, PNG, WebP ou GIF, 5 Mo au plus)</label>
          <input id="poster" type="file" accept="image/jpeg,image/png,image/webp,image/gif" [disabled]="busy() !== null" (change)="uploadPoster(a, $event)" />

          <h2>Pièces jointes</h2>
          @if (attachments().length === 0) {
            <p class="small-text">Aucune pièce jointe.</p>
          } @else {
            <ul class="files">
              @for (f of attachments(); track f.id) {
                <li>
                  📎 <a [href]="api.fileUrl(a.id, f.id)" download>{{ f.fileName }}</a> <span class="small-text">({{ size(f.sizeBytes) }})</span>
                  <button class="btn btn-ghost btn-sm" [disabled]="busy() !== null" (click)="removeAttachment(a, f.id)">Retirer</button>
                </li>
              }
            </ul>
          }
          <label for="attachment">Ajouter une pièce jointe (PDF, image, document Office ou OpenDocument, texte ; 10 Mo au plus, 10 pièces au plus)</label>
          <input id="attachment" type="file" [disabled]="busy() !== null || attachments().length >= 10" (change)="uploadAttachment(a, $event)" />
          @if (busy() === 'upload') { <p class="small-text">Envoi en cours…</p> }
        } @else {
          <p class="small-text">L'affiche et les pièces jointes s'ajoutent juste après la création.</p>
        }
      }
    </section>
  `,
  styles: `
    .check { margin: 1.6rem 0; }
    .check label { display: inline-flex; gap: 0.8rem; align-items: center; }
    .check input { width: auto; }
    .check .small-text { display: block; }
    .row { display: grid; grid-template-columns: repeat(auto-fit, minmax(22.4rem, 1fr)); gap: 1.6rem; align-items: start; }
    .poster { display: block; max-width: 25.6rem; max-height: 35.2rem; border-radius: var(--radius-m); }
    .files { list-style: none; padding: 0; }
    .files li { display: flex; flex-wrap: wrap; gap: 0.8rem; align-items: center; padding: 0.4rem 0; }
  `,
})
export class AnnouncementFormPage {
  protected readonly api = inject(AnnouncementsApi);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  /** Identifier of the announcement being edited; null when creating. */
  protected readonly id = signal<string | null>(null);
  protected readonly announcement = signal<Announcement | null>(null);
  protected readonly loading = signal(false);
  protected readonly loadFailed = signal(false);
  protected readonly busy = signal<string | null>(null);
  protected readonly message = signal('');
  protected readonly error = signal('');

  protected title = '';
  protected kind: AnnouncementKind = 'internal';
  protected expiresOn = '';
  protected commentsEnabled = true;
  protected readonly bodyHtml = signal('');
  protected readonly pollOptions = signal<string[]>(['', '']);
  protected readonly allowMultiple = signal(false);
  protected readonly isAnonymous = signal(false);
  protected readonly closesOn = signal('');
  /** A poll is an announcement of its own: this form creates and edits either a text or a poll. */
  protected readonly form = signal<AnnouncementForm>('text');
  protected readonly isPoll = computed(() => this.form() === 'poll');
  protected readonly today = new Date().toISOString().slice(0, 10);

  protected readonly poster = computed(() => this.announcement()?.files.find((f) => f.isPoster));
  protected readonly attachments = computed(() => this.announcement()?.files.filter((f) => !f.isPoster) ?? []);
  protected readonly size = formatSize;

  constructor() {
    this.route.paramMap.pipe(takeUntilDestroyed(inject(DestroyRef))).subscribe((params) => {
      const id = params.get('id');
      this.id.set(id);
      this.form.set(this.route.snapshot.data['form'] === 'poll' ? 'poll' : 'text');
      if (id) {
        void this.load(id);
      }
      if (history.state?.created) {
        this.message.set("Créée en brouillon. Ajoutez maintenant l'affiche et les pièces jointes si besoin, puis publiez-la.");
      }
    });
  }

  protected async save(): Promise<void> {
    this.error.set('');
    this.message.set('');
    this.busy.set('save');
    try {
      if (this.id()) {
        await this.changeDeadlineIfNeeded();
        await this.api.update(this.id()!, this.input());
        await this.load(this.id()!);
        this.message.set(this.isPoll() ? 'Sondage enregistré.' : 'Annonce enregistrée.');
      } else {
        const created = this.isPoll() ? await this.api.createPoll(this.pollInput()) : await this.api.create(this.input());
        await this.router.navigate(['/admin/announcements', created.id, 'edit'], { state: { created: true } });
      }
    } catch (e) {
      this.error.set(apiErrorMessage(e, "L'enregistrement a échoué."));
    } finally {
      this.busy.set(null);
    }
  }

  protected async togglePublished(a: Announcement): Promise<void> {
    await this.act('publish', () => (a.isPublished ? this.api.unpublish(a.id) : this.api.publish(a.id)), a.isPublished ? 'Annonce dépubliée.' : 'Annonce publiée.');
  }

  protected async uploadPoster(a: Announcement, event: Event): Promise<void> {
    const file = this.pickedFile(event);
    if (!file) {
      return;
    }
    if (file.size > MAX_POSTER_BYTES) {
      this.error.set(`L'affiche est trop lourde (${formatSize(file.size)} ; 5 Mo au plus).`);
      return;
    }
    await this.act('upload', () => this.api.setPoster(a.id, file), 'Affiche enregistrée.');
  }

  protected async removePoster(a: Announcement): Promise<void> {
    await this.act('poster', () => this.api.removePoster(a.id), 'Affiche retirée.');
  }

  protected async uploadAttachment(a: Announcement, event: Event): Promise<void> {
    const file = this.pickedFile(event);
    if (!file) {
      return;
    }
    if (file.size > MAX_ATTACHMENT_BYTES) {
      this.error.set(`La pièce jointe est trop lourde (${formatSize(file.size)} ; 10 Mo au plus).`);
      return;
    }
    await this.act('upload', () => this.api.addAttachment(a.id, file), `« ${file.name} » ajoutée.`);
  }

  protected async removeAttachment(a: Announcement, fileId: string): Promise<void> {
    await this.act('attachment', () => this.api.removeAttachment(a.id, fileId), 'Pièce jointe retirée.');
  }

  /** The file chosen in a file input; the input is emptied so that the same file can be chosen again. */
  private pickedFile(event: Event): File | null {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    input.value = '';
    return file;
  }

  private async act(action: string, call: () => Promise<unknown>, success: string): Promise<void> {
    this.error.set('');
    this.message.set('');
    this.busy.set(action);
    try {
      await call();
      await this.load(this.id()!);
      this.message.set(success);
    } catch (e) {
      this.error.set(apiErrorMessage(e, "L'action a échoué."));
    } finally {
      this.busy.set(null);
    }
  }

  /** Choices, mode and anonymity of a poll never change; the closing date can, while the poll is open. */
  private async changeDeadlineIfNeeded(): Promise<void> {
    const poll = this.announcement()?.poll;
    if (poll && !poll.isClosed && this.closesOn() !== '' && this.closesOn() !== poll.closesOn) {
      await this.api.changePollDeadline(this.id()!, this.closesOn());
      if (this.expiresOn !== '' && this.expiresOn < this.closesOn()) {
        this.expiresOn = this.closesOn();
      }
    }
  }

  /** A function, not a computed: the title is a plain field of the form. */
  protected canSave(): boolean {
    return this.title.trim() !== '' && (!this.isPoll() || this.isPollValid());
  }

  private isPollValid(): boolean {
    const labels = this.pollOptions().map((o) => o.trim().toLowerCase());
    return labels.length >= 2 && labels.every((l) => l.length > 0) && new Set(labels).size === labels.length && this.closesOn() !== '';
  }

  private pollInput(): PollAnnouncementInput {
    return {
      ...this.input(),
      options: this.pollOptions().map((o) => o.trim()),
      allowMultiple: this.allowMultiple(),
      isAnonymous: this.isAnonymous(),
      closesOn: this.closesOn(),
    };
  }

  private input(): AnnouncementInput {
    return {
      title: this.title.trim(),
      bodyHtml: this.bodyHtml(),
      kind: this.kind,
      commentsEnabled: this.commentsEnabled,
      ...(this.expiresOn ? { expiresOn: this.expiresOn } : {}),
    };
  }

  private async load(id: string): Promise<void> {
    this.loading.set(this.announcement() === null);
    try {
      const a = await this.api.get(id);
      this.announcement.set(a);
      this.title = a.title;
      this.kind = a.kind;
      this.expiresOn = a.expiresOn;
      this.commentsEnabled = a.commentsEnabled;
      this.bodyHtml.set(a.bodyHtml);
      this.form.set(a.form);
      if (a.poll) {
        this.pollOptions.set(a.poll.options.map((o) => o.label));
        this.allowMultiple.set(a.poll.allowMultiple);
        this.isAnonymous.set(a.poll.isAnonymous);
        this.closesOn.set(a.poll.closesOn);
      }
      this.loadFailed.set(false);
    } catch {
      this.loadFailed.set(true);
    } finally {
      this.loading.set(false);
    }
  }
}

