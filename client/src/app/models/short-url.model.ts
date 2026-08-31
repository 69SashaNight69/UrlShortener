export interface ShortUrl {
  id: number;
  originalUrl: string;
  shortCode: string;
  createdBy: string;
  createdDate: string;
  canDelete: boolean;
}
