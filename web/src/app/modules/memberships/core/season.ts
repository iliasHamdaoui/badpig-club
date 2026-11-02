/** One season of the list (mirror of the API's `ListSeasonsItem`). Dates are "YYYY-MM-DD". */
export interface Season {
  id: string;
  name: string;
  startsOn: string;
  endsOn: string;
  /** Days after the start during which the previous season's membership is still accepted. */
  graceDays: number;
  internalFee: number;
  externalFee: number;
  /** The season memberships are validated for. */
  isCurrent: boolean;
}

/** Values the board enters for a season (body of the creation and of the update). */
export interface SeasonInput {
  name: string | null;
  startsOn: string | null;
  endsOn: string | null;
  graceDays: number | null;
  internalFee: number | null;
  externalFee: number | null;
}

/** Memberships and fees of one season (mirror of the API's `ListSeasonStatisticsItem`). */
export interface SeasonStatistics {
  seasonId: string;
  name: string;
  startsOn: string;
  endsOn: string;
  internalCount: number;
  externalCount: number;
  totalCount: number;
  internalRevenue: number;
  externalRevenue: number;
  totalRevenue: number;
}

/** Default end of a season that starts on `startsOn`: one year later minus a day (1 September to 31 August). */
export function defaultEnd(startsOn: string): string {
  const d = new Date(`${startsOn}T00:00:00Z`);
  d.setUTCFullYear(d.getUTCFullYear() + 1);
  d.setUTCDate(d.getUTCDate() - 1);
  return d.toISOString().slice(0, 10);
}

/** The day after a date ("YYYY-MM-DD"). */
export function nextDay(date: string): string {
  const d = new Date(`${date}T00:00:00Z`);
  d.setUTCDate(d.getUTCDate() + 1);
  return d.toISOString().slice(0, 10);
}

/** Default name of a season, e.g. "2026-2027". */
export function defaultName(startsOn: string, endsOn: string): string {
  return `${startsOn.slice(0, 4)}-${endsOn.slice(0, 4)}`;
}

/** A period of school holidays of a season (mirror of `ListSchoolHolidaysItem`): the club is closed, no session. */
export interface SchoolHoliday {
  id: string;
  name: string;
  /** First day without session (included), "YYYY-MM-DD". */
  startsOn: string;
  /** Last day without session (included). */
  endsOn: string;
}

/** What the board enters for a period of school holidays. */
export interface SchoolHolidayInput {
  name: string;
  startsOn: string;
  endsOn: string;
}
