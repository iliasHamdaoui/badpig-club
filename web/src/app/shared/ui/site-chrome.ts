import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../infrastructure/auth.service';
import { MenuDetails } from './menu-details';

/** One entry of the main navigation. */
interface NavItem {
  path: string;
  label: string;
  exact?: boolean;
}

/**
 * Header of the site: the logo, the main navigation and the session.
 * Below 1024 the entries move into a panel opened by the "Menu" button; from 1024 they sit in a row, with the name and the sign out button.
 */
@Component({
  selector: 'app-site-header',
  imports: [RouterLink, RouterLinkActive, MenuDetails],
  template: `
    <a class="skip-link btn btn-sm" href="#content">Aller au contenu</a>
    <header class="site-header">
      <div class="container site-header-bar">
        <a class="logo" routerLink="/" aria-label="BadPig, accueil"><img class="logo-img" src="assets/logo.png" alt="" /><span>BadPig</span></a>

        <nav class="site-nav" aria-label="Navigation principale">
          @for (item of items(); track item.path) {
            <a [routerLink]="item.path" routerLinkActive ariaCurrentWhenActive="page" [routerLinkActiveOptions]="{ exact: item.exact ?? false }">{{ item.label }}</a>
          }
        </nav>

        <div class="header-end">
          @if (!auth.signedIn()) {
            <button class="btn" [class.is-busy]="auth.busy()" [disabled]="auth.busy()" (click)="auth.signIn(location.pathname)">
              {{ auth.busy() ? 'Connexion…' : 'Se connecter' }}
            </button>
          } @else {
            <details class="account" #account>
              <summary class="btn btn-ghost btn-sm" [attr.aria-label]="'Mon compte, ' + auth.displayName()">
                <span class="avatar" aria-hidden="true">{{ initials() }}</span>
                <span class="account-name">{{ auth.displayName() }}</span>
                <svg class="ic ic-s" viewBox="0 0 24 24" aria-hidden="true"><path d="M6 9l6 6 6-6"></path></svg>
              </summary>
              <div class="account-panel" (click)="account.open = false">
                <p class="account-who">
                  <strong>{{ auth.displayName() }}</strong><br />
                  <span class="small-text">{{ roleLabel() }}</span>
                </p>
                <a routerLink="/profile">
                  <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="8" r="4"></circle><path d="M4 21a8 8 0 0 1 16 0"></path></svg>
                  Mon profil
                </a>
                <button class="btn btn-ghost btn-sm" [class.is-busy]="auth.busy()" [disabled]="auth.busy()" (click)="auth.signOut()">
                  {{ auth.busy() ? 'Déconnexion…' : 'Déconnexion' }}
                </button>
              </div>
            </details>
          }
          <button class="btn btn-ghost btn-icon menu-toggle" type="button" [attr.aria-expanded]="open()" aria-controls="menu"
                  [attr.aria-label]="open() ? 'Fermer le menu' : 'Ouvrir le menu'" (click)="open.set(!open())">
            @if (open()) {
              <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M6 6l12 12M18 6L6 18"></path></svg>
            } @else {
              <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M4 7h16M4 12h16M4 17h16"></path></svg>
            }
          </button>
        </div>
      </div>

      @if (open()) {
        <div id="menu" class="menu-panel" (click)="open.set(false)">
          <nav class="container" aria-label="Navigation principale">
            @if (auth.signedIn()) {
              <div class="menu-user">
                <div class="row">
                  <span class="avatar" aria-hidden="true">{{ initials() }}</span>
                  <span><strong>{{ auth.displayName() }}</strong><br /><span class="small-text">{{ roleLabel() }}</span></span>
                </div>
                <button class="btn btn-ghost btn-sm" [disabled]="auth.busy()" (click)="auth.signOut()">Déconnexion</button>
              </div>
            }
            @for (item of items(); track item.path) {
              <a [routerLink]="item.path" routerLinkActive ariaCurrentWhenActive="page" [routerLinkActiveOptions]="{ exact: item.exact ?? false }">{{ item.label }}</a>
            }
            @if (auth.signedIn()) {
              <a routerLink="/profile" routerLinkActive ariaCurrentWhenActive="page">Mon profil</a>
            }
          </nav>
        </div>
      }
    </header>
  `,
  styles: `
    :host { display: block; position: sticky; top: 0; z-index: var(--z-header); }
    /* The skip link stays out of sight until it takes the focus. */
    .skip-link:not(:focus) { position: absolute; width: 0.1rem; height: 0.1rem; overflow: hidden; clip-path: inset(50%); white-space: nowrap; }
    .menu-panel { position: absolute; left: 0; right: 0; }
    /* On a narrow screen the bar must hold the logo, the session and the menu button: the name of the club steps aside. */
    @container vp (max-width: 47.9rem) { .logo span { display: none; } }

    /* The account: one button that opens the profile and the sign out, instead of two entries in the navigation. */
    .avatar {
      display: grid; place-items: center; width: 3.2rem; height: 3.2rem; flex: none; border-radius: 50%;
      background: var(--selected-background); color: var(--selected-text); font-size: var(--text-small); font-weight: var(--weight-bold);
    }
    .account { position: relative; }
    /* The same height as a navigation entry: a two-line button made the bar look out of line. */
    .account > summary {
      list-style: none; display: inline-flex; align-items: center; gap: var(--space-10);
      min-height: 4rem; padding-inline: var(--space-10); border-color: var(--transparent); font-weight: var(--weight-regular);
    }
    .account > summary:hover { border-color: var(--button-ghost-border); }
    .account[open] > summary { border-color: var(--button-ghost-border); background: var(--button-ghost-background-hover); }
    .account > summary::-webkit-details-marker { display: none; }
    .account-name { display: none; max-width: 18rem; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
    .account-who { padding: var(--space-5) var(--space-10) var(--space-10); border-bottom: 0.1rem solid var(--color-border); }
    .account-panel {
      position: absolute; right: 0; top: calc(100% + var(--space-5)); z-index: var(--z-menu); display: grid; gap: var(--space-5);
      min-width: 22rem; padding: var(--space-10); background: var(--color-surface);
      border: 0.1rem solid var(--color-border); border-radius: var(--radius-m); box-shadow: var(--shadow-m);
    }
    .account-panel a {
      display: flex; align-items: center; gap: var(--space-10); min-height: var(--tap-min); padding: 0 var(--space-10);
      border-radius: var(--radius-m); color: var(--color-text); text-decoration: none;
    }
    .account-panel a:hover { background: var(--nav-link-background-hover); color: var(--color-text); }
    @container vp (min-width: 102.4rem) { .account-name { display: block; } }
  `,
})
export class SiteHeader {
  protected readonly auth = inject(AuthService);
  protected readonly location = location;
  protected readonly open = signal(false);

