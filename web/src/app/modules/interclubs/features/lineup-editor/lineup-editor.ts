import { Component, OnInit, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LineupForm, MATCH_SLOTS, TeamMember, lineupFormOf, lineupIsValid, FixtureMatch } from '../../core/interclubs';

/** The captain chooses, among the players of the team, who plays each of the seven matches. */
@Component({
  selector: 'app-lineup-editor',
  imports: [FormsModule],
  template: `
    <form (ngSubmit)="save()">
      <table>
        <thead><tr><th>Match</th><th>Joueurs</th></tr></thead>
        <tbody>
          @for (slot of slots; track slot.code) {
            <tr>
              <td>{{ slot.label }}</td>
              <td class="players">
                @for (i of indexes(slot.players); track i) {
                  <select [attr.aria-label]="slot.label + ', joueur ' + (i + 1)" [name]="slot.code + i" [(ngModel)]="form[slot.code][i]">
                    <option value="">— personne —</option>
                    @for (p of players(); track p.accountId) { <option [value]="p.accountId">{{ p.name }}</option> }
                  </select>
                }
              </td>
            </tr>
          }
        </tbody>
      </table>
      @if (!isValid()) { <p class="error">Chaque match doit avoir tous ses joueurs (ou aucun), deux joueurs différents pour un double.</p> }
      @if (players().length === 0) { <p class="small-text">L'équipe n'a pas encore de joueurs : ajoutez-les dans l'effectif de l'équipe.</p> }
      <p class="actions">
        <button type="submit" [class.is-busy]="busy()" [disabled]="busy() || !isValid()">Enregistrer la composition</button>
        <button type="button" class="btn btn-ghost" [disabled]="busy()" (click)="cancelled.emit()">Annuler</button>
      </p>
    </form>
  `,
  styles: `
    .players { display: flex; flex-wrap: wrap; gap: 0.8rem; }
  `,
})
export class LineupEditor implements OnInit {
  readonly matches = input.required<FixtureMatch[]>();
  readonly players = input.required<TeamMember[]>();
  readonly busy = input(false);
  readonly saved = output<LineupForm>();
  readonly cancelled = output<void>();
  protected readonly slots = MATCH_SLOTS;
  protected form: LineupForm = {};

  ngOnInit(): void {
    this.form = lineupFormOf(this.matches());
  }

  protected indexes(count: number): number[] {
    return Array.from({ length: count }, (_, i) => i);
  }

  protected isValid(): boolean {
    return lineupIsValid(this.form);
  }

  protected save(): void {
    if (this.isValid()) {
      this.saved.emit(this.form);
    }
  }
}
