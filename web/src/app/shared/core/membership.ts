/** "valid", "grace" (start of the season: previous season accepted) or "to_validate". */
export type Membership = 'valid' | 'grace' | 'to_validate';

/** "internal" = licensed by the club, "external" = member of the club but licensed in another club. */
export type MembershipKind = 'internal' | 'external';

export const MEMBERSHIP_KINDS: { value: MembershipKind; label: string }[] = [
  { value: 'internal', label: 'Licencié du club' },
  { value: 'external', label: 'Licencié dans un autre club' },
];

export function kindLabel(kind: MembershipKind): string {
  return MEMBERSHIP_KINDS.find((k) => k.value === kind)?.label ?? kind;
}
