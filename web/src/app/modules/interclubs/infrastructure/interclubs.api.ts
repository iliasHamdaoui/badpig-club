import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { API } from '../../../shared/infrastructure/api';
import { ListQuery, Page, listParams } from '../../../shared/core/page';
import { DateAnswer, Fixture, FixtureForm, FixtureItem, InterclubSeason, LineupForm, MatchEntry, Proposal, ProposalForm, ResultForm, Team, TeamCandidate, TeamMember, fixtureBody, lineupBody, matchBody, proposalBody, resultBody } from '../core/interclubs';

@Injectable({ providedIn: 'root' })
export class InterclubsApi {
  private readonly http = inject(HttpClient);

  /** The seasons the interclubs can be filtered on (those with teams, plus the current one). */
  seasons(query: ListQuery): Promise<Page<InterclubSeason>> {
    return firstValueFrom(this.http.get<Page<InterclubSeason>>(`${API}/interclubs/seasons`, { params: listParams(query) }));
  }

  teams(query: ListQuery & { seasonId?: string }): Promise<Page<Team>> {
    return firstValueFrom(this.http.get<Page<Team>>(`${API}/interclubs/teams`, { params: listParams(query) }));
  }

  members(teamId: string, query: ListQuery): Promise<Page<TeamMember>> {
    return firstValueFrom(this.http.get<Page<TeamMember>>(`${API}/interclubs/teams/${teamId}/members`, { params: listParams(query) }));
  }

  /** The active members who are not in the team yet (captain of the team, or board). */
  candidates(teamId: string, query: ListQuery): Promise<Page<TeamCandidate>> {
    return firstValueFrom(this.http.get<Page<TeamCandidate>>(`${API}/interclubs/teams/${teamId}/candidates`, { params: listParams(query) }));
  }

  addMember(teamId: string, accountId: string): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/interclubs/teams/${teamId}/members/${accountId}`, {}));
  }

  removeMember(teamId: string, accountId: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API}/interclubs/teams/${teamId}/members/${accountId}`));
  }

  fixtures(query: ListQuery & { teamId?: string; seasonId?: string; when?: string }): Promise<Page<FixtureItem>> {
    return firstValueFrom(this.http.get<Page<FixtureItem>>(`${API}/interclubs/fixtures`, { params: listParams(query) }));
  }

  fixture(id: string): Promise<Fixture> {
    return firstValueFrom(this.http.get<Fixture>(`${API}/interclubs/fixtures/${id}`));
  }

  /** The captain of the team (or the board) adds a fixture. Returns its identifier. */
  createFixture(teamId: string, form: FixtureForm): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(`${API}/interclubs/teams/${teamId}/fixtures`, fixtureBody(form)));
  }

  updateFixture(id: string, form: FixtureForm): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/interclubs/fixtures/${id}`, fixtureBody(form)));
  }

  setLineup(fixtureId: string, form: LineupForm): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/interclubs/fixtures/${fixtureId}/lineup`, lineupBody(form)));
  }

  setResult(fixtureId: string, form: ResultForm): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/interclubs/fixtures/${fixtureId}/result`, resultBody(form)));
  }

  /** One match at a time, as the matches finish; the other matches are left untouched. */
  setMatchResult(fixtureId: string, slot: string, match: MatchEntry): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/interclubs/fixtures/${fixtureId}/matches/${slot}/result`, matchBody(match)));
  }

  deleteFixture(id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API}/interclubs/fixtures/${id}`));
  }

  proposals(query: ListQuery & { teamId?: string; status?: string }): Promise<Page<Proposal>> {
    return firstValueFrom(this.http.get<Page<Proposal>>(`${API}/interclubs/proposals`, { params: listParams(query) }));
  }

  proposal(id: string): Promise<Proposal> {
    return firstValueFrom(this.http.get<Proposal>(`${API}/interclubs/proposals/${id}`));
  }

  /** The captain of the team (or the board) offers two dates or more for the next fixture. Returns the proposal identifier. */
  createProposal(teamId: string, form: ProposalForm): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(`${API}/interclubs/teams/${teamId}/proposals`, proposalBody(form)));
  }

  /** The signed-in person answers one date; a new answer replaces the previous one. */
  answer(proposalId: string, dateId: string, answer: DateAnswer): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/interclubs/proposals/${proposalId}/answers`, { answers: [{ dateId, answer }] }));
  }

  /** Chooses a date: the fixture is created. Returns the fixture identifier. */
  choose(proposalId: string, dateId: string): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(`${API}/interclubs/proposals/${proposalId}/choose`, { dateId }));
  }

  deleteProposal(id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API}/interclubs/proposals/${id}`));
  }

  // --- Board ---

  createTeam(name: string, seasonId: string): Promise<{ id: string }> {
    return firstValueFrom(this.http.post<{ id: string }>(`${API}/admin/interclubs/teams`, { name, seasonId }));
  }

  updateTeam(id: string, name: string, seasonId: string): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/admin/interclubs/teams/${id}`, { name, seasonId }));
  }

  deleteTeam(id: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API}/admin/interclubs/teams/${id}`));
  }

  assignCaptain(teamId: string, captainId: string): Promise<void> {
    return firstValueFrom(this.http.put<void>(`${API}/admin/interclubs/teams/${teamId}/captain`, { captainId }));
  }

  removeCaptain(teamId: string): Promise<void> {
    return firstValueFrom(this.http.delete<void>(`${API}/admin/interclubs/teams/${teamId}/captain`));
  }
}
