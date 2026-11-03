/** One person of the club directory (GET /api/v1/club/directory). */
export interface DirectoryPerson {
  id: string;
  name: string;
  /** Version of the picture, which is part of its address. */
  photoVersion: number;
}
