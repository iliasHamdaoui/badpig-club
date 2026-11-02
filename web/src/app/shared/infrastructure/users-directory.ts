import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AdminUser } from '../core/admin-user';
import { ListQuery, Page, listParams } from '../core/page';
import { API } from './api';

/** Read access to the administration user list (one page at a time, see `AdminUser`). */
@Injectable({ providedIn: 'root' })
export class UsersDirectory {
  private readonly http = inject(HttpClient);

  users(query: ListQuery): Promise<Page<AdminUser>> {
    return firstValueFrom(this.http.get<Page<AdminUser>>(`${API}/admin/users`, { params: listParams(query) }));
  }
}
