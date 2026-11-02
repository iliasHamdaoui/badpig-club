/** Free-form title within the board (Président(e), Secrétaire…), with its holder if it has one. */
export interface TitleDef {
  id: string;
  label: string;
  holderId: string | null;
  holderName: string | null;
}

/** A person of the board, as the club members see them (mirror of `ListBoardMembersItem`). */
export interface BoardMember {
  accountId: string;
  name: string;
  email: string;
  /** Title at the board; null when the person holds none. */
  title: string | null;
}
