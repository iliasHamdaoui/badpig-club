import { Component, inject } from '@angular/core';
import { AuthService } from '../../../../shared/infrastructure/auth.service';

@Component({
  selector: 'app-members',
  template: `
    <section class="card">
      <h1>Espace membres</h1>
      <p>Bonjour {{ auth.displayName() }}. Cet espace est réservé aux adhérents du club.</p>
      <p>Ici viendront : planning des créneaux, inscriptions aux tournois, documents du club…</p>
    </section>
  `,
})
export class MembersAreaPage {
  protected readonly auth = inject(AuthService);
}
