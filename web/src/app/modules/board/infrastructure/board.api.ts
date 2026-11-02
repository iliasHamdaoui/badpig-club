import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API } from '../../../shared/infrastructure/api';
import { ListQuery, Page, listParams } from '../../../shared/core/page';
import { BoardMember, TitleDef } from '../core/title';

@Injectable({ providedIn: 'root' })
export class BoardApi {
  private readonly http = inject(HttpClient);

  changeBoard(id: string, add: boolean): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${API}/admin/users/${id}/board/${add ? 'add' : 'remove'}`, {}));
  }

  changeTitle(id: string, titleId: string | null): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/admin/users/${id}/title`, { titleId }));
  }

  titles(query: ListQuery): Promise<Page<TitleDef>> {
    return firstValueFrom(this.http.get<Page<TitleDef>>(`${API}/admin/titles`, { params: listParams(query) }));
  }

  /** Every title (there are only a few), by name: for the title choice list. */
  /** The people of the board, with their title and e-mail: reserved to the club members (the API checks it). */
  boardMembers(): Promise<Page<BoardMember>> {
    return firstValueFrom(this.http.get<Page<BoardMember>>(`${API}/club/board`, { params: listParams({ sort: 'title', pageSize: 100 }) }));
  }

  async allTitles(): Promise<TitleDef[]> {
    return (await this.titles({ sort: 'label', direction: 'asc', pageSize: 100 })).items;
  }

  /** Returns the id of the new title. */
  createTitle(label: string): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(`${API}/admin/titles`, { label }));
  }

  renameTitle(id: string, label: string): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/admin/titles/${id}`, { label }));
  }

  deleteTitle(id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API}/admin/titles/${id}`));
  }
}
