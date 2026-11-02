import { NgTemplateOutlet } from '@angular/common';
import { Component, computed, effect, inject, input, output, signal } from '@angular/core';
import { apiErrorMessage, formatDate } from '../../../../shared/core/format';
import { AuthService } from '../../../../shared/infrastructure/auth.service';
import { Poll } from '../../core/announcement';
import { AnnouncementsApi } from '../../infrastructure/announcements.api';

/** The poll of an announcement: answer form for club members while it is open, results once it is closed. */
@Component({
  selector: 'app-announcement-poll',
  imports: [NgTemplateOutlet],
  template: `
    <section class="poll" aria-label="Sondage">
      <h2>📊 Sondage</h2>
      <p class="meta">
        {{ poll().isAnonymous ? 'Anonyme' : 'Non anonyme' }} · {{ poll().allowMultiple ? 'plusieurs réponses possibles' : 'une seule réponse' }} ·
        @if (poll().isClosed) { clos depuis le {{ date(poll().closesOn) }} } @else { ouvert jusqu'au {{ date(poll().closesOn) }} inclus }
      </p>

      @if (poll().isClosed) {
        <p>Le sondage est terminé{{ poll().participantCount === null ? '.' : ' : ' + count(poll().participantCount!) + '.' }}</p>
        @if (poll().participantCount === null) {
          <ul class="choices">@for (o of poll().options; track o.id) { <li>{{ o.label }}</li> }</ul>
          <p class="small-text">Les résultats sont réservés aux adhérents.</p>
        } @else {
          <ng-container [ngTemplateOutlet]="results" />
        }
      } @else if (poll().participantCount === null) {
        <ul class="choices">@for (o of poll().options; track o.id) { <li>{{ o.label }}</li> }</ul>
        <p class="small-text">Seuls les adhérents du club peuvent répondre.</p>
        @if (!auth.signedIn()) {
          <button class="btn btn-ghost" [class.is-busy]="auth.busy()" [disabled]="auth.busy()" (click)="auth.signIn(location.pathname)">Se connecter pour répondre</button>
        }
      } @else {
        <form (submit)="$event.preventDefault(); save()">
          @for (o of poll().options; track o.id) {
            <label class="choice">
              @if (poll().allowMultiple) {
                <input type="checkbox" [checked]="isSelected(o.id)" [disabled]="!poll().canVote || busy()" (change)="toggle(o.id)" />
              } @else {
                <input type="radio" name="poll-choice" [checked]="isSelected(o.id)" [disabled]="!poll().canVote || busy()" (change)="select(o.id)" />
              }
              {{ o.label }}
            </label>
          }
          @if (poll().canVote) {
            <p class="actions">
              <button type="submit" [class.is-busy]="busy()" [disabled]="busy() || selected().length === 0 || !hasChanged()">
                {{ poll().myOptionIds.length > 0 ? 'Modifier ma réponse' : 'Envoyer ma réponse' }}
              </button>
              @if (poll().myOptionIds.length > 0) {
                <button type="button" class="btn btn-ghost" [disabled]="busy()" (click)="withdraw()">Retirer ma réponse</button>
              }
            </p>
            <p class="small-text">
              {{ poll().myOptionIds.length > 0 ? 'Votre réponse est enregistrée. ' : '' }}Vous pouvez la modifier jusqu'au {{ date(poll().closesOn) }} inclus ;
              les résultats seront affichés ensuite. {{ count(poll().participantCount!) }} pour le moment.
            </p>
          } @else {
            <p class="small-text">Le sondage s'ouvrira aux réponses une fois l'annonce publiée.</p>
          }
        </form>
        @if (poll().resultsVisible) {
          <h3>Résultats provisoires</h3>
          <p class="small-text">Visibles du bureau seulement tant que le sondage est ouvert : les réponses peuvent encore changer jusqu'au {{ date(poll().closesOn) }} inclus.</p>
          <ng-container [ngTemplateOutlet]="results" />
        }
      }
      @if (message()) { <p class="success">{{ message() }}</p> }
      @if (error()) { <p class="error">{{ error() }}</p> }
    </section>

    <ng-template #results>
      @for (o of poll().options; track o.id) {
        <div class="result">
          <div class="line">
            <span>{{ o.label }} @if (isMine(o.id)) { <b class="mine">✓ votre choix</b> }</span>
            <span>{{ o.voteCount ?? 0 }} · {{ percent(o.voteCount ?? 0) }} %</span>
          </div>
          <div class="bar" role="img" [attr.aria-label]="o.label + ' : ' + percent(o.voteCount ?? 0) + ' %'"><span [style.width.%]="percent(o.voteCount ?? 0)"></span></div>
          @if (o.voters && o.voters.length > 0) { <p class="voters">{{ o.voters.join(', ') }}</p> }
        </div>
      }
    </ng-template>
  `,
  styles: `
    .poll { margin: 2.4rem 0; padding: 1.6rem 2rem; background: #faf6f5; border-radius: 12px; border: 0.1rem solid #eadcda; }
    .poll h3 { margin: 1.6rem 0 0.32rem; font-size: 1.6rem; }
    .poll h2 { margin: 0 0 0.4rem; font-size: var(--text-h5); }
    .meta { color: var(--color-text-muted); font-size: var(--text-small); }
    .choices { padding-left: 1.92rem; }
    .choice { display: flex; align-items: center; gap: 0.96rem; margin: 0.56rem 0; font-weight: 400; cursor: pointer; }
    .choice input { width: auto; }
    .result { margin: 0.96rem 0; }
    .line { display: flex; justify-content: space-between; gap: 1.6rem; }
    .bar { height: 0.96rem; background: #eadcda; border-radius: var(--radius-pill); overflow: hidden; margin-top: 0.32rem; }
    .bar span { display: block; height: 100%; background: var(--color-brand); }
    .mine { color: var(--color-brand-strong); font-size: var(--text-small); }
    .voters { margin: 0.32rem 0 0; color: var(--color-text-muted); font-size: var(--text-small); }
  `,
})
export class AnnouncementPoll {
  readonly announcementId = input.required<string>();
  readonly poll = input.required<Poll>();
  /** The announcement must be reloaded (an answer was recorded or removed). */
  readonly changed = output<void>();

