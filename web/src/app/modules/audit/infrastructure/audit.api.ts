import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ListQuery, Page, listParams } from '../../../shared/core/page';
import { API } from '../../../shared/infrastructure/api';
import { AuditEntry } from '../core/audit-actions';

@Injectable({ providedIn: 'root' })
export class AuditApi {
  private readonly http = inject(HttpClient);

  auditLog(query: ListQuery): Promise<Page<AuditEntry>> {
    return firstValueFrom(this.http.get<Page<AuditEntry>>(`${API}/admin/audit-log`, { params: listParams(query) }));
  }
}
