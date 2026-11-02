import { Component, effect, inject, signal, untracked } from '@angular/core';
import { AdminUser } from '../shared/core/admin-user';
import { ListState, Page } from '../shared/core/page';
import { AdminActivity } from '../shared/infrastructure/admin-activity';
import { UsersDirectory } from '../shared/infrastructure/users-directory';
import { PageSize, Pager, SearchBox, SortHeader, SortSelect } from '../shared/ui/list-controls';
import { AccountActions, AccountStatus, InviteUserForm, LicenceField } from '../modules/accounts';
import { BoardActions, BoardApi, TitleDef, TitleSelect } from '../modules/board';
import { MembershipActions, MembershipStatus } from '../modules/memberships';
import { MenuDetails } from '../shared/ui/menu-details';
import { Crumb, PageHead } from '../shared/ui/page-head';

/**
 * Administration page (host): lays out the user list and assembles the pieces each module provides
 * (account actions, membership, board and titles). The modules do not know each other.
 */
@Component({
  selector: 'app-admin-users',
  host: { class: 'bleed' },
  imports: [SearchBox, SortHeader, SortSelect, Pager, PageSize, InviteUserForm, AccountStatus, AccountActions, LicenceField, MembershipStatus, MembershipActions, BoardActions, TitleSelect, MenuDetails, PageHead],
  providers: [AdminActivity],
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Adhérents" />

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

      <details class="card panel">
        <summary class="card-title">
          <svg class="ic ic-s chevron" viewBox="0 0 24 24" aria-hidden="true"><path d="M9 6l6 6-6 6"></path></svg>
          Inviter une personne
        </summary>
        <app-invite-user-form />
      </details>

      <div class="list-tools stack stack-s">
        <div class="filters">
          <app-search-box label="Nom, e-mail ou licence" (searched)="list.setSearch($event)" />
          <app-sort-select [state]="list" [fields]="sortFields" />
          <app-page-size [state]="list" />
        </div>
        <div class="row" style="--row-gap: var(--space-10)">
          @for (filter of groupFilters; track filter.value) {
            <button type="button" class="chip" [attr.aria-pressed]="group() === filter.value" (click)="setGroup(filter.value)">
              {{ filter.label }}
            </button>
          }
        </div>
        @if (loaded()) {
          <p class="small-text" aria-live="polite">
            {{ users().totalCount }} personne{{ users().totalCount > 1 ? 's' : '' }}
          </p>
        }
      </div>

      @if (!loaded()) {
        <div class="stack" aria-hidden="true"><span class="skeleton" style="height: 6rem"></span><span class="skeleton" style="height: 6rem"></span></div>
      } @else if (users().items.length === 0) {
        <p class="empty">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="11" cy="11" r="7"></circle><path d="M20 20l-4-4"></path></svg>
          <strong>Aucun résultat</strong>
          <span>Essayez une autre recherche.</span>
        </p>
      } @else {
        <table class="rtable" aria-label="Adhérents">
          <thead>
            <tr>
              <th scope="col"><app-sort-header [state]="list" field="name" label="Nom" /></th>
              <th scope="col">Groupes et titre</th>
              <th scope="col"><app-sort-header [state]="list" field="status" label="Compte" /></th>
              <th scope="col"><app-sort-header [state]="list" field="membership" label="Adhésion" /></th>
              <th scope="col">Licence</th>
              <th scope="col">Actions</th>
            </tr>
          </thead>
          <tbody>
            @for (u of users().items; track u.id) {
              <tr>
                <td class="cell-main" data-label="Nom">
                  <strong>{{ u.name }}</strong>
                  <span class="small-text" style="overflow-wrap: anywhere">{{ u.email }}</span>
                </td>
                <td data-label="Groupes et titre">
                  <span class="stack stack-s" style="justify-items: start">
                    <span class="small-text">{{ u.groups.join(', ') }}</span>
                    <app-title-select [user]="u" [titles]="titles()" />
                  </span>
                </td>
                <td data-label="Compte"><app-account-status [user]="u" /></td>
                <td data-label="Adhésion"><app-membership-status [user]="u" /></td>
                <td data-label="Licence"><app-licence-field [user]="u" /></td>
                <td class="cell-actions" data-label="Actions">
                  <div class="actions">
                    <app-membership-actions [user]="u" />
                    @if (hasMoreActions(u)) {
                      <details class="more">
                        <summary class="btn btn-ghost btn-sm">Plus d'actions</summary>
                        <div class="more-panel">
                          <app-account-actions [user]="u" />
                          <app-board-actions [user]="u" />
                        </div>
                      </details>
                    }
                    @if (!u.canValidateMembership && !hasMoreActions(u)) {
                      <span class="small-text">Votre compte : la gestion passe par un autre membre du bureau.</span>
                    }
                  </div>
                </td>
              </tr>
            }
          </tbody>
        </table>
        <app-pager [page]="users().page" [totalPages]="users().totalPages" [totalCount]="users().totalCount" (pageChange)="list.goTo($event)" />
        <p class="small-text">Un compte désactivé ne peut plus se connecter ; il est supprimé automatiquement au bout d'un mois, sauf si on le réactive avant. La suppression est définitive.</p>
      }
    </main>
  `,
  styles: `
    summary { cursor: pointer; }
    .list-tools { display: grid; gap: var(--space-10); }
    /* Column widths, so that an e-mail is not cut in the middle of a word and the actions always start at the same place. */
    @container vp (min-width: 76.8rem) {
      /* Fixed layout: the columns follow these widths instead of the length of an e-mail. */
      .rtable { table-layout: fixed; }
      .rtable th:nth-child(1), .rtable td:nth-child(1) { width: 20%; }
      .rtable th:nth-child(2), .rtable td:nth-child(2) { width: 13%; }
      .rtable th:nth-child(3), .rtable td:nth-child(3) { width: 15%; }
      .rtable th:nth-child(4), .rtable td:nth-child(4) { width: 18%; }
      .rtable th:nth-child(5), .rtable td:nth-child(5) { width: 12%; }
    }
  `,
})
export class AdminUsersPage {
  protected readonly trail: Crumb[] = [{ label: 'Administration', path: '/admin' }, { label: 'Adhérents' }];
  private readonly directory = inject(UsersDirectory);
  private readonly board = inject(BoardApi);
  protected readonly activity = inject(AdminActivity);
  protected readonly list = new ListState('name');
  /** Nothing to offer when the person may not act on this account (their own, for instance). */
  protected hasMoreActions(user: AdminUser): boolean {
    return user.canManage || user.boardAction !== null;
  }

  /** Everybody is in "Membres", so the first filter means the people who are neither on the board nor administrators. */
  protected readonly groupFilters = [
    { value: '', label: 'Tout le club' },
    { value: 'members', label: 'Adhérents' },
    { value: 'board', label: 'Bureau' },
    { value: 'admin', label: 'Administrateurs' },
  ];
  protected readonly group = signal('');

  protected setGroup(value: string): void {
    if (value !== this.group()) {
      this.group.set(value);
      this.list.goTo(1);
    }
  }

  protected readonly sortFields = [
    { field: 'name', label: 'Nom' },
    { field: 'email', label: 'E-mail' },
    { field: 'status', label: 'Compte' },
    { field: 'membership', label: 'Adhésion' },
  ];
  protected readonly users = signal<Page<AdminUser>>({ items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 });
  protected readonly titles = signal<TitleDef[]>([]);
  protected readonly loaded = signal(false);

  constructor() {
    // Loads at start, after every action of a module component, and when the search, the sort or the page change.
    effect(() => {
      this.activity.changed();
      const query = { ...this.list.query(), group: this.group() };
      untracked(() => void this.reload(query));
    });
  }

  private async reload(query: ReturnType<ListState['query']> & { group: string }): Promise<void> {
    try {
      const [users, titles] = await Promise.all([this.directory.users(query), this.board.allTitles()]);
      if (users.items.length === 0 && users.page > 1) {
        this.list.goTo(users.totalPages); // the last page disappeared (e.g. its only account was deleted)
        return;
      }
      this.users.set(users);
      this.titles.set(titles);
      this.loaded.set(true);
    } catch {
      this.activity.error.set('Impossible de charger la liste des utilisateurs.');
    }
  }
}
