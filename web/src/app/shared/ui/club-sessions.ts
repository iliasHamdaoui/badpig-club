import { Component, inject, input } from '@angular/core';
import { formatLongDate } from '../core/format';
import { ClubSessionToCome, dayLabel, hourLabel } from '../core/club-sessions';
import { ClubSessionsApi } from '../infrastructure/club-sessions.api';

/**
 * When the club plays: whether there is a session today, the next ones, and the weekly schedule.
 * The answer comes from the API (Paris date, school holidays taken out), for a visitor as well as for a member.
 */
@Component({
  selector: 'app-club-sessions',
  template: `
    @if (api.sessions(); as s) {
      <div class="stack stack-s">
        @if (s.isOpenToday && s.next.length > 0) {
          <p class="alert alert-success">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><path d="M5 12l5 5 9-10"></path></svg>
            <span><span class="alert-title">Séance ce soir.</span> De {{ hour(s.next[0].startTime) }} à {{ hour(s.next[0].endTime) }}.</span>
          </p>
        } @else {
          <p class="alert alert-info">
            <svg class="ic" viewBox="0 0 24 24" aria-hidden="true"><circle cx="12" cy="12" r="9"></circle><path d="M12 11v5M12 8h.01"></path></svg>
            <span>
              <span class="alert-title">Pas de séance aujourd'hui.</span>
              @if (s.closure) { {{ s.closure.name }}, jusqu'au {{ day(s.closure.endsOn) }}. }
            </span>
          </p>
        }

        @if (upcoming().length > 0) {
          <dl>
            @for (session of upcoming(); track session.date) {
              <div class="kv">
                <dt>{{ session.isToday ? "Aujourd'hui" : day(session.date) }}</dt>
                <dd>{{ hour(session.startTime) }} – {{ hour(session.endTime) }}</dd>
              </div>
            }
          </dl>
        } @else {
          <p class="small-text">Aucune séance d'ici la reprise.</p>
        }

        @if (showWeekly() && s.weekly.length > 0) {
          <p class="small-text">
            Habituellement {{ weekly() }}, de {{ hour(s.weekly[0].startTime) }} à {{ hour(s.weekly[0].endTime) }} ; fermé pendant les vacances scolaires.
          </p>
        }
      </div>
    } @else if (api.failed()) {
      <p class="small-text">Les horaires ne sont pas accessibles pour le moment.</p>
    } @else {
      <span class="skeleton" style="height: 12rem" aria-hidden="true"></span>
    }
  `,
  styles: `:host { display: block; }`,
})
export class ClubSessionsBlock {
  /** How many sessions to come are listed (the first one may be today's). */
  readonly count = input(3);
  /** Adds the usual weekly days under the list. */
  readonly showWeekly = input(true);
  protected readonly api = inject(ClubSessionsApi);
  protected readonly hour = hourLabel;

  constructor() {
    void this.api.load();
  }

  protected upcoming(): ClubSessionToCome[] {
    return (this.api.sessions()?.next ?? []).slice(0, this.count());
  }

  /** "les mardis, mercredis et vendredis". */
  protected weekly(): string {
    const days = (this.api.sessions()?.weekly ?? []).map((d) => `${dayLabel(d.day).toLowerCase()}s`);
    if (days.length === 0) {
      return '';
    }

    return days.length === 1 ? `les ${days[0]}` : `les ${days.slice(0, -1).join(', ')} et ${days.at(-1)}`;
  }

  /** "mardi 3 novembre" — the year is not useful for a date so close. */
  protected day(date: string): string {
    return formatLongDate(date).replace(/ \d{4}$/, '');
  }
}
