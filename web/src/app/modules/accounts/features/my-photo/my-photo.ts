import { Component, inject, input, output, signal } from '@angular/core';
import { apiErrorMessage } from '../../../../shared/core/format';
import { Avatar } from '../../../../shared/ui/avatar';
import { AccountsApi } from '../../infrastructure/accounts.api';
import { PhotoCropper } from './photo-cropper';

/** Largest file the person may choose before cropping (the picture sent is a small square, far lighter). */
const MAX_FILE_BYTES = 15 * 1024 * 1024;

/** « Ma photo » on the profile page: choose, centre, replace or remove the profile picture. */
@Component({
  selector: 'app-my-photo',
  imports: [Avatar, PhotoCropper],
  template: `
    <section class="card stack" aria-labelledby="t-photo">
      <h2 id="t-photo" class="card-title">Ma photo</h2>
      @if (message()) {
        <p class="alert alert-success" role="status"><svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M5 12l5 5 9-10"></path></svg><span>{{ message() }}</span></p>
      }
      @if (error()) {
        <p class="alert alert-error" role="alert"><svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg><span><span class="alert-title">Erreur.</span> {{ error() }}</span></p>
      }
      @if (chosen(); as file) {
        <app-photo-cropper [file]="file" [busy]="saving()" (confirmed)="save($event)" (cancelled)="chosen.set(null)" (unreadable)="unreadablePhoto()" />
      } @else {
        <div class="row current">
          <app-avatar [accountId]="accountId()" [name]="name()" [version]="version()" size="l" />
          <div class="stack stack-s">
            <p class="form-actions">
              <button type="button" class="btn" (click)="picker.click()">{{ version() ? 'Changer ma photo' : 'Ajouter ma photo' }}</button>
              <input #picker class="sr-only" type="file" accept="image/jpeg,image/png,image/webp" tabindex="-1" aria-label="Choisir une photo" (change)="choose($event)" />
              @if (version()) {
                <button type="button" class="btn btn-danger" [class.is-busy]="removing()" [disabled]="removing()" (click)="remove()">Supprimer ma photo</button>
              }
            </p>
            <p class="small-text">
              Votre photo s'affiche à côté de vos commentaires et dans la liste des adhérents. Elle figure aussi au trombinoscope du club,
              visible des adhérents uniquement : sans photo, vous n'y apparaissez pas. Vous pouvez la changer ou la supprimer à tout moment.
            </p>
          </div>
        </div>
      }
    </section>
  `,
  styles: `
    .current { align-items: center; gap: var(--space-20); flex-wrap: wrap; }
  `,
})
export class MyPhoto {
  readonly accountId = input.required<string>();
  readonly name = input('');
  /** Version of the current picture, or null. */
  readonly version = input<number | null>(null);
  /** The picture changed: the profile is read again by the page. */
  readonly changed = output<void>();

  private readonly api = inject(AccountsApi);
  protected readonly chosen = signal<File | null>(null);
  protected readonly saving = signal(false);
  protected readonly removing = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');

  protected choose(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;
    input.value = '';
    this.message.set('');
    this.error.set('');
    if (!file) {
      return;
    }
    if (!/^image\/(jpeg|png|webp)$/.test(file.type)) {
      this.error.set('Choisissez une image au format JPEG, PNG ou WebP.');
    } else if (file.size > MAX_FILE_BYTES) {
      this.error.set('Cette image est trop lourde (15 Mo au plus).');
    } else {
      this.chosen.set(file);
    }
  }

  protected unreadablePhoto(): void {
    this.chosen.set(null);
    this.error.set("Cette image n'a pas pu être lue. Essayez-en une autre.");
  }

  protected async save(blob: Blob): Promise<void> {
    this.saving.set(true);
    this.error.set('');
    try {
      await this.api.setMyPhoto(blob);
      this.chosen.set(null);
      this.message.set('Photo enregistrée.');
      this.changed.emit();
    } catch (e) {
      this.error.set(apiErrorMessage(e, "L'enregistrement a échoué. Réessayez avec une autre photo."));
    } finally {
      this.saving.set(false);
    }
  }

  protected async remove(): Promise<void> {
    this.removing.set(true);
    this.message.set('');
    this.error.set('');
    try {
      await this.api.removeMyPhoto();
      this.message.set('Photo supprimée.');
      this.changed.emit();
    } catch (e) {
      this.error.set(apiErrorMessage(e, 'La suppression a échoué. Réessayez plus tard.'));
    } finally {
      this.removing.set(false);
    }
  }
}
