import { Component, effect, inject, signal, untracked } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ListState, Page } from '../../../../shared/core/page';
import { AdminActivity } from '../../../../shared/infrastructure/admin-activity';
import { AuthService } from '../../../../shared/infrastructure/auth.service';
import { Pager, SearchBox } from '../../../../shared/ui/list-controls';
import { Team, TeamCandidate, TeamMember } from '../../core/interclubs';
import { InterclubsApi } from '../../infrastructure/interclubs.api';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

const NO_PAGE = { items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 };

/** The players of a team. The captain (or the board) picks them among all the active members: only the players answer the proposals of dates. */
@Component({
  selector: 'app-team-members',
  imports: [SearchBox, Pager, PageHead],
  providers: [AdminActivity],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60); max-width: 84rem">
      <app-page-head [trail]="trail" [title]="title()" [lead]="lead()" />

      @if (activity.message()) {
        <p class="alert alert-success">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M5 12l5 5 9-10"></path></svg>
          <span>{{ activity.message() }}</span>
        </p>
      }
      @if (activity.error()) {
        <p class="alert alert-error">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg>
          <span><span class="alert-title">Erreur.</span> {{ activity.error() }}</span>
        </p>
      }

      <section class="stack" aria-labelledby="t-players">
        <h2 id="t-players">Joueurs</h2>
        <app-search-box label="Rechercher un joueur…" (searched)="members.setSearch($event)" />
        @if (!loaded()) {
          <div class="stack stack-s" aria-hidden="true"><span class="skeleton" style="height: 6rem"></span><span class="skeleton" style="height: 6rem"></span></div>
        } @else if (players().items.length === 0) {
          <p class="empty">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="9" cy="8" r="3.5"></circle><path d="M2 20a7 7 0 0 1 14 0"></path></svg>
            <strong>Aucun joueur</strong>
            <span>{{ members.search() ? 'Aucun joueur pour cette recherche.' : "L'effectif de l'équipe se compose ci-dessous." }}</span>
          </p>
        } @else {
          <ul class="people">
            @for (m of players().items; track m.accountId) {
              <li class="person">
                <span class="row" style="--row-gap: var(--space-10)">
                  <span class="name">{{ m.name }}</span>
                  @if (m.isCaptain) { <span class="highlight highlight-tertiary">Capitaine</span> }
                </span>
                @if (canManage()) {
                  <button class="btn btn-danger btn-sm" [class.is-busy]="activity.busy() === m.accountId" [disabled]="activity.busy() !== null" (click)="remove(m)">Retirer</button>
                }
              </li>
            }
          </ul>
          <app-pager [page]="players().page" [totalPages]="players().totalPages" [totalCount]="players().totalCount" (pageChange)="members.goTo($event)" />
        }
      </section>

      @if (canManage()) {
        <section class="stack" aria-labelledby="t-add">
          <h2 id="t-add">Ajouter des joueurs</h2>
          <p>Tous les adhérents actifs qui ne sont pas encore dans l'équipe.</p>
          <app-search-box label="Rechercher un adhérent…" (searched)="candidates.setSearch($event)" />
          @if (candidatesPage().items.length === 0) {
            <p class="empty">
              <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="11" cy="11" r="7"></circle><path d="M20 20l-4-4"></path></svg>
              <strong>{{ candidates.search() ? 'Aucun résultat' : 'Personne à ajouter' }}</strong>
              <span>{{ candidates.search() ? 'Essayez un autre nom.' : "Tous les adhérents sont déjà dans l'équipe." }}</span>
            </p>
          } @else {
            <ul class="people">
              @for (c of candidatesPage().items; track c.accountId) {
                <li class="person">
                  <span class="name">{{ c.name }}</span>
                  <button class="btn btn-sm" [class.is-busy]="activity.busy() === c.accountId" [disabled]="activity.busy() !== null" (click)="add(c)">Ajouter</button>
                </li>
              }
            </ul>
            <app-pager [page]="candidatesPage().page" [totalPages]="candidatesPage().totalPages" [totalCount]="candidatesPage().totalCount" (pageChange)="candidates.goTo($event)" />
          }
        </section>
      }
    </main>
  `,
  styles: `
    .people { list-style: none; display: grid; }
    .person { display: flex; flex-wrap: wrap; justify-content: space-between; align-items: center; gap: var(--space-15); padding: var(--space-15); background: var(--card-background); border: 0.1rem solid var(--card-border); border-radius: var(--radius-m); }
    .person + .person { margin-top: var(--space-10); }
    .name { font-weight: var(--weight-bold); }
  `,
})
export class TeamMembersPage {
  protected readonly trail: Crumb[] = [{ label: 'Interclubs', path: '/interclubs' }, { label: 'Équipes', path: '/interclubs/teams' }, { label: 'Effectif' }];
  protected readonly activity = inject(AdminActivity);
  private readonly api = inject(InterclubsApi);
  private readonly auth = inject(AuthService);
  protected readonly teamId = inject(ActivatedRoute).snapshot.paramMap.get('id') ?? '';
  protected readonly members = new ListState('name', 'asc', 25);
  protected readonly candidates = new ListState('name', 'asc', 10);
  protected readonly team = signal<Team | null>(null);
  protected readonly players = signal<Page<TeamMember>>(NO_PAGE);
  protected readonly candidatesPage = signal<Page<TeamCandidate>>(NO_PAGE);
  protected readonly loaded = signal(false);
  protected readonly canManage = signal(false);

  protected title(): string {
    const team = this.team();
    return team ? `Effectif de ${team.name}` : 'Effectif';
  }

  protected lead(): string {
    const team = this.team();
    return team ? `Capitaine : ${team.captainName ?? 'pas encore désigné'}.` : '';
  }

  constructor() {
    void this.loadTeam();
    effect(() => {
      const query = this.members.query();
      this.activity.changed();
      untracked(() => void this.loadMembers(query));
    });
    effect(() => {
      const query = this.candidates.query();
      this.activity.changed();
      if (this.canManage()) {
        untracked(() => void this.loadCandidates(query));
      }
    });
  }

  protected async add(person: TeamCandidate): Promise<void> {
    await this.activity.run(person.accountId, () => this.api.addMember(this.teamId, person.accountId), `${person.name} est dans l'équipe.`);
  }

  protected async remove(member: TeamMember): Promise<void> {
    await this.activity.run(member.accountId, () => this.api.removeMember(this.teamId, member.accountId), `${member.name} n'est plus dans l'équipe.`);
  }

  private async loadTeam(): Promise<void> {
    try {
      const team = (await this.api.teams({ pageSize: 100 })).items.find((t) => t.id === this.teamId) ?? null;
      this.team.set(team);
      // The board and the captain manage the players; the API checks it again.
      this.canManage.set(this.auth.isBoard() || (team?.isMyTeam ?? false));
    } catch {
      this.team.set(null);
    }
  }

  private async loadMembers(query: Parameters<InterclubsApi['members']>[1]): Promise<void> {
    try {
      this.players.set(await this.api.members(this.teamId, query));
      this.loaded.set(true);
    } catch {
      this.loaded.set(true);
    }
  }

  private async loadCandidates(query: Parameters<InterclubsApi['candidates']>[1]): Promise<void> {
    try {
      this.candidatesPage.set(await this.api.candidates(this.teamId, query));
    } catch {
      this.candidatesPage.set(NO_PAGE);
    }
  }
}