  protected readonly auth = inject(AuthService);
  private readonly api = inject(AnnouncementsApi);
  protected readonly location = location;
  protected readonly selected = signal<string[]>([]);
  protected readonly busy = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected readonly date = formatDate;
  protected readonly hasChanged = computed(() => {
    const mine = this.poll().myOptionIds;
    const now = this.selected();
    return now.length !== mine.length || now.some((id) => !mine.includes(id));
  });

  constructor() {
    // The choices shown follow the recorded answer (at loading and after every change).
    effect(() => this.selected.set([...this.poll().myOptionIds]));
  }

  protected isSelected(id: string): boolean {
    return this.selected().includes(id);
  }

  protected isMine(id: string): boolean {
    return this.poll().myOptionIds.includes(id);
  }

  protected select(id: string): void {
    this.selected.set([id]);
  }

  protected toggle(id: string): void {
    this.selected.update((ids) => (ids.includes(id) ? ids.filter((i) => i !== id) : [...ids, id]));
  }

  protected percent(votes: number): number {
    const total = this.poll().participantCount ?? 0;
    return total > 0 ? Math.round((100 * votes) / total) : 0;
  }

  protected count(n: number): string {
    return n === 0 ? 'aucune réponse' : n === 1 ? '1 réponse' : `${n} réponses`;
  }

  protected save(): Promise<void> {
    return this.act(() => this.api.vote(this.announcementId(), this.selected()), 'Votre réponse est enregistrée.');
  }

  protected withdraw(): Promise<void> {
    return this.act(() => this.api.withdrawVote(this.announcementId()), 'Votre réponse est retirée.');
  }

  private async act(call: () => Promise<unknown>, success: string): Promise<void> {
    this.message.set('');
    this.error.set('');
    this.busy.set(true);
    try {
      await call();
      this.message.set(success);
    } catch (e) {
      this.error.set(apiErrorMessage(e, "L'action a échoué. Rechargez la page et réessayez."));
    } finally {
      this.busy.set(false);
      this.changed.emit(); // reload even after a failure: the poll may have closed
    }
  }
}
