/** Where a fixture is played, from the point of view of the club. */
export type Venue = 'home' | 'away';

export const VENUE_LABELS: Record<Venue, string> = { home: 'À domicile', away: "À l'extérieur" };

/** A team of the club in the interclub competition (mirror of the API's `ListTeamsItem`). */
export interface Team {
  id: string;
  name: string;
  /** Identifier of the season the team plays in. */
  seasonId: string;
  /** Name of that season, e.g. "2026-2027". */
  season: string;
  captainId: string | null;
  captainName: string | null;
  /** True when the signed-in person is the captain of the team. */
  isMyTeam: boolean;
  memberCount: number;
  /** True when the signed-in person is a player of the team. */
  isPlayer: boolean;
}

/** A season the interclubs can be filtered on (mirror of `ListInterclubSeasonsItem`). */
export interface InterclubSeason {
  id: string;
  /** Name of the season, e.g. "2026-2027". */
  name: string;
  /** True for the season of the club today: the one selected by default. */
  isCurrent: boolean;
  teamCount: number;
}

/** A player of a team (mirror of `ListTeamMembersItem`). */
export interface TeamMember {
  accountId: string;
  name: string;
  isCaptain: boolean;
}

/** A person the captain can add to the team (mirror of `ListTeamCandidatesItem`). */
export interface TeamCandidate {
  accountId: string;
  name: string;
}

/** A fixture in the calendar (mirror of `ListFixturesItem`). Dates are yyyy-MM-dd, times HH:mm:ss. */
export interface FixtureItem {
  id: string;
  teamId: string;
  teamName: string;
  date: string;
  startTime: string | null;
  opponent: string;
  venue: Venue;
  place: string | null;
  /** Matches won by each side, final score; null until the seven matches are decided. */
  clubScore: number | null;
  opponentScore: number | null;
  /** How many of the seven matches have a result (0 to 7). */
  decidedMatches: number;
  /** Matches won so far by each side, while the result is being entered. */
  clubMatches: number;
  opponentMatches: number;
  /** True when the fixture is being played right now (computed by the API). */
  isLive: boolean;
}

/** One fixture with its team and captain (mirror of `GetFixtureResponse`). */
export interface Fixture {
  id: string;
  teamId: string;
  teamName: string;
  season: string;
  captainId: string | null;
  captainName: string | null;
  date: string;
  startTime: string | null;
  opponent: string;
  venue: Venue;
  place: string | null;
  clubScore: number | null;
  opponentScore: number | null;
  /** The seven matches in the usual order. */
  matches: FixtureMatch[];
  /** True when the signed-in person can change or delete the fixture (captain of the team, or board). */
  canManage: boolean;
}

/** What the captain types to define a fixture. */
export interface FixtureForm {
  date: string;
  /** HH:mm, empty when not known. */
  startTime: string;
  opponent: string;
  venue: Venue;
  place: string;
}

export const EMPTY_FIXTURE_FORM: FixtureForm = { date: '', startTime: '', opponent: '', venue: 'home', place: '' };

/** The JSON body of the creation or the change of a fixture. */
export function fixtureBody(form: FixtureForm) {
  return {
    date: form.date,
    startTime: form.startTime || null,
    opponent: form.opponent.trim(),
    venue: form.venue,
    place: form.place.trim() || null,
  };
}

/** The form filled with the values of an existing fixture. */
export function formOf(fixture: Fixture): FixtureForm {
  return {
    date: fixture.date,
    startTime: fixture.startTime?.slice(0, 5) ?? '',
    opponent: fixture.opponent,
    venue: fixture.venue,
    place: fixture.place ?? '',
  };
}

/** "14:00:00" → "14 h 00"; empty when the time is not known. */
export function timeLabel(time: string | null): string {
  return time ? `${time.slice(0, 2)} h ${time.slice(3, 5)}` : '';
}

/** What a person can answer to a proposed date. */
export type DateAnswer = 'yes' | 'maybe' | 'no';

export const ANSWER_LABELS: Record<DateAnswer, string> = { yes: 'Disponible', maybe: 'Peut-être', no: 'Indisponible' };

/** The answer of one person to a date (mirror of `GetDateProposalAnswer`). */
export interface ProposalAnswer {
  accountId: string;
  name: string;
  answer: DateAnswer;
}

/** A date offered in a proposal, with the count of the answers (`answers` is only filled for the captain and the board). */
export interface ProposedDate {
  id: string;
  date: string;
  startTime: string | null;
  yes: number;
  maybe: number;
  no: number;
  myAnswer: DateAnswer | null;
  answers?: ProposalAnswer[];
}

