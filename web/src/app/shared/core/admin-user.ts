import { Membership, MembershipKind } from './membership';

/**
 * One row of the administration user list (GET /api/v1/admin/users).
 * The endpoint belongs to the Accounts module of the API, which composes the data of the other modules,
 * so the shape is a contract shared by the web modules (mirror of the API's `ListUsersItem`).
 */
export interface AdminUser {
  id: string;
  username: string;
  name: string;
  email: string;
  isActive: boolean;
  groups: string[];
  canManage: boolean;
  /** "pending" = invited, has not signed up yet. */
  status: 'active' | 'disabled' | 'pending';
  invitedByName: string | null;
  invitedAt: string | null;
  /** What the signed-in person can do on this account's Board group. */
  boardAction: 'add' | 'remove' | null;
  membership: Membership;
  /** Last validated season, e.g. "2026-2027". */
  membershipSeason: string | null;
  /** Kind of the latest membership; without one, the kind chosen at the invitation ("internal" by default). */
  membershipKind: MembershipKind;
  /** Fee recorded with the latest validated membership, in euros. */
  membershipAmount: number | null;
  membershipValidatedAt: string | null;
  membershipValidatedByName: string | null;
  canValidateMembership: boolean;
  /** Federation licence number (FFBaD) on record, or null. */
  licence: string | null;
  titleId: string | null;
  titleLabel: string | null;
  /** Disabled account: planned date of the automatic deletion. */
  automaticDeletionAt: string | null;
}
