/** One day of the weekly schedule of the club (mirror of the API's `ClubSessionDay`). */
export interface ClubSessionDay {
  /** monday, tuesday… as the API writes it. */
  day: string;
  /** "20:00:00". */
  startTime: string;
  endTime: string;
}

/** A session to come (mirror of `ClubSessionToCome`). */
export interface ClubSessionToCome {
  /** yyyy-MM-dd. */
  date: string;
  startTime: string;
  endTime: string;
  isToday: boolean;
}

/** The school holidays the club is in right now (mirror of `ClubClosure`). */
export interface ClubClosure {
  name: string;
  /** Last day without session (included). */
  endsOn: string;
}

/** When the club plays and what happens today (mirror of `ListClubSessionsResponse`). */
export interface ClubSessions {
  today: string;
  weekly: ClubSessionDay[];
  isOpenToday: boolean;
  closure: ClubClosure | null;
  next: ClubSessionToCome[];
}

const DAYS: Record<string, string> = {
  monday: 'Lundi',
  tuesday: 'Mardi',
  wednesday: 'Mercredi',
  thursday: 'Jeudi',
  friday: 'Vendredi',
  saturday: 'Samedi',
  sunday: 'Dimanche',
};

/** "tuesday" → "Mardi". */
export function dayLabel(day: string): string {
  return DAYS[day] ?? day;
}

/** "20:00:00" → "20 h", "20:30:00" → "20 h 30". */
export function hourLabel(time: string): string {
  const minutes = time.slice(3, 5);
  return minutes === '00' ? `${time.slice(0, 2)} h` : `${time.slice(0, 2)} h ${minutes}`;
}
