import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ShopAlerts } from '../modules/shop';
import { UsersDirectory } from '../shared/infrastructure/users-directory';
import { Crumb, PageHead } from '../shared/ui/page-head';

/**
 * Home page of the board: the rubrics grouped by subject, instead of one long line of links.
 * The user list, which used to be here, now has its own page (/admin/users).
 */
@Component({
  selector: 'app-admin-home',
  imports: [RouterLink, PageHead],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Administration" lead="Tout ce que le bureau gère, par thème." />

      @if (toValidate() > 0) {
        <p class="alert alert-warning">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 3l9 17H3zM12 10v4M12 17h.01"></path></svg>
          <span>
            <span class="alert-title">{{ toValidate() }} adhésion{{ toValidate() > 1 ? 's' : '' }} à valider.</span>&ngsp;
            <a routerLink="/admin/users">Voir les adhérents</a>
          </span>
        </p>
      }

      @if (ordersToHandOver() > 0) {
        <p class="alert alert-warning">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M3 4h2l2 12h11l2-8H6"></path><circle cx="9" cy="20" r="1"></circle><circle cx="17" cy="20" r="1"></circle></svg>
          <span>
            <span class="alert-title">{{ ordersToHandOver() }} commande{{ ordersToHandOver() > 1 ? 's' : '' }} à remettre.</span>&ngsp;
            <a routerLink="/admin/shop/orders">Voir les commandes</a>
          </span>
        </p>
      }

      <section class="stack stack-s" aria-labelledby="g-members">
        <h2 id="g-members" style="font-size: var(--text-h4)">Membres et adhésions</h2>
        <div class="grid-auto" style="--col-min: 26rem">
          <a class="tile" routerLink="/admin/users">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="9" cy="8" r="3.5"></circle><path d="M2 20a7 7 0 0 1 14 0M17 5a3.5 3.5 0 0 1 0 7M22 20a7 7 0 0 0-4-6"></path></svg>
            <span>Adhérents<small>Comptes, invitations, adhésions, titres</small></span>
          </a>
          <a class="tile" routerLink="/admin/seasons">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><rect x="4" y="3" width="16" height="18" rx="2"></rect><path d="M8 8h8M8 12h8M8 16h5"></path></svg>
            <span>Saisons et cotisations<small>Dates, tolérance, montants</small></span>
          </a>
          <a class="tile" routerLink="/admin/seasons/statistics">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M5 20V10M12 20V4M19 20v-7"></path></svg>
            <span>Statistiques<small>Adhésions par saison</small></span>
          </a>
          <a class="tile" routerLink="/admin/titles">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M5 20h14M12 4l2.5 5 5.5.8-4 3.9.9 5.3-4.9-2.6-4.9 2.6.9-5.3-4-3.9 5.5-.8z"></path></svg>
            <span>Titres du bureau<small>Président, trésorier…</small></span>
          </a>
        </div>
      </section>

      <section class="stack stack-s" aria-labelledby="g-com">
        <h2 id="g-com" style="font-size: var(--text-h4)">Communication</h2>
        <div class="grid-auto" style="--col-min: 26rem">
          <a class="tile" routerLink="/admin/announcements">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M4 5h16v14H4zM8 10h8M8 14h5"></path></svg>
            <span>Annonces et sondages<small>Publier, épingler, terminer</small></span>
          </a>
        </div>
      </section>

      <section class="stack stack-s" aria-labelledby="g-ic">
        <h2 id="g-ic" style="font-size: var(--text-h4)">Interclubs</h2>
        <div class="grid-auto" style="--col-min: 26rem">
          <a class="tile" routerLink="/interclubs/teams">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M12 3l8 4v5c0 5-3.5 8-8 9-4.5-1-8-4-8-9V7z"></path></svg>
            <span>Équipes et capitaines<small>Effectifs</small></span>
          </a>
          <a class="tile" routerLink="/interclubs">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="5" width="18" height="16" rx="2"></rect><path d="M3 10h18M8 3v4M16 3v4"></path></svg>
            <span>Rencontres<small>Calendrier, résultats</small></span>
          </a>
          <!-- Masqué avec les propositions de dates :
          <a class="tile" routerLink="/interclubs/proposals">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="9"></circle><path d="M8 12l3 3 5-6"></path></svg>
            <span>Disponibilités<small>Dates proposées, réponses</small></span>
          </a>
          -->
        </div>
      </section>

      <section class="stack stack-s" aria-labelledby="g-shop">
        <h2 id="g-shop" style="font-size: var(--text-h4)">Boutique</h2>
        <div class="grid-auto" style="--col-min: 26rem">
          <a class="tile" routerLink="/admin/shop">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M4 7h16l-1.5 12h-13zM9 7a3 3 0 0 1 6 0"></path></svg>
            <span>Articles et stock<small>Variantes, alertes</small></span>
          </a>
          <a class="tile" routerLink="/admin/shop/orders">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M3 4h2l2 12h11l2-8H6"></path><circle cx="9" cy="20" r="1"></circle><circle cx="17" cy="20" r="1"></circle></svg>
            <span>Commandes<small>Préparation, remise en main propre</small></span>
          </a>
          <a class="tile" routerLink="/admin/shop/audit-log">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M4 5h16v14H4zM8 9h8M8 13h8M8 17h4"></path></svg>
            <span>Journal de la boutique<small>Stock, prix, commandes</small></span>
          </a>
        </div>
      </section>

      <section class="stack stack-s" aria-labelledby="g-log">
        <h2 id="g-log" style="font-size: var(--text-h4)">Journal</h2>
        <div class="grid-auto" style="--col-min: 26rem">
          <a class="tile" routerLink="/admin/audit-log">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="9"></circle><path d="M12 7v5l3 2"></path></svg>
            <span>Actions du bureau<small>Qui a fait quoi, quand</small></span>
          </a>
        </div>
      </section>
    </main>
  `,
})
export class AdminHomePage implements OnInit {
  protected readonly trail: Crumb[] = [];
  private readonly users = inject(UsersDirectory);
  private readonly shop = inject(ShopAlerts);
  protected readonly toValidate = signal(0);
  protected readonly ordersToHandOver = signal(0);

  async ngOnInit(): Promise<void> {
    try {
      // The list has no filter on the membership: the club is small enough to count on one page.
      const page = await this.users.users({ sort: 'membership', page: 1, pageSize: 100 });
      this.toValidate.set(page.items.filter((u) => u.status === 'active' && u.membership === 'to_validate').length);
    } catch {
      this.toValidate.set(0);
    }
    this.ordersToHandOver.set(await this.shop.ordersToHandOver());
  }
}
