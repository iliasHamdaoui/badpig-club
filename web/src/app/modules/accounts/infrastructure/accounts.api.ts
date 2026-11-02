import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { MembershipKind } from '../../../shared/core/membership';
import { API } from '../../../shared/infrastructure/api';
import { Profile } from '../core/profile';

@Injectable({ providedIn: 'root' })
export class AccountsApi {
  private readonly http = inject(HttpClient);

  myProfile(): Promise<Profile> {
    return firstValueFrom(this.http.get<Profile>(`${API}/me`));
  }

  updateMyProfile(name: string, email: string): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/me`, { name, email }));
  }

  /** Records my federation licence number; an empty value removes the one on record. */
  setMyLicence(number: string): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/me/licence`, { number }));
  }

  deleteMyAccount(): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API}/me`));
  }

  /** `group`: "members", "board" or "admin" (only an Admin may invite an administrator). Returns the id of the new account. */
  invite(name: string, email: string, group: 'members' | 'board' | 'admin', membershipKind: MembershipKind): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(`${API}/admin/invitations`, { name, email, group, membershipKind }));
  }

  /** Records the licence number of a person (board); an empty value removes the one on record. */
  setLicence(id: string, number: string): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/admin/users/${id}/licence`, { number }));
  }

  resendInvitation(id: string): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${API}/admin/users/${id}/resend-invitation`, {}));
  }

  disable(id: string): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${API}/admin/users/${id}/disable`, {}));
  }

  enable(id: string): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${API}/admin/users/${id}/enable`, {}));
  }

  deleteUser(id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API}/admin/users/${id}`));
  }
}
