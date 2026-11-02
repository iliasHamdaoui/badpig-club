/** Who can read an announcement: `internal` = club members only, `public` = any visitor of the site. */
export type AnnouncementKind = 'internal' | 'public';

/** What an announcement is: a written text, or a standalone poll (its title is the question). */
export type AnnouncementForm = 'text' | 'poll';

/** One announcement of a list (mirror of the API's list item). */
export interface AnnouncementItem {
  id: string;
  title: string;
  summary: string;
  kind: AnnouncementKind;
  form: AnnouncementForm;
  isPublished: boolean;
  isPinned: boolean;
  publishedAt: string | null;
  posterId: string | null;
  attachmentCount: number;
  /** Last day shown (yyyy-MM-dd). */
  expiresOn: string;
  isExpired: boolean;
  /** Null for a visitor (likes are for the members). */
  likeCount: number | null;
  likedByMe: boolean;
  /** Only filled for the board. */
  authorName: string | null;
}

export interface AnnouncementFile {
  id: string;
  fileName: string;
  contentType: string;
  sizeBytes: number;
  isPoster: boolean;
}

/** One announcement with its whole content. */
export interface Announcement {
  id: string;
  title: string;
  bodyHtml: string;
  kind: AnnouncementKind;
  form: AnnouncementForm;
  isPublished: boolean;
  isPinned: boolean;
  publishedAt: string | null;
  updatedAt: string;
  files: AnnouncementFile[];
  expiresOn: string;
  isExpired: boolean;
  /** Members can comment (the comments are hidden from the site when false). */
  commentsEnabled: boolean;
  /** Filled when the form is `poll`. */
  poll: Poll | null;
  likeCount: number | null;
  likedByMe: boolean;
  authorName: string | null;
}

/** What the board writes (creation and update). */
export interface AnnouncementInput {
  title: string;
  bodyHtml: string;
  kind: AnnouncementKind;
  /** yyyy-MM-dd; left out = 90 days from today on creation, unchanged on update. */
  expiresOn?: string;
  /** Members can comment; left out = on at creation, unchanged on update. */
  commentsEnabled?: boolean;
}

/** One choice of a poll. */
export interface PollOption {
  id: string;
  label: string;
  /** Only once the poll is closed, for club members. */
  voteCount: number | null;
  /** Names; only once the poll is closed, for the board, when the poll is not anonymous. */
  voters: string[] | null;
}

/** The poll of an announcement, as the reader may see it. */
export interface Poll {
  allowMultiple: boolean;
  isAnonymous: boolean;
  /** Last day the answers can be given or changed (yyyy-MM-dd). */
  closesOn: string;
  isClosed: boolean;
  /** The counts are filled: members see them once closed, the board at any time. */
  resultsVisible: boolean;
  /** The signed-in club member can answer or change the answer now. */
  canVote: boolean;
  /** Null for a visitor. */
  participantCount: number | null;
  myOptionIds: string[];
  options: PollOption[];
}

/** What the board writes to create a poll (an announcement of its own; the text is an optional explanation). */
export interface PollAnnouncementInput {
  title: string;
  bodyHtml: string;
  kind: AnnouncementKind;
  options: string[];
  allowMultiple: boolean;
  isAnonymous: boolean;
  closesOn: string;
  /** yyyy-MM-dd; left out = 90 days from today (or the closing date if later). */
  expiresOn?: string;
}

/** The comment an answer quotes (chat style). */
export interface CommentQuote {
  id: string;
  /** Null once the quoted comment is deleted. */
  authorName: string | null;
  excerpt: string;
  isDeleted: boolean;
}

/** A comment of the conversation under an announcement (members and board only). */
export interface AnnouncementComment {
  id: string;
  authorName: string;
  text: string;
  createdAt: string;
  isMine: boolean;
  /** Its author, or a board member. */
  canDelete: boolean;
  /** The comment this one answers; null for a message of its own. */
  replyTo: CommentQuote | null;
}

export const KIND_LABELS: Record<AnnouncementKind, string> = {
  internal: 'Interne (adhérents)',
  public: 'Publique (tout visiteur)',
};