/** Dates offered by a captain for the next fixture (mirror of `ListDateProposalsItem` and `GetDateProposalResponse`). */
export interface Proposal {
  id: string;
  teamId: string;
  teamName: string;
  opponent: string;
  venue: Venue;
  place: string | null;
  /** True while the captain has not chosen a date. */
  isOpen: boolean;
  /** The fixture created from the chosen date. */
  fixtureId: string | null;
  /** True when the signed-in person is a player of the team and the proposal is still open. */
  canAnswer: boolean;
  canManage: boolean;
  dates: ProposedDate[];
}

/** What the captain types to propose dates. */
export interface ProposalForm {
  opponent: string;
  venue: Venue;
  place: string;
  dates: { date: string; startTime: string }[];
}

export function emptyProposalForm(): ProposalForm {
  return { opponent: '', venue: 'home', place: '', dates: [{ date: '', startTime: '' }, { date: '', startTime: '' }] };
}

/** The JSON body of the creation of a proposal. */
export function proposalBody(form: ProposalForm) {
  return {
    opponent: form.opponent.trim(),
    venue: form.venue,
    place: form.place.trim() || null,
    dates: form.dates.map((d) => ({ date: d.date, startTime: d.startTime || null })),
  };
}

/** The seven matches of a fixture, in the usual order, with the number of players of each. */
export const MATCH_SLOTS = [
  { code: 'SD', label: 'Simple dame', players: 1 },
  { code: 'MX1', label: 'Mixte 1', players: 2 },
  { code: 'MX2', label: 'Mixte 2', players: 2 },
  { code: 'DD', label: 'Double dame', players: 2 },
  { code: 'DH', label: 'Double homme', players: 2 },
  { code: 'SH1', label: 'Simple homme 1', players: 1 },
  { code: 'SH2', label: 'Simple homme 2', players: 1 },
] as const;

export type MatchOutcome = 'won' | 'lost';

export const OUTCOME_LABELS: Record<MatchOutcome, string> = { won: 'Gagné', lost: 'Perdu' };

export interface MatchPlayer {
  accountId: string;
  name: string;
}

/** The points of one set: those of the club, then those of the opponent. */
export interface SetScore {
  club: number;
  opponent: number;
}

/** One of the seven matches of a fixture (mirror of `GetFixtureMatch`). */
export interface FixtureMatch {
  slot: string;
  players: MatchPlayer[];
  /** The sets played, in order; empty until the result is entered. */
  sets: SetScore[];
  /** How the match ended; null until the result is entered. */
  ending: MatchEnding | null;
  /** The side that gave up (forfeit, retirement); null otherwise. */
  withdrawn: MatchSide | null;
  /** Computed by the API: from the sets (2 sets win the match) or from the side that gave up; null until the match is decided. */
  outcome: MatchOutcome | null;
}

/** "15-7 15-14": the sets of a match, written for the people. */
export function setsText(sets: SetScore[]): string {
  return sets.map((s) => `${s.club}-${s.opponent}`).join(' ');
}

/** What the captain chooses for one match: the players (account identifiers, empty = not chosen yet). */
export type LineupForm = Record<string, string[]>;

/** The lineup as it is now, ready to be edited. */
export function lineupFormOf(matches: FixtureMatch[]): LineupForm {
  const form: LineupForm = {};
  for (const slot of MATCH_SLOTS) {
    const players = matches.find((m) => m.slot === slot.code)?.players.map((p) => p.accountId) ?? [];
    form[slot.code] = Array.from({ length: slot.players }, (_, i) => players[i] ?? '');
  }
  return form;
}

/** True when each match has either all its players or none, and nobody plays twice the same match. */
export function lineupIsValid(form: LineupForm): boolean {
  return MATCH_SLOTS.every((slot) => {
    const chosen = form[slot.code].filter((id) => id !== '');
    return (chosen.length === 0 || chosen.length === slot.players) && new Set(chosen).size === chosen.length;
  });
}

/** The JSON body of the lineup: only the matches that have players. */
export function lineupBody(form: LineupForm) {
  return {
    slots: MATCH_SLOTS.filter((slot) => form[slot.code].some((id) => id !== '')).map((slot) => ({ slot: slot.code, playerIds: form[slot.code] })),
  };
}

/**
 * A set is won at 15 points with a lead of 2; from 14-14 play goes on until a lead of 2, or until 21. A match has 2 or 3 sets and is won
 * with 2 sets. A match can also end by forfeit or retirement (same rules as the API).
 */
export const POINTS_TO_WIN_SET = 15;
export const MAX_POINTS_IN_SET = 21;
export const SETS_TO_WIN_MATCH = 2;
export const MAX_SETS = 3;

export type MatchEnding = 'played' | 'forfeit' | 'retirement';
export type MatchSide = 'club' | 'opponent';

export const ENDING_LABELS: Record<MatchEnding, string> = { played: 'Joué', forfeit: 'Forfait', retirement: 'Abandon' };
export const SIDE_LABELS: Record<MatchSide, string> = { club: 'Notre joueur', opponent: "L'adversaire" };

/**
 * What the captain types for one set: the points of each side, null when the field is empty (the set was not played).
 * These are the values of number fields, which Angular gives as numbers, never as text.
 */
