import { Component, OnInit, input, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { EMPTY_FIXTURE_FORM, FixtureForm, Team, VENUE_LABELS, Venue } from '../../core/interclubs';

/** Form of a fixture (date, time, opponent, home or away, place), used to add one and to change one. */
@Component({
  selector: 'app-fixture-form',
  imports: [FormsModule],
  template: `
    <form class="stack stack-s" style="max-width: 48rem" (ngSubmit)="submit()">
      @if (teams().length > 0) {
        <div class="field">
          <label class="label" for="fx-team">Équipe</label>
          <select class="select" id="fx-team" name="team" [ngModel]="teamId()" (ngModelChange)="teamId.set($event)" required>
            @for (t of teams(); track t.id) { <option [value]="t.id">{{ t.name }}</option> }
          </select>
        </div>
      }
      <div class="cols cols-2">
        <div class="field"><label class="label" for="fx-date">Date</label><input class="input" id="fx-date" name="date" type="date" [(ngModel)]="form.date" required /></div>
        <div class="field"><label class="label" for="fx-time">Heure (facultatif)</label><input class="input" id="fx-time" name="time" type="time" [(ngModel)]="form.startTime" /></div>
      </div>
      <div class="field"><label class="label" for="fx-opponent">Club adverse</label><input class="input" id="fx-opponent" name="opponent" [(ngModel)]="form.opponent" maxlength="100" required autocomplete="off" /></div>
      <div class="field">
        <label class="label" for="fx-venue">Lieu de la rencontre</label>
        <select class="select" id="fx-venue" name="venue" [(ngModel)]="form.venue">
          @for (v of venues; track v[0]) { <option [value]="v[0]">{{ v[1] }}</option> }
        </select>
      </div>
      <div class="field"><label class="label" for="fx-place">Gymnase ou adresse (facultatif)</label><input class="input" id="fx-place" name="place" [(ngModel)]="form.place" maxlength="200" autocomplete="off" /></div>
      <p class="form-actions">
        <button class="btn" type="submit" [class.is-busy]="busy()" [disabled]="busy() || !canSubmit()">{{ submitLabel() }}</button>
        <button class="btn btn-ghost" type="button" [disabled]="busy()" (click)="cancelled.emit()">Annuler</button>
      </p>
    </form>
  `,
})
export class FixtureFormComponent implements OnInit {
  /** The teams to choose from (when adding a fixture); empty when the team is already known (changing one). */
  readonly teams = input<Team[]>([]);
  readonly initial = input<FixtureForm>(EMPTY_FIXTURE_FORM);
  readonly submitLabel = input('Enregistrer');
  readonly busy = input(false);
  readonly saved = output<{ teamId: string; form: FixtureForm }>();
  readonly cancelled = output<void>();

  protected readonly venues = Object.entries(VENUE_LABELS) as [Venue, string][];
  protected readonly teamId = signal('');
  protected form: FixtureForm = { ...EMPTY_FIXTURE_FORM };

  ngOnInit(): void {
    this.form = { ...this.initial() };
    this.teamId.set(this.teams()[0]?.id ?? '');
  }

  protected canSubmit(): boolean {
    return !!this.form.date && !!this.form.opponent.trim() && (this.teams().length === 0 || !!this.teamId());
  }

  protected submit(): void {
    if (this.canSubmit()) {
      this.saved.emit({ teamId: this.teamId(), form: { ...this.form } });
    }
  }
}
