import { Component, effect, inject, signal, untracked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute } from '@angular/router';
import { ListState, Page } from '../../../../shared/core/page';
import { formatLongDate } from '../../../../shared/core/format';
import { AdminActivity } from '../../../../shared/infrastructure/admin-activity';
import { Pager, SearchBox } from '../../../../shared/ui/list-controls';
import { Crumb, PageHead } from '../../../../shared/ui/page-head';
import { SchoolHoliday, Season } from '../../core/season';
import { MembershipsApi } from '../../infrastructure/memberships.api';

const EMPTY = { items: [], page: 1, pageSize: 25, totalCount: 0, totalPages: 0 };

/** The periods of school holidays of a season: during them the club is closed and the site says there is no session. */
@Component({
  selector: 'app-manage-holidays',
  imports: [FormsModule, SearchBox, Pager, PageHead],
  providers: [AdminActivity],
  host: { class: 'bleed' },
  template: `
    <main class="container stack stack-l" style="padding-block: var(--space-30) var(--space-60); max-width: 84rem">
      <app-page-head [trail]="trail" [title]="title()" lead="Pendant ces périodes, le club est fermé : le site annonce qu'il n'y a pas de séance et donne la prochaine." />

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
          Ajouter des vacances
        </summary>
        <form class="stack stack-s" (ngSubmit)="add()">
          <div class="cols cols-2">
            <div class="field">
              <label class="label" for="h-name">Nom</label>
              <input class="input" id="h-name" name="name" [(ngModel)]="draft.name" maxlength="60" placeholder="Vacances de la Toussaint" required autocomplete="off" />
            </div>
            <div class="field">
              <label class="label" for="h-start">Premier jour sans séance</label>
              <input class="input" id="h-start" name="startsOn" type="date" [(ngModel)]="draft.startsOn" required />
            </div>
            <div class="field">
              <label class="label" for="h-end">Dernier jour sans séance</label>
              <input class="input" id="h-end" name="endsOn" type="date" [(ngModel)]="draft.endsOn" required />
            </div>
          </div>
          <p class="form-actions">
            <button class="btn" type="submit" [class.is-busy]="activity.busy() === 'add'" [disabled]="activity.busy() !== null || !canAdd()">Ajouter</button>
          </p>
          <p class="small-text">Les deux jours sont compris. Les dates restent dans la saison et ne chevauchent pas une autre période.</p>
        </form>
      </details>

      <app-search-box label="Rechercher des vacances…" (searched)="list.setSearch($event)" />

      @if (!loaded()) {
        <div class="stack stack-s" aria-hidden="true"><span class="skeleton" style="height: 6rem"></span><span class="skeleton" style="height: 6rem"></span></div>
      } @else if (holidays().items.length === 0) {
        <p class="empty">
          <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><rect x="3" y="5" width="18" height="16" rx="2"></rect><path d="M3 10h18M8 3v4M16 3v4"></path></svg>
          <strong>Aucune période</strong>
          <span>{{ list.search() ? 'Aucun résultat.' : "Ajoutez les vacances scolaires de la saison : le site s'en servira pour annoncer les séances." }}</span>
        </p>
      } @else {
        <ul class="periods">
          @for (h of holidays().items; track h.id) {
            <li class="card stack stack-s">
              @if (editing() === h.id) {
                <form class="stack stack-s" (ngSubmit)="save(h)">
                  <div class="cols cols-2">
                    <div class="field"><label class="label" [attr.for]="'n-' + h.id">Nom</label><input class="input" [id]="'n-' + h.id" name="editName" [(ngModel)]="edit.name" maxlength="60" required /></div>
                    <div class="field"><label class="label" [attr.for]="'s-' + h.id">Premier jour</label><input class="input" [id]="'s-' + h.id" name="editStart" type="date" [(ngModel)]="edit.startsOn" required /></div>
                    <div class="field"><label class="label" [attr.for]="'e-' + h.id">Dernier jour</label><input class="input" [id]="'e-' + h.id" name="editEnd" type="date" [(ngModel)]="edit.endsOn" required /></div>
                  </div>
                  <p class="form-actions">
                    <button class="btn btn-sm" type="submit" [class.is-busy]="activity.busy() === h.id" [disabled]="activity.busy() !== null">Enregistrer</button>
                    <button class="btn btn-ghost btn-sm" type="button" (click)="editing.set(null)">Annuler</button>
                  </p>
                </form>
              } @else {
                <div class="block-head">
                  <h2 class="card-title">{{ h.name }}</h2>
                  <span class="small-text">{{ nights(h) }} jour{{ nights(h) > 1 ? 's' : '' }}</span>
                </div>
                <p>Du {{ day(h.startsOn) }} au {{ day(h.endsOn) }} inclus.</p>
                <p class="form-actions">
                  <button class="btn btn-ghost btn-sm" type="button" [disabled]="activity.busy() !== null" (click)="startEdit(h)">Modifier</button>
                  <button class="btn btn-danger btn-sm" type="button" [class.is-busy]="activity.busy() === h.id" [disabled]="activity.busy() !== null" (click)="remove(h)">Supprimer</button>
                </p>
              }
            </li>
          }
        </ul>
        <app-pager [page]="holidays().page" [totalPages]="holidays().totalPages" [totalCount]="holidays().totalCount" (pageChange)="list.goTo($event)" />
      }
    </main>
  `,
  styles: `
    ul { list-style: none; display: grid; gap: var(--space-15); }
  `,
})
export class ManageHolidaysPage {
  protected readonly trail: Crumb[] = [
    { label: 'Administration', path: '/admin' },
    { label: 'Saisons', path: '/admin/seasons' },
    { label: 'Vacances scolaires' },
  ];
  protected readonly activity = inject(AdminActivity);
  private readonly api = inject(MembershipsApi);
  protected readonly seasonId = inject(ActivatedRoute).snapshot.paramMap.get('id') ?? '';
  protected readonly list = new ListState('startsOn', 'asc', 25);
  protected readonly holidays = signal<Page<SchoolHoliday>>(EMPTY);
  protected readonly season = signal<Season | null>(null);
  protected readonly loaded = signal(false);
  protected readonly editing = signal<string | null>(null);
  protected draft = { name: '', startsOn: '', endsOn: '' };
  protected edit = { name: '', startsOn: '', endsOn: '' };
  protected readonly day = formatLongDate;