export interface SetEntry {
  club: number | null;
  opponent: number | null;
}

/** What the captain types for one match: how it ended, who gave up (forfeit, retirement) and the three sets. */
export interface MatchEntry {
  ending: MatchEnding;
  withdrawn: MatchSide;
  sets: SetEntry[];
}

/** The seven matches, as typed. */
export type ResultForm = Record<string, MatchEntry>;

export function resultFormOf(matches: FixtureMatch[]): ResultForm {
  const form: ResultForm = {};
  for (const slot of MATCH_SLOTS) {
    const match = matches.find((m) => m.slot === slot.code);
    const sets = match?.sets ?? [];
    form[slot.code] = {
      ending: match?.ending ?? 'played',
      withdrawn: match?.withdrawn ?? 'club',
      sets: Array.from({ length: MAX_SETS }, (_, i) => ({ club: sets[i]?.club ?? null, opponent: sets[i]?.opponent ?? null })),
    };
  }
  return form;
}

/** The sets typed for a match, or null when an entry is half filled, not a whole number, or a blank entry comes before a filled one. */
export function parseSets(entries: SetEntry[]): SetScore[] | null {
  const sets: SetScore[] = [];
  let ended = false;
  for (const entry of entries) {
    const blank = entry.club === null && entry.opponent === null;
    if (blank) {
      ended = true;
      continue;
    }
    if (ended || !Number.isInteger(entry.club) || !Number.isInteger(entry.opponent)) {
      return null;
    }
    sets.push({ club: entry.club!, opponent: entry.opponent! });
  }
  return sets;
}

/** A finished set: 15 against 13 or fewer; from 14-14 a lead of 2 (16-14 … 21-19); or 21-20. */
export function isValidSet(set: SetScore): boolean {
  const winner = Math.max(set.club, set.opponent);
  const loser = Math.min(set.club, set.opponent);
  if (loser < 0 || winner === loser) {
    return false;
  }
  const atFifteen = winner === POINTS_TO_WIN_SET && loser <= POINTS_TO_WIN_SET - 2;
  const onTheLead = winner > POINTS_TO_WIN_SET && winner < MAX_POINTS_IN_SET && loser === winner - 2;
  const atTheCap = winner === MAX_POINTS_IN_SET && loser >= MAX_POINTS_IN_SET - 2 && loser < MAX_POINTS_IN_SET;
  return atFifteen || onTheLead || atTheCap;
}

/** won or lost from the sets (the higher score wins a set), or null while no side has won 2 sets. */
export function outcomeOfSets(sets: SetScore[]): MatchOutcome | null {
  const club = sets.filter((s) => s.club > s.opponent).length;
  const opponent = sets.filter((s) => s.opponent > s.club).length;
  return club >= SETS_TO_WIN_MATCH ? 'won' : opponent >= SETS_TO_WIN_MATCH ? 'lost' : null;
}

/** The sets typed for a match, when they make a finished match (the match stops as soon as a side has 2 sets). */
export function finishedMatch(entries: SetEntry[]): SetScore[] | null {
  const sets = parseSets(entries);
  if (!sets || sets.length < SETS_TO_WIN_MATCH || !sets.every(isValidSet)) {
    return null;
  }
  return outcomeOfSets(sets) !== null && outcomeOfSets(sets.slice(0, -1)) === null ? sets : null;
}

/** The sets of a match as sent to the API, or null when the entry is not right: finished sets, none for a forfeit, the sets so far for a retirement. */
export function matchSets(match: MatchEntry): SetScore[] | null {
  if (match.ending === 'played') {
    return finishedMatch(match.sets);
  }
  const sets = parseSets(match.sets);
  if (match.ending === 'forfeit') {
    return sets?.length === 0 ? sets : null;
  }
  return sets && sets.every(isValidSet) && outcomeOfSets(sets) === null ? sets : null;
}

/** The outcome of a typed match (won or lost for the club), or null when it is not right yet. */
export function matchOutcome(match: MatchEntry): MatchOutcome | null {
  const sets = matchSets(match);
  if (sets === null) {
    return null;
  }
  if (match.ending === 'played') {
    return outcomeOfSets(sets);
  }
  return match.withdrawn === 'club' ? 'lost' : 'won';
}

export function resultIsComplete(form: ResultForm): boolean {
  return MATCH_SLOTS.every((slot) => matchOutcome(form[slot.code]) !== null);
}

/** The JSON body of one match: how it ended, who gave up, and its sets. */
export function matchBody(match: MatchEntry) {
  return { ending: match.ending, withdrawn: match.ending === 'played' ? null : match.withdrawn, sets: matchSets(match) ?? [] };
}

export function resultBody(form: ResultForm) {
  return { matches: MATCH_SLOTS.map((slot) => ({ slot: slot.code, ...matchBody(form[slot.code]) })) };
}