  /** The entries the person may see; the guards and the API check it again. */
  protected items(): NavItem[] {
    const items: NavItem[] = [
      { path: '/', label: 'Accueil', exact: true },
      { path: '/club', label: 'Le club' },
      { path: '/announcements', label: 'Annonces' },
    ];
    if (this.auth.isMember() && this.auth.membershipUpToDate() !== false) {
      items.push({ path: '/interclubs', label: 'Interclubs' }, { path: '/shop', label: 'Boutique' });
    }
    if (this.auth.isBoard()) {
      items.push({ path: '/admin', label: 'Administration' });
    }
    return items;
  }

  /** The initials shown in the round badge of the account button. */
  protected initials(): string {
    return this.auth.displayName().split(' ').filter((w) => w).slice(0, 2).map((w) => w[0].toUpperCase()).join('');
  }

  protected roleLabel(): string {
    return this.auth.isBoard() ? 'Bureau' : 'Adhérent';
  }
}

/** Footer of the site: who we are, how to reach us, and the public pages. */
@Component({
  selector: 'app-site-footer',
  imports: [RouterLink],
  template: `
    <footer class="site-footer">
      <div class="container">
        <div class="footer-grid">
          <div class="stack stack-s">
            <h2>BadPig</h2>
            <p class="small-text">Badminton Pignan — séances les mardis, mercredis et vendredis, de 20h à 23h, au gymnase du Collège Marie Curie.</p>
          </div>
          <div>
            <h2>Contact</h2>
            <p class="small-text"><a href="mailto:contact@badpig.fr">contact&#64;badpig.fr</a></p>
            <p class="row" style="--row-gap: var(--space-15)">
              <a class="social" href="https://www.facebook.com/BadPig34/" target="_blank" rel="noopener">
                <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M14 9h3V5.5h-2.5A3.5 3.5 0 0 0 11 9v2H8.5v3.5H11V21h3.5v-6.5H17L17.5 11H14V9.5a.5.5 0 0 1 .5-.5z"></path></svg>
                Facebook
              </a>
              <a class="social" href="https://www.instagram.com/badminton_pignan/" target="_blank" rel="noopener">
                <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><rect x="3.5" y="3.5" width="17" height="17" rx="5"></rect><circle cx="12" cy="12" r="4"></circle><path d="M17 7h.01"></path></svg>
                Instagram
              </a>
            </p>
          </div>
          <nav aria-label="Liens du pied de page">
            <h2>Le site</h2>
            <ul>
              <li><a routerLink="/club">Le club</a></li>
              <li><a routerLink="/announcements">Annonces</a></li>
            </ul>
          </nav>
        </div>
        <p class="small-text footer-bottom">© BadPig — Badminton Pignan</p>
      </div>
    </footer>
  `,
  styles: `.social { display: inline-flex; align-items: center; gap: var(--space-5); min-height: var(--tap-min); }`,
})
export class SiteFooter {}
