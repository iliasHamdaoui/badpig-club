import { Membership } from '../../../shared/core/membership';

/** Own profile (GET /api/v1/me). */
export interface Profile {
  id: string;
  username: string;
  name: string;
  email: string;
  groups: string[];
  membership: Membership;
  /** True if the members area is accessible (board and admins: always). */
  membershipUpToDate: boolean;
  /** Label of the title within the board (Président(e)...). */
  title: string | null;
  /** Federation licence number (FFBaD) on record, or null. */
  licence: string | null;
  /** Day the person joined the club (their first validated membership), or null. */
  memberSince: string | null;
  /** Version of the profile picture, or null when there is none. */
  photoVersion: number | null;
}
