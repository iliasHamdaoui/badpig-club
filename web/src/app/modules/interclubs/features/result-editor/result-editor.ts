import { Component, OnInit, input, output } from '@angular/core';
import { FormsModule } from '@angular/forms';
import {
  ENDING_LABELS, FixtureMatch, MATCH_SLOTS, MAX_POINTS_IN_SET, MAX_SETS, MatchEnding, MatchEntry, MatchSide, OUTCOME_LABELS, POINTS_TO_WIN_SET, ResultForm, SIDE_LABELS, matchOutcome, parseSets, resultFormOf, resultIsComplete,
} from '../../core/interclubs';

/**
 * The captain enters how each of the seven matches ended. Played: the points of each set (15 points with a lead of 2; from 14-14, a lead of 2
 * or 21 points), 2 or 3 sets. Forfeit or retirement: who gave up (that side loses). Nobody types who won; the score of the fixture is the number of matches won by each side.
 */
@Component({
  selector: 'app-result-editor',
  imports: [FormsModule],
  template: `
    <form (ngSubmit)="save()">
      <p class="small-text">Saisissez les points de chaque set (nous : à gauche, adversaire : à droite). Un set se gagne à {{ pointsToWin }} points avec 2 points d'écart ; à partir de 14-14, il faut 2 points d'écart ou atteindre {{ maxPoints }}. Un match se joue en 2 ou 3 sets : le troisième set n'est saisi que si chacun en a gagné un. En cas de forfait (match non joué) ou d'abandon (match interrompu, saisissez les sets terminés), indiquez qui est concerné : il perd le match.</p>
      <p class="small-text">Vous pouvez enregistrer les matchs <strong>un par un</strong>, au fur et à mesure, avec le bouton de chaque ligne : le score de la rencontre apparaîtra quand les 7 matchs seront saisis.</p>
      <div class="scroll">
        <table>
          <thead><tr><th>Match</th><th>Déroulement</th>@for (n of setNumbers; track n) { <th>Set {{ n }}</th> }<th>Résultat</th><th></th></tr></thead>
          <tbody>
            @for (slot of slots; track slot.code) {
              <tr>
                <td>{{ slot.label }}</td>
                <td class="ending">
                  <select [attr.aria-label]="slot.label + ', déroulement'" [name]="slot.code + 'e'" [(ngModel)]="form[slot.code].ending">
                    @for (e of endings; track e) { <option [value]="e">{{ endingLabels[e] }}</option> }
                  </select>
                  @if (form[slot.code].ending !== 'played') {
                    <select [attr.aria-label]="slot.label + ', qui est concerné'" [name]="slot.code + 'w'" [(ngModel)]="form[slot.code].withdrawn">
                      @for (w of sides; track w) { <option [value]="w">{{ sideLabels[w] }}</option> }
                    </select>
                  }
                </td>
                @for (n of setNumbers; track n) {
                  <td class="set">
                    <input type="number" min="0" [max]="maxPoints" inputmode="numeric" [attr.aria-label]="slot.label + ', set ' + n + ', nous'" [name]="slot.code + 'c' + n" [(ngModel)]="form[slot.code].sets[n - 1].club" />
                    <span>–</span>
                    <input type="number" min="0" [max]="maxPoints" inputmode="numeric" [attr.aria-label]="slot.label + ', set ' + n + ', adversaire'" [name]="slot.code + 'o' + n" [(ngModel)]="form[slot.code].sets[n - 1].opponent" />
                  </td>
                }
                <td>
                  @if (state(slot.code); as s) {
                    @if (s === 'invalid') { <span class="error">Saisie incorrecte</span> }
                    @else { <span class="badge" [class.lost]="s === 'lost'">{{ outcomes[s] }}</span> }
                  } @else { <span class="small-text">—</span> }
                </td>
                <td>
                  <button type="button" class="btn btn-ghost btn-sm" [disabled]="busy() || state(slot.code) !== 'won' && state(slot.code) !== 'lost'"
                          (click)="saveOne(slot.code)">Enregistrer</button>
                </td>
              </tr>
            }
          </tbody>
        </table>
      </div>
      <p>Score de la rencontre : <strong>{{ wins().club }} – {{ wins().opponent }}</strong> @if (!isComplete()) { <span class="small-text">(les 7 matchs doivent être saisis)</span> }</p>
      <p class="actions">
        <button type="submit" [class.is-busy]="busy()" [disabled]="busy() || !isComplete()">Enregistrer les 7 matchs</button>
        <button type="button" class="btn btn-ghost" [disabled]="busy()" (click)="cancelled.emit()">Annuler</button>
      </p>
    </form>
  `,
  styles: `
    .scroll { overflow-x: auto; }
    .set, .ending { white-space: nowrap; }
    .ending select { margin-right: 0.48rem; }
    .set input { width: 5.6rem; padding: 0.48rem 0.64rem; text-align: center; }
  `,
})
export class ResultEditor implements OnInit {
  readonly matches = input.required<FixtureMatch[]>();
  readonly busy = input(false);
  readonly saved = output<ResultForm>();
  readonly savedMatch = output<{ slot: string; match: MatchEntry }>();
  readonly cancelled = output<void>();
  protected readonly slots = MATCH_SLOTS;
  protected readonly setNumbers = Array.from({ length: MAX_SETS }, (_, i) => i + 1);
  protected readonly pointsToWin = POINTS_TO_WIN_SET;
  protected readonly maxPoints = MAX_POINTS_IN_SET;
  protected readonly endings: MatchEnding[] = ['played', 'forfeit', 'retirement'];
  protected readonly sides: MatchSide[] = ['club', 'opponent'];
  protected readonly endingLabels = ENDING_LABELS;
  protected readonly sideLabels = SIDE_LABELS;
  protected readonly outcomes = OUTCOME_LABELS;
  protected form: ResultForm = {};

  ngOnInit(): void {
    this.form = resultFormOf(this.matches());
  }

  /** '' when nothing is typed yet; the outcome of the match when it is right; 'invalid' otherwise. */
  protected state(slot: string): 'won' | 'lost' | 'invalid' | '' {
    const match = this.form[slot];
    if (match.ending === 'played' && parseSets(match.sets)?.length === 0) {
      return '';
    }
    return matchOutcome(match) ?? 'invalid';
  }

  /** Matches won by each side, among the matches entered correctly. */
  protected wins(): { club: number; opponent: number } {
    const outcomes = MATCH_SLOTS.map((slot) => matchOutcome(this.form[slot.code]));
    return { club: outcomes.filter((o) => o === 'won').length, opponent: outcomes.filter((o) => o === 'lost').length };
  }

  protected isComplete(): boolean {
    return resultIsComplete(this.form);
  }

  /** Sends one match alone, as soon as it is entered correctly. */
  protected saveOne(slot: string): void {
    if (matchOutcome(this.form[slot]) !== null)
      this.savedMatch.emit({ slot, match: this.form[slot] });
  }

  protected save(): void {
    if (this.isComplete()) {
      this.saved.emit(this.form);
    }
  }
}
