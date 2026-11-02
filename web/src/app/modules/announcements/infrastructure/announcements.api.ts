import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API } from '../../../shared/infrastructure/api';
import { ListQuery, Page, listParams } from '../../../shared/core/page';
import { Announcement, AnnouncementComment, AnnouncementInput, AnnouncementItem, PollAnnouncementInput } from '../core/announcement';

/** Extra filters of the list (the board can filter on the kind and the status). */
export interface AnnouncementListQuery extends ListQuery {
  kind?: string;
  status?: string;
  form?: string;
}

@Injectable({ providedIn: 'root' })
export class AnnouncementsApi {
  private readonly http = inject(HttpClient);

  // --- Reading (public: visitors only get what they may read) ---

  list(query: AnnouncementListQuery): Promise<Page<AnnouncementItem>> {
    return firstValueFrom(this.http.get<Page<AnnouncementItem>>(`${API}/announcements`, { params: listParams(query) }));
  }

  get(id: string): Promise<Announcement> {
    return firstValueFrom(this.http.get<Announcement>(`${API}/announcements/${id}`));
  }

  /** Address of a poster or an attachment (the browser sends the session cookie itself). */
  fileUrl(announcementId: string, fileId: string): string {
    return `${API}/announcements/${announcementId}/files/${fileId}`;
  }

  like(id: string): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${API}/announcements/${id}/like`, {}));
  }

  unlike(id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API}/announcements/${id}/like`));
  }

  /** Answers the poll (or changes the answer): the chosen options replace the previous answer. */
  vote(id: string, optionIds: string[]): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/announcements/${id}/poll/vote`, { optionIds }));
  }

  withdrawVote(id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API}/announcements/${id}/poll/vote`));
  }

  // --- Comments (members and board) ---

  listComments(id: string, query: ListQuery): Promise<Page<AnnouncementComment>> {
    return firstValueFrom(this.http.get<Page<AnnouncementComment>>(`${API}/announcements/${id}/comments`, { params: listParams(query) }));
  }

  /** Adds a comment, or an answer when `parentId` is the comment to answer. */
  addComment(id: string, text: string, parentId?: string): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(`${API}/announcements/${id}/comments`, { text, ...(parentId ? { parentId } : {}) }));
  }

  /** A member deletes their own comment; the board deletes any comment. */
  deleteComment(id: string, commentId: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API}/announcements/${id}/comments/${commentId}`));
  }

  // --- Board ---

  /** Returns the id of the new announcement (a draft). */
  create(input: AnnouncementInput): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(`${API}/admin/announcements`, input));
  }

  /** Returns the id of the new poll (a draft). */
  createPoll(input: PollAnnouncementInput): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(`${API}/admin/announcements/polls`, input));
  }

  update(id: string, input: AnnouncementInput): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/admin/announcements/${id}`, input));
  }

  remove(id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API}/admin/announcements/${id}`));
  }

  publish(id: string): Promise<void> {
    return this.post(id, 'publish');
  }

  unpublish(id: string): Promise<void> {
    return this.post(id, 'unpublish');
  }

  pin(id: string): Promise<void> {
    return this.post(id, 'pin');
  }

  unpin(id: string): Promise<void> {
    return this.post(id, 'unpin');
  }

  setPoster(id: string, file: File): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/admin/announcements/${id}/poster`, this.form(file)));
  }

  removePoster(id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API}/admin/announcements/${id}/poster`));
  }

  addAttachment(id: string, file: File): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(`${API}/admin/announcements/${id}/attachments`, this.form(file)));
  }

  removeAttachment(id: string, fileId: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API}/admin/announcements/${id}/attachments/${fileId}`));
  }

  changePollDeadline(id: string, closesOn: string): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/admin/announcements/${id}/poll/deadline`, { closesOn }));
  }

  private post(id: string, action: string): Promise<void> {
    return firstValueFrom(this.http.post<void>(`${API}/admin/announcements/${id}/${action}`, {}));
  }

  private form(file: File): FormData {
    const form = new FormData();
    form.append('file', file, file.name);
    return form;
  }
}
