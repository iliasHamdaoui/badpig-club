import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { apiErrorMessage, formatLongDate } from '../../../../shared/core/format';
import { ENDING_LABELS, Fixture, FixtureForm, LineupForm, MATCH_SLOTS, MatchEntry, OUTCOME_LABELS, ResultForm, SIDE_LABELS, TeamMember, VENUE_LABELS, formOf, setsText, timeLabel } from '../../core/interclubs';
import { InterclubsApi } from '../../infrastructure/interclubs.api';
import { FixtureFormComponent } from '../fixture-form/fixture-form';
import { LineupEditor } from '../lineup-editor/lineup-editor';
import { ResultEditor } from '../result-editor/result-editor';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

type Mode = 'fixture' | 'lineup' | 'result' | null;

/** One fixture: date, opponent and place, the lineup of the seven matches and the result; the captain of the team (or the board) changes them. */
@Component({
  selector: 'app-fixture-detail',
  imports: [FixtureFormComponent, LineupEditor, ResultEditor, PageHead],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Rencontre" />

      @if (failed()) {
        <p class="error">Cette rencontre est introuvable.</p>
      } @else if (fixture(); as f) {
        @if (message()) { <p class="success">{{ message() }}</p> }
        @if (error()) { <p class="error">{{ error() }}</p> }

        @if (mode() === 'fixture') {
          <app-fixture-form [initial]="form(f)" [busy]="busy()" (saved)="save($event.form)" (cancelled)="mode.set(null)" />
        } @else {
          <h2>{{ f.teamName }} contre {{ f.opponent }}</h2>
          @if (f.clubScore !== null) { <p class="score">Score : <strong>{{ f.clubScore }} – {{ f.opponentScore }}</strong></p> }
          <dl>
            <dt>Date</dt><dd>{{ day(f.date) }}@if (f.startTime) { à {{ time(f.startTime) }} }</dd>
            <dt>Lieu</dt><dd>{{ venues[f.venue] }}@if (f.place) { : {{ f.place }} }</dd>
            <dt>Capitaine</dt><dd>{{ f.captainName ?? 'Pas encore désigné' }}</dd>
          </dl>

          <h2>Les 7 matchs</h2>
          @if (mode() === 'lineup') {
            <app-lineup-editor [matches]="f.matches" [players]="players()" [busy]="busy()" (saved)="saveLineup($event)" (cancelled)="mode.set(null)" />
          } @else if (mode() === 'result') {
            <app-result-editor [matches]="f.matches" [busy]="busy()" (saved)="saveResult($event)" (savedMatch)="saveMatch($event)" (cancelled)="mode.set(null)" />
          } @else {
            <div class="scroll">
              <table>
                <thead><tr><th>Match</th><th>Joueurs</th><th>Résultat</th></tr></thead>
                <tbody>
                  @for (m of f.matches; track m.slot) {
                    <tr>
                      <td>{{ labelOf(m.slot) }}</td>
                      <td>@for (p of m.players; track p.accountId) { <div>{{ p.name }}</div> } @empty { <span class="small-text">à composer</span> }</td>
                      <td>
                        @if (m.outcome) { <span class="badge" [class.lost]="m.outcome === 'lost'">{{ outcomes[m.outcome] }}</span> <span class="small-text">@if (m.ending && m.ending !== 'played' && m.withdrawn) { {{ endingLabels[m.ending] }} ({{ sideLabels[m.withdrawn] }}) } {{ sets(m.sets) }}</span> }
                        @else { <span class="small-text">—</span> }
                      </td>
                    </tr>
                  }
                </tbody>
              </table>
            </div>
          }

          @if (f.canManage && mode() === null) {
            <p class="actions">
              <button class="btn" (click)="mode.set('lineup')">Composer l'équipe</button>
              <button class="btn" [disabled]="!isPlayed(f)" [attr.title]="isPlayed(f) ? null : 'Disponible à partir du jour de la rencontre'" (click)="mode.set('result')">Saisir le résultat</button>
              <button class="btn btn-ghost" (click)="mode.set('fixture')">Modifier la rencontre</button>
              @if (confirming()) {
                <span>Supprimer cette rencontre ?</span>
                <button class="btn btn-danger btn-sm" [class.is-busy]="busy()" [disabled]="busy()" (click)="remove()">Oui, supprimer</button>
                <button class="btn btn-ghost btn-sm" [disabled]="busy()" (click)="confirming.set(false)">Non</button>
              } @else {
                <button class="btn btn-danger" (click)="confirming.set(true)">Supprimer</button>
              }
            </p>
          }
        }
      } @else {
        <p>Chargement…</p>
      }
    </main>
  `,
  styles: `
    dl { display: grid; grid-template-columns: max-content 1fr; gap: 0.64rem 1.6rem; }
    dt { font-weight: 600; }
    dd { margin: 0; }
    .scroll { overflow-x: auto; }
    .score { font-size: var(--text-h5); }
  `,
})
export class FixtureDetailPage {
  protected readonly trail: Crumb[] = [{ label: 'Interclubs', path: '/interclubs' }, { label: 'Rencontre' }];
  private readonly api = inject(InterclubsApi);
  private readonly router = inject(Router);
  private readonly id = inject(ActivatedRoute).snapshot.paramMap.get('id') ?? '';
  protected readonly fixture = signal<Fixture | null>(null);
  protected readonly players = signal<TeamMember[]>([]);
  protected readonly failed = signal(false);
  protected readonly mode = signal<Mode>(null);
  protected readonly confirming = signal(false);
  protected readonly busy = signal(false);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected readonly venues = VENUE_LABELS;
  protected readonly outcomes = OUTCOME_LABELS;
  protected readonly endingLabels = ENDING_LABELS;
  protected readonly sideLabels = SIDE_LABELS;
  protected readonly day = formatLongDate;
  protected readonly time = timeLabel;
  protected readonly form = formOf;
  protected readonly sets = setsText;

  constructor() {
    void this.load();
  }

  protected labelOf(slot: string): string {
    return MATCH_SLOTS.find((s) => s.code === slot)?.label ?? slot;
  }

  /** The result can be entered from the day of the fixture (the API checks the date again). */
  protected isPlayed(fixture: Fixture): boolean {
    return fixture.date <= new Date().toLocaleDateString('sv-SE');
  }

  protected async save(form: FixtureForm): Promise<void> {
    await this.run(async () => {
      await this.api.updateFixture(this.id, form);
      this.finish('Rencontre modifiée.');
    }, 'Impossible de modifier la rencontre.');
  }

  protected async saveLineup(form: LineupForm): Promise<void> {
    await this.run(async () => {
      await this.api.setLineup(this.id, form);
      this.finish('Composition enregistrée.');
    }, "Impossible d'enregistrer la composition.");
  }

  protected async saveResult(form: ResultForm): Promise<void> {
    await this.run(async () => {
      await this.api.setResult(this.id, form);
      this.finish('Résultat enregistré.');
    }, "Impossible d'enregistrer le résultat.");
  }

  /** One match entered alone: the editor stays open so that the captain can carry on with the next match. */
  protected async saveMatch(entry: { slot: string; match: MatchEntry }): Promise<void> {
    await this.run(async () => {
      await this.api.setMatchResult(this.id, entry.slot, entry.match);
      this.message.set(`${this.labelOf(entry.slot)} enregistré.`);
      await this.load();
    }, "Impossible d'enregistrer ce match.");
  }

  protected async remove(): Promise<void> {
    await this.run(async () => {
      await this.api.deleteFixture(this.id);
      await this.router.navigateByUrl('/interclubs');
    }, 'Impossible de supprimer la rencontre.');
  }

  private finish(message: string): void {
    this.mode.set(null);
    this.message.set(message);
    void this.load();
  }

  private async run(action: () => Promise<void>, failure: string): Promise<void> {
    this.message.set('');
    this.error.set('');
    this.busy.set(true);
    try {
      await action();
    } catch (e) {
      this.error.set(apiErrorMessage(e, failure));
    } finally {
      this.busy.set(false);
    }
  }

  private async load(): Promise<void> {
    try {
      const fixture = await this.api.fixture(this.id);
      this.fixture.set(fixture);
      if (fixture.canManage) {
        this.players.set((await this.api.members(fixture.teamId, { sort: 'name', direction: 'asc', pageSize: 100 })).items);
      }
    } catch {
      this.failed.set(true);
    }
  }
}
