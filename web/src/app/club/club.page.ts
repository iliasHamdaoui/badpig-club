import { HttpErrorResponse } from '@angular/common/http';
import { Component, effect, inject, signal, untracked } from '@angular/core';
import { RouterLink } from '@angular/router';
import { BoardApi, BoardMember } from '../modules/board';
import { Crumb, PageHead } from '../shared/ui/page-head';
import { ClubSessionsBlock } from '../shared/ui/club-sessions';
import { AuthService } from '../shared/infrastructure/auth.service';

/**
 * Public page presenting the club. The people of the board, with their title and e-mail, are read from the API
 * and only shown to the club members; a visitor sees the president, who is the public contact of the club.
 */
/** Where the club plays: used by the link to the map, like on the home page. */
const MAP_LINK = 'https://www.openstreetmap.org/search?query=Rue%20du%20Coll%C3%A8ge%2034570%20Pignan';

@Component({
  selector: 'app-club',
  imports: [RouterLink, PageHead, ClubSessionsBlock],
  host: { class: 'bleed' },
  template: `
    <main>
      <section style="padding-block: var(--space-30) 0">
        <div class="container">
          <app-page-head
            [trail]="trail"
            title="Le club"
            lead="Un club de village où l'on vient jouer au badminton pour le plaisir, trois soirs par semaine."
          />
        </div>
      </section>

      <section class="section" aria-labelledby="t-presentation">
        <div class="container cols cols-main cols-gap-l" style="align-items: start">
          <div class="stack">
            <h2 id="t-presentation">Qui nous sommes</h2>
            <p>
              BadPig est l'association de badminton de Pignan, dans l'Hérault. Le club accueille des adultes de tous niveaux :
              on y vient pour se dépenser après le travail, progresser à son rythme, et retrouver les autres autour du filet.
            </p>
            <p>
              Les séances sont libres : on joue en simple ou en double, selon qui est là. Pour celles et ceux qui aiment la
              compétition, le club engage plusieurs équipes en <a routerLink="/interclubs">interclubs</a>.
            </p>
            <p>
              L'association est gérée bénévolement par son conseil d'administration, élu par les adhérents en assemblée générale.
            </p>
          </div>
          <aside class="card stack stack-s">
            <h2 class="card-title">En pratique</h2>
            <app-club-sessions [count]="2" />
            <p class="small-text">
              Gymnase du Collège Marie Curie<br />
              Rue du Collège, 34570 Pignan
            </p>
            <p>
              <a class="link-action" [href]="mapLink" target="_blank" rel="noopener">
                Ouvrir le plan dans OpenStreetMap
                <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><path d="M7 17L17 7M8 7h9v9"></path></svg>
              </a>
            </p>
          </aside>
        </div>
      </section>

      <section class="section" style="padding-top: 0" aria-labelledby="t-board">
        <div class="container stack stack-l">
          <div class="stack stack-s">
            <h2 id="t-board">Le bureau</h2>
            <p class="lead">
              Une dizaine de bénévoles font tourner le club : ils valident les adhésions, encadrent les équipes d'interclubs,
              tiennent la boutique et organisent les moments conviviaux de la saison.
            </p>
            <p>
              Le <strong>conseil d'administration</strong>, élu en assemblée générale, représente l'association : un président,
              une secrétaire et une trésorière.
            </p>
          </div>

          @if (!auth.signedIn()) {
            <div class="card stack stack-s">
              <p class="row">
                <span class="avatar" aria-hidden="true">SF</span>
                <span class="highlight highlight-secondary">Président</span>
              </p>
              <h3 class="card-title">Savinien Ferrant</h3>
              <p class="small-text">Pour toute question sur le club ou une demande d'adhésion, écrivez-lui.</p>
              <p><a class="link-action" href="mailto:savinien.ferrant&#64;gmail.com">
                <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="5" width="18" height="14" rx="2"></rect><path d="M3 7l9 6 9-6"></path></svg>
                savinien.ferrant&#64;gmail.com
              </a></p>
            </div>
            <p class="alert alert-info">
              <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><rect x="5" y="11" width="14" height="9" rx="2"></rect><path d="M8 11V8a4 4 0 0 1 8 0v3"></path></svg>
              <span>Les coordonnées de tous les membres du bureau sont réservées aux adhérents. Connectez-vous pour les voir.</span>
            </p>
          } @else if (failed()) {
            <p class="alert alert-error">
              <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg>
              <span><span class="alert-title">Erreur.</span> {{ error() }}</span>
            </p>
          } @else if (!loaded()) {
            <div class="grid-auto" style="--col-min: 26rem" aria-hidden="true">
              <span class="skeleton" style="height: 14rem"></span>
              <span class="skeleton" style="height: 14rem"></span>
              <span class="skeleton" style="height: 14rem"></span>
            </div>
          } @else if (members().length === 0) {
            <p class="empty">
              <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="9" cy="8" r="3.5"></circle><path d="M2 20a7 7 0 0 1 14 0M17 5a3.5 3.5 0 0 1 0 7M22 20a7 7 0 0 0-4-6"></path></svg>
              <strong>Personne à afficher</strong>
              <span>Aucun compte actif du groupe Bureau n'a été trouvé.</span>
            </p>
          } @else {
            <ul class="grid-auto" style="--col-min: 26rem">
              @for (m of members(); track m.accountId) {
                <li>
                  <article class="card stack stack-s" style="height: 100%">
                    <p class="row">
                      <span class="avatar" aria-hidden="true">{{ initials(m.name) }}</span>
                      @if (m.title) {
                        <span class="highlight highlight-secondary">{{ m.title }}</span>
                      } @else {
                        <span class="highlight highlight-neutral">Membre du bureau</span>
                      }
                    </p>
                    <h3 class="card-title">{{ m.name }}</h3>
                    <p><a class="link-action" [href]="'mailto:' + m.email">
                      <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="5" width="18" height="14" rx="2"></rect><path d="M3 7l9 6 9-6"></path></svg>
                      Écrire
                    </a></p>
                  </article>
                </li>
              }
            </ul>
          }
        </div>
      </section>

    </main>
  `,
  styles: `
    ul { list-style: none; }
    .avatar {
      display: grid; place-items: center; width: 4.4rem; height: 4.4rem; flex: none; border-radius: 50%;
      background: var(--selected-background); color: var(--selected-text); font-weight: var(--weight-bold);
    }
  `,
})
export class ClubPage {
  protected readonly trail: Crumb[] = [];
  protected readonly mapLink = MAP_LINK;
  protected readonly auth = inject(AuthService);
  private readonly api = inject(BoardApi);
  protected readonly members = signal<BoardMember[]>([]);
  protected readonly loaded = signal(false);
  protected readonly failed = signal(false);
  protected readonly error = signal('');

  constructor() {
    // The session may be known after this page is built: the list follows it instead of being read once.
    effect(() => {
      if (this.auth.signedIn()) {
        untracked(() => void this.load());
      }
    });
  }

  private async load(): Promise<void> {
    try {
      this.members.set((await this.api.boardMembers()).items);
      this.failed.set(false);
    } catch (e) {
      this.error.set(
        e instanceof HttpErrorResponse && e.status === 403
          ? "La composition du bureau est réservée aux adhérents à jour de cotisation."
          : "La composition du bureau n'a pas pu être chargée.",
      );
      this.failed.set(true);
    } finally {
      this.loaded.set(true);
    }
  }

  protected initials(name: string): string {
    return name
      .split(' ')
      .filter((word) => word)
      .slice(0, 2)
      .map((word) => word[0].toUpperCase())
      .join('');
  }
}
