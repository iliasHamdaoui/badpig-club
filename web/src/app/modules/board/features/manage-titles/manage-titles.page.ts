import { Component, effect, inject, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ListState, Page } from '../../../../shared/core/page';
import { AdminActivity } from '../../../../shared/infrastructure/admin-activity';
import { Pager, SearchBox, SortHeader } from '../../../../shared/ui/list-controls';
import { TitleDef } from '../../core/title';
import { BoardApi } from '../../infrastructure/board.api';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';

@Component({
  selector: 'app-manage-titles',
  imports: [FormsModule, SearchBox, SortHeader, Pager, PageHead],
  providers: [AdminActivity],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60)">
      <app-page-head [trail]="trail" title="Titres du bureau" />

      <h2>Créer un titre</h2>
      <form class="new-title" (ngSubmit)="create()">
        <div>
          <label for="new-title">Nom du titre</label>
          <input class="input" id="new-title" name="newTitle" [(ngModel)]="newTitle" required maxlength="60" autocomplete="off" />
        </div>
        <button class="btn" type="submit" [class.is-busy]="creating()" [disabled]="creating() || !newTitle.trim()">{{ creating() ? 'Création…' : 'Créer' }}</button>
      </form>

      @if (activity.message()) { <p class="success">{{ activity.message() }}</p> }
      @if (activity.error()) { <p class="error">{{ activity.error() }}</p> }

      <h2>Titres existants</h2>
      <app-search-box label="Rechercher un titre ou une personne…" (searched)="list.setSearch($event)" />
      @if (!loaded()) {
        <p>Chargement…</p>
      } @else if (titles().items.length === 0) {
        <p>{{ list.search() ? 'Aucun résultat.' : 'Aucun titre pour le moment.' }}</p>
      } @else {
        <table>
          <thead><tr><th><app-sort-header [state]="list" field="label" label="Titre" /></th><th>Attribué à</th><th></th></tr></thead>
          <tbody>
            @for (t of titles().items; track t.id) {
              <tr>
                <td>
                  @if (editing() === t.id) {
                    <input class="input" [name]="'edit-' + t.id" [(ngModel)]="editedLabel" maxlength="60" aria-label="Nouveau nom du titre" />
                  } @else {
                    {{ t.label }}
                  }
                </td>
                <td>{{ holder(t) }}</td>
                <td>
                  <div class="actions">
                    @if (editing() === t.id) {
                      <button class="btn btn-sm" [class.is-busy]="activity.busy() === t.id" [disabled]="activity.busy() !== null || !editedLabel.trim()" (click)="rename(t)">Enregistrer</button>
                      <button class="btn btn-ghost btn-sm" [disabled]="activity.busy() !== null" (click)="editing.set(null)">Annuler</button>
                    } @else if (toDelete() === t.id) {
                      <span>{{ holder(t) ? 'Supprimer ce titre ? ' + holder(t) + ' le perdra.' : 'Supprimer ce titre ?' }}</span>
                      <button class="btn btn-danger btn-sm" [class.is-busy]="activity.busy() === t.id" [disabled]="activity.busy() !== null" (click)="remove(t)">Oui, supprimer</button>
                      <button class="btn btn-ghost btn-sm" [disabled]="activity.busy() !== null" (click)="toDelete.set(null)">Annuler</button>
                    } @else {
                      <button class="btn btn-ghost btn-sm" [disabled]="activity.busy() !== null" (click)="edit(t)">Renommer</button>
                      <button class="btn btn-danger btn-sm" [disabled]="activity.busy() !== null" (click)="toDelete.set(t.id)">Supprimer</button>
                    }
                  </div>
                </td>
              </tr>
            }
          </tbody>
        </table>
        <app-pager [page]="titles().page" [totalPages]="titles().totalPages" [totalCount]="titles().totalCount" (pageChange)="list.goTo($event)" />
        <p class="small-text">Un titre n'a qu'un titulaire, choisi parmi les membres du bureau. L'attribution se fait dans la liste des utilisateurs de l'administration.</p>
      }
    </main>
  `,
  styles: `
    .new-title { display: flex; flex-wrap: wrap; gap: 1.2rem; align-items: flex-end; }
    .new-title div { flex: 1 1 22.4rem; }
    .new-title label { margin-top: 0; }
  `,
})
export class ManageTitlesPage {
  protected readonly trail: Crumb[] = [{ label: 'Administration', path: '/admin' }, { label: 'Titres du bureau' }];
  private readonly api = inject(BoardApi);
  protected readonly activity = inject(AdminActivity);
  protected readonly list = new ListState('createdAt');
  protected readonly titles = signal<Page<TitleDef>>({ items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 });
  protected readonly loaded = signal(false);
  protected readonly editing = signal<string | null>(null);
  protected readonly toDelete = signal<string | null>(null);
  protected newTitle = '';
  protected editedLabel = '';
  protected readonly creating = () => this.activity.busy() === 'create';

  constructor() {
    // Loads at start, after every action, and when the search, the sort or the page change.
    effect(() => {
      this.activity.changed();
      const query = this.list.query();
      untracked(() => void this.reload(query));
    });
  }

  private async reload(query: ReturnType<ListState['query']>): Promise<void> {
    try {
      const titles = await this.api.titles(query);
      if (titles.items.length === 0 && titles.page > 1) {
        this.list.goTo(titles.totalPages); // the last page disappeared
        return;
      }
      this.titles.set(titles);
      this.loaded.set(true);
    } catch {
      this.activity.error.set('Impossible de charger les titres.');
    }
  }

  protected holder(t: TitleDef): string {
    return t.holderName ?? '';
  }

  protected edit(t: TitleDef): void {
    this.editedLabel = t.label;
    this.editing.set(t.id);
  }

  protected async create(): Promise<void> {
    const label = this.newTitle.trim();
    const ok = await this.activity.run('create', () => this.api.createTitle(this.newTitle), `Titre « ${label} » créé.`, 'La création a échoué.');
    if (ok) {
      this.newTitle = '';
    }
  }

  protected async rename(t: TitleDef): Promise<void> {
    const ok = await this.activity.run(t.id, () => this.api.renameTitle(t.id, this.editedLabel), `Titre renommé en « ${this.editedLabel.trim()} ».`);
    if (ok) {
      this.editing.set(null);
    }
  }

  protected async remove(t: TitleDef): Promise<void> {
    const ok = await this.activity.run(t.id, () => this.api.deleteTitle(t.id), `Titre « ${t.label} » supprimé.`);
    if (ok) {
      this.toDelete.set(null);
    }
  }
}
