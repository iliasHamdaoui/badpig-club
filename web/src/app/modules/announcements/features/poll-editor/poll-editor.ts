import { Component, computed, input, model } from '@angular/core';
import { FormsModule } from '@angular/forms';

/**
 * Fields of a poll (board): the choices, single or multiple answer, anonymous or not, and the closing date.
 * Used when a poll is created; once members have answered the choices are locked (only the closing date can change).
 */
@Component({
  selector: 'app-poll-editor',
  imports: [FormsModule],
  template: `
    <section class="editor">
      @if (lockedReason()) { <p class="small-text">{{ lockedReason() }}</p> }

      <label>Choix (2 à 10)</label>
      @for (option of options(); track $index) {
        <div class="option">
          <input [name]="'option-' + $index" [ngModel]="option" (ngModelChange)="setOption($index, $event)" maxlength="100" [disabled]="locked()" [attr.aria-label]="'Choix ' + ($index + 1)" autocomplete="off" />
          <button type="button" class="btn btn-ghost btn-sm" [disabled]="locked() || options().length <= 2" (click)="removeOption($index)" [attr.aria-label]="'Retirer le choix ' + ($index + 1)">✕</button>
        </div>
      }
      <p><button type="button" class="btn btn-ghost btn-sm" [disabled]="locked() || options().length >= 10" (click)="addOption()">+ Ajouter un choix</button></p>

      <div class="row">
        <div>
          <label for="poll-mode">Réponses</label>
          <select id="poll-mode" name="pollMode" [ngModel]="allowMultiple()" (ngModelChange)="allowMultiple.set($event)" [disabled]="locked()">
            <option [ngValue]="false">Une seule réponse</option>
            <option [ngValue]="true">Plusieurs réponses possibles</option>
          </select>
        </div>
        <div>
          <label for="poll-anonymous">Participants</label>
          <select id="poll-anonymous" name="pollAnonymous" [ngModel]="isAnonymous()" (ngModelChange)="isAnonymous.set($event)" [disabled]="locked()">
            <option [ngValue]="false">Non anonyme (le bureau voit qui a répondu quoi, à la clôture)</option>
            <option [ngValue]="true">Anonyme (personne ne voit qui a répondu quoi)</option>
          </select>
        </div>
        <div>
          <label for="poll-closes">Date limite (incluse)</label>
          <input id="poll-closes" name="pollCloses" type="date" [ngModel]="closesOn()" (ngModelChange)="closesOn.set($event)" [min]="today" [disabled]="closed()" />
        </div>
      </div>
      <p class="small-text">Seuls les adhérents répondent. Ils peuvent changer leur réponse jusqu'à la date limite ; le lendemain, plus rien ne change et les résultats s'affichent.</p>
    </section>
  `,
  styles: `
    .editor { margin-top: 1.6rem; }
    .option { display: flex; gap: 0.8rem; margin: 0.48rem 0; }
    .row { display: grid; grid-template-columns: repeat(auto-fit, minmax(22.4rem, 1fr)); gap: 1.6rem; }
  `,
})
export class PollEditor {
  readonly options = model<string[]>(['', '']);
  readonly allowMultiple = model(false);
  readonly isAnonymous = model(false);
  readonly closesOn = model('');
  /** Members have already answered: the choices, the mode and the anonymity cannot change any more. */
  readonly hasAnswers = input(false);
  /** The poll is closed: nothing can change. */
  readonly closed = input(false);

  protected readonly today = new Date().toISOString().slice(0, 10);
  protected readonly locked = computed(() => this.hasAnswers() || this.closed());
  protected readonly lockedReason = computed(() => {
    if (this.closed()) {
      return 'Le sondage est clos : plus rien ne peut changer.';
    }
    return this.hasAnswers() ? 'Des adhérents ont déjà répondu : les choix ne peuvent plus être modifiés, seule la date limite peut changer.' : '';
  });

  protected setOption(index: number, value: string): void {
    this.options.update((options) => options.map((o, i) => (i === index ? value : o)));
  }

  protected addOption(): void {
    this.options.update((options) => [...options, '']);
  }

  protected removeOption(index: number): void {
    this.options.update((options) => options.filter((_, i) => i !== index));
  }
}
