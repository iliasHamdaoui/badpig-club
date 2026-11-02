/** What waits in the club mailbox, as the API counts it (the messages themselves stay on the mail server). */
export interface MailboxStatus {
  /** False when no mailbox is configured or the mail server could not be reached: the site shows nothing. */
  isAvailable: boolean;
  unreadCount: number;
  webmailUrl: string | null;
  checkedAt: string | null;
}
