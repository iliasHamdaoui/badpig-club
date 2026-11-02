import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LatestAnnouncements } from '../modules/announcements';
import { NextFixtures } from '../modules/interclubs';
import { AuthService } from '../shared/infrastructure/auth.service';
import { ClubSessionsBlock } from '../shared/ui/club-sessions';

/** Where the club plays: used by the address block and by the link to the map. */
const MAP_LINK = 'https://www.openstreetmap.org/search?query=Rue%20du%20Coll%C3%A8ge%2034570%20Pignan';

/**
 * Home page, in its two states: the shop window of the club for a visitor, and the dashboard of a member
 * (announcements, next fixtures, what is waiting for an answer, shortcuts).
 */
@Component({
  selector: 'app-home',
  imports: [RouterLink, LatestAnnouncements, NextFixtures, ClubSessionsBlock],
  host: { class: 'bleed' },
  template: `
    @if (isMember()) {
      <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
        <header class="stack stack-s">
          <h1 style="font-size: var(--text-h2)">Bonjour {{ firstName() }}</h1>
          @if (auth.membershipUpToDate() === false) {
            <p class="row" style="--row-gap: var(--space-10)">
              <span class="highlight highlight-warning">
                <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 3l9 17H3zM12 10v4M12 17h.01"></path></svg>
                Cotisation à valider
              </span>
              <a routerLink="/profile">Voir mon profil</a>
            </p>
          }
        </header>

        <div class="cols cols-main cols-gap-l" style="align-items: start">
          <section class="stack" aria-labelledby="t-news">
            <div class="block-head"><h2 id="t-news">Annonces</h2><a class="link-action" routerLink="/announcements">Toutes les annonces</a></div>
            <app-latest-announcements [limit]="3" layout="rows" />
          </section>

          <div class="stack stack-l">
            <section class="stack" aria-labelledby="t-seances-member">
              <h2 id="t-seances-member">Les séances</h2>
              <app-club-sessions [count]="2" [showWeekly]="false" />
            </section>

            <section class="stack" aria-labelledby="t-ic">
              <div class="block-head"><h2 id="t-ic">Prochaines rencontres</h2><a class="link-action" routerLink="/interclubs">Calendrier</a></div>
              <app-next-fixtures [limit]="3" />
            </section>
          </div>
        </div>

        <nav class="stack" aria-labelledby="t-shortcuts">
          <h2 id="t-shortcuts" class="sr-only">Raccourcis</h2>
          <div class="grid-auto" style="--col-min: 24rem">
            <a class="tile" routerLink="/interclubs">
              <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="5" width="18" height="16" rx="2"></rect><path d="M3 10h18M8 3v4M16 3v4"></path></svg>
              <span>Interclubs<small>Calendrier, équipes, résultats</small></span>
            </a>
            <a class="tile" routerLink="/shop">
              <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M4 7h16l-1.5 12h-13zM9 7a3 3 0 0 1 6 0"></path></svg>
              <span>Boutique<small>Articles du club</small></span>
            </a>
            <a class="tile" routerLink="/profile">
              <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="8" r="4"></circle><path d="M4 21a8 8 0 0 1 16 0"></path></svg>
              <span>Mon profil<small>Coordonnées, adhésion</small></span>
            </a>
          </div>
        </nav>
      </main>
    } @else {
      <main>
        <section class="hero" aria-labelledby="t-hero">
          <span class="hero-court" aria-hidden="true"></span>
          <div class="container hero-grid">
            <div class="hero-in">
              <p class="eyebrow" style="color: var(--hero-text)">Badminton Pignan</p>
              <h1 id="t-hero">BadPig, le badminton entre voisins</h1>
              <p class="lead">Un club amateur et familial : trois soirs par semaine, on joue, on rit et on progresse à son rythme.</p>
              <p>Débutants et joueurs expérimentés partagent les mêmes séances, et le club engage aussi des équipes en interclubs.</p>
              <div class="row" style="--row-gap: var(--space-20)">
                <button class="btn btn-inverse" [class.is-busy]="auth.busy()" [disabled]="auth.busy()" (click)="auth.signIn('/')">Je suis adhérent</button>
              </div>
            </div>
            <div class="hero-logo"><img src="assets/logo.png" alt="BadPig, Badminton Pignan : un cochon aux quatre raquettes" /></div>
          </div>
        </section>

        <section class="section">
          <div class="container cols cols-main cols-gap-l" style="align-items: start">
            <div class="stack" aria-labelledby="t-news-public">
              <h2 id="t-news-public">Les annonces</h2>
              <app-latest-announcements [limit]="3" layout="rows" />
              <p><a class="link-action" routerLink="/announcements">
                Voir plus d'annonces
                <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><path d="M9 6l6 6-6 6"></path></svg>
              </a></p>
            </div>

            <div class="stack stack-l">
              <section class="stack" aria-labelledby="t-seances">
                <h2 id="t-seances">Les séances</h2>
                <app-club-sessions />
              </section>

              <section class="stack" aria-labelledby="t-place">
                <h2 id="t-place">Où nous jouons</h2>
                <address style="font-style: normal">
                  <strong>Collège Marie Curie — Gymnase</strong><br />
                  Halle aux sports du Collège<br />
                  Rue du Collège<br />
                  34570 Pignan
                </address>
                <p>
                  <a class="link-action" [href]="mapLink" target="_blank" rel="noopener">
                    Ouvrir le plan dans OpenStreetMap
                    <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><path d="M7 17L17 7M8 7h9v9"></path></svg>
                  </a>
                </p>
              </section>

              <section class="stack" aria-labelledby="t-contact">
                <h2 id="t-contact">Contact</h2>
                <div class="card stack stack-s">
                  <p><strong>Le bureau</strong><br /><span class="small-text">BadPig – Badminton Pignan</span></p>
                  <p>
                    <a class="link-action" href="mailto:contact&#64;badpig.fr">
                      <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="5" width="18" height="14" rx="2"></rect><path d="M3 7l9 6 9-6"></path></svg>
                      contact&#64;badpig.fr
                    </a>
                  </p>
                  <p class="row">
                    <a class="btn btn-ghost btn-sm" href="https://www.facebook.com/BadPig34/" target="_blank" rel="noopener">
                      <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><path d="M14 9h3V5.5h-2.5A3.5 3.5 0 0 0 11 9v2H8.5v3.5H11V21h3.5v-6.5H17L17.5 11H14V9.5a.5.5 0 0 1 .5-.5z"></path></svg>
                      Facebook
                    </a>
                    <a class="btn btn-ghost btn-sm" href="https://www.instagram.com/badminton_pignan/" target="_blank" rel="noopener">
                      <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><rect x="3.5" y="3.5" width="17" height="17" rx="5"></rect><circle cx="12" cy="12" r="4"></circle><path d="M17 7h.01"></path></svg>
                      Instagram
                    </a>
                  </p>
                </div>
              </section>
            </div>
          </div>
        </section>

        <section class="section-tight" style="background: var(--color-surface-inverse); color: var(--color-text-inverse)" aria-labelledby="t-cta">
          <div class="container row row-between" style="--row-gap: var(--space-20)">
            <div class="stack stack-s" style="max-width: 56rem">
              <h2 id="t-cta" style="font-size: var(--text-h3)">Vous êtes déjà adhérent ?</h2>
              <p>Connectez-vous pour retrouver les annonces réservées, les interclubs et la boutique.</p>
            </div>
            <button class="btn btn-inverse" [class.is-busy]="auth.busy()" [disabled]="auth.busy()" (click)="auth.signIn('/')">Je suis adhérent</button>
          </div>
        </section>
      </main>
    }
  `,
})
export class HomePage {
  protected readonly auth = inject(AuthService);
  protected readonly mapLink = MAP_LINK;

  /** The dashboard is for the members whose membership is up to date; the others see the public page. */
  protected isMember(): boolean {
    return this.auth.isMember() && this.auth.membershipUpToDate() !== false;
  }

  protected firstName(): string {
    return this.auth.displayName().split(' ')[0];
  }
}