  constructor() {
    void this.loadSeason();
    effect(() => {
      const query = this.list.query();
      this.activity.changed();
      untracked(() => void this.load(query));
    });
  }

  protected title(): string {
    const season = this.season();
    return season ? `Vacances scolaires ${season.name}` : 'Vacances scolaires';
  }

  protected canAdd(): boolean {
    return this.draft.name.trim().length > 0 && !!this.draft.startsOn && !!this.draft.endsOn;
  }

  /** Number of days without session, both days included. */
  protected nights(holiday: SchoolHoliday): number {
    const start = new Date(holiday.startsOn).getTime();
    const end = new Date(holiday.endsOn).getTime();
    return Math.round((end - start) / 86400000) + 1;
  }

  protected async add(): Promise<void> {
    const name = this.draft.name.trim();
    if (await this.activity.run('add', () => this.api.addHoliday(this.seasonId, { ...this.draft, name }), `${name} ajouté.`)) {
      this.draft = { name: '', startsOn: '', endsOn: '' };
    }
  }

  protected startEdit(holiday: SchoolHoliday): void {
    this.edit = { name: holiday.name, startsOn: holiday.startsOn, endsOn: holiday.endsOn };
    this.editing.set(holiday.id);
  }

  protected async save(holiday: SchoolHoliday): Promise<void> {
    const name = this.edit.name.trim();
    if (await this.activity.run(holiday.id, () => this.api.updateHoliday(this.seasonId, holiday.id, { ...this.edit, name }), 'Période modifiée.')) {
      this.editing.set(null);
    }
  }

  protected async remove(holiday: SchoolHoliday): Promise<void> {
    await this.activity.run(holiday.id, () => this.api.deleteHoliday(this.seasonId, holiday.id), `${holiday.name} supprimé.`);
  }

  private async loadSeason(): Promise<void> {
    try {
      const page = await this.api.seasons({ pageSize: 100 });
      this.season.set(page.items.find((s) => s.id === this.seasonId) ?? null);
    } catch {
      this.season.set(null);
    }
  }

  private async load(query: Parameters<MembershipsApi['holidays']>[1]): Promise<void> {
    try {
      this.holidays.set(await this.api.holidays(this.seasonId, query));
      this.loaded.set(true);
    } catch {
      this.loaded.set(true);
    }
  }
}
