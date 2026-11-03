import { Component, effect, inject, signal, untracked } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ListState, Page } from '../../../../shared/core/page';
import { Avatar } from '../../../../shared/ui/avatar';
import { Pager, SearchBox } from '../../../../shared/ui/list-controls';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';
import { DirectoryPerson } from '../../core/directory-person';
import { AccountsApi } from '../../infrastructure/accounts.api';

/** The club directory (« trombinoscope »): the members who chose to put a picture, for the members with an up-to-date membership. */
@Component({
  selector: 'app-club-directory',
  imports: [RouterLink, PageHead, SearchBox, Pager, Avatar],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Trombinoscope" lead="Les visages du club : pour mettre un nom sur celles et ceux que l'on croise au gymnase." />

      <div class="filters">
        <app-search-box label="Rechercher un nom…" (searched)="list.setSearch($event)" />
      </div>

      @if (error()) {
        <p class="alert alert-error" role="alert"><svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M8 3h8l5 5v8l-5 5H8l-5-5V8z"></path><path d="M12 8v5M12 16h.01"></path></svg><span>{{ error() }}</span></p>
      }

      @if (!loaded()) {
        <div class="grid" aria-hidden="true">
          @for (i of placeholders; track i) { <span class="skeleton" style="height: 22rem"></span> }
        </div>
      } @else if (people().items.length === 0) {
        <p class="empty">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="8" r="4"></circle><path d="M4 21c0-4 4-6 8-6s8 2 8 6"></path></svg>
          <strong>{{ list.search() ? 'Aucun résultat' : 'Personne pour le moment' }}</strong>
          <span>
            @if (list.search()) { Aucun nom ne correspond à cette recherche. }
            @else { Seuls les adhérents qui ont ajouté une photo apparaissent ici. <a routerLink="/profile">Ajouter ma photo</a> }
          </span>
        </p>
      } @else {
        <ul class="grid">
          @for (person of people().items; track person.id) {
            <li class="person card">
              <app-avatar [accountId]="person.id" [name]="person.name" [version]="person.photoVersion" size="xl" />
              <strong>{{ person.name }}</strong>
            </li>
          }
        </ul>
        <app-pager [page]="people().page" [totalPages]="people().totalPages" [totalCount]="people().totalCount" (pageChange)="list.goTo($event)" />
        <p class="small-text">Seuls les adhérents qui l'ont choisi apparaissent ici : chacun ajoute, change ou retire sa photo depuis <a routerLink="/profile">son profil</a>.</p>
      }
    </main>
  `,
  styles: `
    .grid { display: grid; grid-template-columns: repeat(auto-fill, minmax(20rem, 1fr)); gap: var(--space-15); list-style: none; padding: 0; }
    .person { display: grid; justify-items: center; gap: var(--space-10); text-align: center; overflow-wrap: anywhere; }
  `,
})
export class ClubDirectoryPage {
  protected readonly trail: Crumb[] = [{ label: 'Le club', path: '/club' }, { label: 'Trombinoscope' }];
  protected readonly placeholders = [1, 2, 3, 4, 5, 6];
  private readonly api = inject(AccountsApi);
  protected readonly list = new ListState('name', 'asc', 48);
  protected readonly people = signal<Page<DirectoryPerson>>({ items: [], page: 1, pageSize: 48, totalCount: 0, totalPages: 0 });
  protected readonly loaded = signal(false);
  protected readonly error = signal('');

  constructor() {
    effect(() => {
      const query = this.list.query();
      untracked(() => void this.reload(query));
    });
  }

  private async reload(query: ReturnType<ListState['query']>): Promise<void> {
    try {
      this.people.set(await this.api.listDirectory(query));
      this.error.set('');
    } catch {
      this.error.set('Impossible de charger le trombinoscope.');
    } finally {
      this.loaded.set(true);
    }
  }
}
