import { Component, inject, signal } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { apiErrorMessage, formatLongDate } from '../../../../shared/core/format';
import { ANSWER_LABELS, DateAnswer, Proposal, ProposedDate, VENUE_LABELS, timeLabel } from '../../core/interclubs';
import { InterclubsApi } from '../../infrastructure/interclubs.api';

/** One proposal of dates: each adherent answers for every date; the captain (or the board) sees who answered and chooses the date. */
@Component({
  selector: 'app-proposal-detail',
  imports: [RouterLink],
  template: `
    <section class="card">
      <h1>Dates proposées</h1>
      <p><a routerLink="/interclubs/proposals">← Dates proposées</a> · <a routerLink="/interclubs">Calendrier</a></p>

      @if (failed()) {
        <p class="error">Cette proposition est introuvable.</p>
      } @else if (proposal(); as p) {
        @if (message()) { <p class="success">{{ message() }}</p> }
        @if (error()) { <p class="error">{{ error() }}</p> }

        <h2>{{ p.teamName }} contre {{ p.opponent }} <span class="small-text">({{ venues[p.venue] }}@if (p.place) { , {{ p.place }} })</span></h2>

        @if (p.isOpen) {
          @if (p.canAnswer) {
            <p>Dites pour chaque date si vous êtes disponible. Vous pouvez changer d'avis tant que le capitaine n'a pas choisi.</p>
          } @else {
            <p class="small-text">Seuls les joueurs de l'équipe peuvent répondre ; le capitaine les ajoute à l'équipe.</p>
          }
        } @else {
          <p class="success">Une date a été choisie.@if (p.fixtureId) { <a [routerLink]="['/interclubs/fixtures', p.fixtureId]"> Voir la rencontre</a> }</p>
        }

        @for (d of p.dates; track d.id) {
          <article class="date">
            <h3>{{ day(d.date) }}@if (d.startTime) { <span class="small-text">à {{ time(d.startTime) }}</span> }</h3>
            <p class="counts">{{ d.yes }} disponible{{ d.yes > 1 ? 's' : '' }} · {{ d.maybe }} peut-être · {{ d.no }} indisponible{{ d.no > 1 ? 's' : '' }}</p>

            @if (p.canAnswer) {
              <div class="actions" role="group" [attr.aria-label]="'Votre réponse pour le ' + day(d.date)">
                @for (a of answers; track a[0]) {
                  <button type="button" class="btn btn-sm" [class.btn-ghost]="d.myAnswer !== a[0]" [attr.aria-pressed]="d.myAnswer === a[0]"
                          [disabled]="busy() !== null" (click)="answer(p, d, a[0])">{{ a[1] }}</button>
                }
              </div>
            } @else if (d.myAnswer) {
              <p class="small-text">Votre réponse : {{ answerLabels[d.myAnswer] }}</p>
            }

            @if (p.canManage && d.answers?.length) {
              <ul class="names">
                @for (a of d.answers; track a.accountId) { <li><span class="dot" [class]="a.answer"></span>{{ a.name }} : {{ answerLabels[a.answer] }}</li> }
              </ul>
            }

            @if (p.canManage && p.isOpen) {
              @if (confirming() === d.id) {
                <p class="actions">
                  <span>Choisir cette date et créer la rencontre ?</span>
                  <button class="btn btn-sm" [class.is-busy]="busy() === 'choose'" [disabled]="busy() !== null" (click)="choose(p, d)">Oui, choisir</button>
                  <button class="btn btn-ghost btn-sm" (click)="confirming.set(null)">Non</button>
                </p>
              } @else {
                <p><button class="btn btn-ghost btn-sm" [disabled]="busy() !== null" (click)="confirming.set(d.id)">Choisir cette date</button></p>
              }
            }
          </article>
        }

        @if (p.canManage) {
          <p class="actions">
            @if (confirming() === 'delete') {
              <span>Supprimer cette proposition et les réponses ?</span>
              <button class="btn btn-danger btn-sm" [class.is-busy]="busy() === 'delete'" [disabled]="busy() !== null" (click)="remove(p)">Oui, supprimer</button>
              <button class="btn btn-ghost btn-sm" (click)="confirming.set(null)">Non</button>
            } @else {
              <button class="btn btn-ghost" [disabled]="busy() !== null" (click)="confirming.set('delete')">Supprimer la proposition</button>
            }
          </p>
        }
      } @else {
        <p>Chargement…</p>
      }
    </section>
  `,
  styles: `
    .date { border: 0.1rem solid var(--color-border); border-radius: var(--radius-m); padding: 0.4rem 1.6rem; margin: 1.6rem 0; }
    .counts { color: var(--color-text-muted); margin: 0 0 0.8rem; }
    .names { list-style: none; padding: 0; margin: 0.8rem 0; }
    .dot { display: inline-block; width: 1.12rem; height: 1.12rem; border-radius: 50%; margin-right: 0.8rem; background: var(--color-border-strong); }
    .dot.yes { background: var(--color-success-text); }
    .dot.maybe { background: var(--warning-base); }
    .dot.no { background: var(--color-error-text); }
  `,
})
export class ProposalDetailPage {
  private readonly api = inject(InterclubsApi);
  private readonly router = inject(Router);
  private readonly id = inject(ActivatedRoute).snapshot.paramMap.get('id') ?? '';
  protected readonly proposal = signal<Proposal | null>(null);
  protected readonly failed = signal(false);
  /** The action running ("choose", "delete", or the date being answered); the buttons are disabled meanwhile. */
  protected readonly busy = signal<string | null>(null);
  /** The date whose choice is awaiting confirmation, or "delete". */
  protected readonly confirming = signal<string | null>(null);
  protected readonly message = signal('');
  protected readonly error = signal('');
  protected readonly venues = VENUE_LABELS;
  protected readonly answerLabels = ANSWER_LABELS;
  protected readonly answers = Object.entries(ANSWER_LABELS) as [DateAnswer, string][];
  protected readonly day = formatLongDate;
  protected readonly time = timeLabel;

  constructor() {
    void this.load();
  }

  protected async answer(proposal: Proposal, date: ProposedDate, answer: DateAnswer): Promise<void> {
    await this.run(date.id, async () => {
      await this.api.answer(proposal.id, date.id, answer);
      await this.load();
    }, "Impossible d'enregistrer votre réponse.");
  }

  protected async choose(proposal: Proposal, date: ProposedDate): Promise<void> {
    await this.run('choose', async () => {
      const fixture = await this.api.choose(proposal.id, date.id);
      await this.router.navigate(['/interclubs/fixtures', fixture.id]);
    }, 'Impossible de choisir cette date.');
  }

  protected async remove(proposal: Proposal): Promise<void> {
    await this.run('delete', async () => {
      await this.api.deleteProposal(proposal.id);
      await this.router.navigateByUrl('/interclubs/proposals');
    }, 'Impossible de supprimer la proposition.');
  }

  private async run(key: string, action: () => Promise<void>, failure: string): Promise<void> {
    this.message.set('');
    this.error.set('');
    this.busy.set(key);
    try {
      await action();
    } catch (e) {
      this.error.set(apiErrorMessage(e, failure));
      await this.load();
    } finally {
      this.busy.set(null);
      this.confirming.set(null);
    }
  }

  private async load(): Promise<void> {
    try {
      this.proposal.set(await this.api.proposal(this.id));
    } catch {
      this.failed.set(true);
    }
  }
}
