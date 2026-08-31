import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ShortUrl } from '../models/short-url.model';

@Injectable({ providedIn: 'root' })
export class ShortUrlService {
  private readonly baseUrl = `${environment.apiUrl}/ShortUrls`;

  constructor(private http: HttpClient) { }

  getAll(): Observable<ShortUrl[]> {
    return this.http.get<ShortUrl[]>(this.baseUrl);
  }

  create(originalUrl: string): Observable<ShortUrl> {
    return this.http.post<ShortUrl>(this.baseUrl, { originalUrl });
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  getById(id: number): Observable<ShortUrl> {
    return this.http.get<ShortUrl>(`${this.baseUrl}/${id}`);
  }
}
