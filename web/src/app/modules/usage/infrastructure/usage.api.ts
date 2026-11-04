import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ListQuery, Page, listParams } from '../../../shared/core/page';
import { API } from '../../../shared/infrastructure/api';
import { UsageDay, UsageItem } from '../core/usage-item';

@Injectable({ providedIn: 'root' })
export class UsageApi {
  private readonly http = inject(HttpClient);

  /** The pages of the website over the last `days` days (7, 30, 90 or 365). */
  statistics(query: ListQuery, days: number): Promise<Page<UsageItem>> {
    return firstValueFrom(this.http.get<Page<UsageItem>>(`${API}/admin/usage`, { params: { ...listParams(query), days } }));
  }

  /** The use of the website day by day over the last `days` days, for the chart. */
  async trend(days: number): Promise<UsageDay[]> {
    const response = await firstValueFrom(this.http.get<{ days: UsageDay[] }>(`${API}/admin/usage/trend`, { params: { days } }));
    return response.days;
  }
}
