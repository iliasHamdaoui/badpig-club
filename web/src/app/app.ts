import { Component, effect, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { AccountsApi } from './modules/accounts';
import { LiveFixturesBanner } from './modules/interclubs';
import { UsageTracker } from './shared/infrastructure/usage-tracker';
import { AuthService } from './shared/infrastructure/auth.service';
import { SiteFooter, SiteHeader } from './shared/ui/site-chrome';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, LiveFixturesBanner, SiteHeader, SiteFooter],
  template: `
    <div class="vp-in">
      <app-site-header />
      @if (auth.isMember() && auth.membershipUpToDate() !== false) {
        <app-live-fixtures />
      }
      <main id="content" tabindex="-1"><router-outlet /></main>
      <app-site-footer />
    </div>
  `,
  styles: `
    .vp-in { display: flex; flex-direction: column; min-height: 100vh; }
    main { flex: 1; }
    main:focus { outline: none; }
  `,
})
export class App {
  protected readonly auth = inject(AuthService);
  private readonly api = inject(AccountsApi);

  constructor() {
    inject(UsageTracker).start();
    // As soon as the person is signed in, check whether the membership is up to date (to show or hide the members area).
    effect(() => {
      if (this.auth.signedIn()) {
        this.api.myProfile().then((p) => this.auth.membershipUpToDate.set(p.membershipUpToDate), () => undefined);
      } else {
        this.auth.membershipUpToDate.set(null);
      }
    });
  }
}
