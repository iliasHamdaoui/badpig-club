import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ListQuery, Page, listParams } from '../../../shared/core/page';
import { MembershipKind } from '../../../shared/core/membership';
import { API } from '../../../shared/infrastructure/api';
import { SchoolHoliday, SchoolHolidayInput, Season, SeasonInput, SeasonStatistics } from '../core/season';

@Injectable({ providedIn: 'root' })
export class MembershipsApi {
  private readonly http = inject(HttpClient);

  validate(id: string, kind: MembershipKind): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${API}/admin/users/${id}/membership/validate`, { kind }));
  }

  cancel(id: string): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${API}/admin/users/${id}/membership/cancel`, {}));
  }

  seasons(query: ListQuery): Promise<Page<Season>> {
    return firstValueFrom(this.http.get<Page<Season>>(`${API}/admin/seasons`, { params: listParams(query) }));
  }

  /** Returns the id of the new season. */
  createSeason(season: SeasonInput): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(`${API}/admin/seasons`, season));
  }

  updateSeason(id: string, season: SeasonInput): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/admin/seasons/${id}`, season));
  }

  deleteSeason(id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API}/admin/seasons/${id}`));
  }

  statistics(query: ListQuery): Promise<Page<SeasonStatistics>> {
    return firstValueFrom(this.http.get<Page<SeasonStatistics>>(`${API}/admin/seasons/statistics`, { params: listParams(query) }));
  }

  /** The periods of school holidays of a season, the earliest first. */
  holidays(seasonId: string, query: ListQuery): Promise<Page<SchoolHoliday>> {
    return firstValueFrom(this.http.get<Page<SchoolHoliday>>(`${API}/admin/seasons/${seasonId}/holidays`, { params: listParams(query) }));
  }

  addHoliday(seasonId: string, holiday: SchoolHolidayInput): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(`${API}/admin/seasons/${seasonId}/holidays`, holiday));
  }

  updateHoliday(seasonId: string, id: string, holiday: SchoolHolidayInput): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/admin/seasons/${seasonId}/holidays/${id}`, holiday));
  }

  deleteHoliday(seasonId: string, id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API}/admin/seasons/${seasonId}/holidays/${id}`));
  }
}
