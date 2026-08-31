import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';
import { ShortUrlService } from '../../services/short-url';
import { AuthService } from '../../services/auth';
import { ShortUrl } from '../../models/short-url.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-short-urls-table',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './short-urls-table.html',
  styleUrl: './short-urls-table.css'
})
export class ShortUrlsTable implements OnInit {
  urls = signal<ShortUrl[]>([]);
  isLoading = signal(true);
  errorMessage = signal('');

  newUrl = '';
  addErrorMessage = signal('');
  isAdding = signal(false);

  deletingId = signal<number | null>(null);

  readonly shortUrlOrigin = environment.apiUrl.replace('/api', '');

  constructor(
    private shortUrlService: ShortUrlService,
    public authService: AuthService
  ) { }

  ngOnInit(): void {
    this.loadUrls();
  }

  loadUrls(): void {
    this.isLoading.set(true);

    this.shortUrlService.getAll().subscribe({
      next: (data) => {
        this.urls.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('Failed to load URLs.');
        this.isLoading.set(false);
      }
    });
  }

  onAddUrl(): void {
    if (!this.newUrl.trim()) {
      return;
    }

    this.addErrorMessage.set('');
    this.isAdding.set(true);

    this.shortUrlService.create(this.newUrl.trim()).subscribe({
      next: (created) => {
        this.urls.update((current) => [created, ...current]);
        this.newUrl = '';
        this.isAdding.set(false);
      },
      error: (err: HttpErrorResponse) => {
        this.isAdding.set(false);

        if (err.status === 409) {
          this.addErrorMessage.set('This URL already exists.');
        } else if (err.status === 400) {
          this.addErrorMessage.set('Please enter a valid URL.');
        } else {
          this.addErrorMessage.set('Failed to add URL.');
        }
      }
    });
  }

  onDelete(id: number): void {
    if (!confirm('Are you sure you want to delete this URL?')) {
      return;
    }

    this.deletingId.set(id);

    this.shortUrlService.delete(id).subscribe({
      next: () => {
        this.urls.update((current) =>
          current.filter((u) => u.id !== id)
        );

        this.deletingId.set(null);
      },
      error: () => {
        this.deletingId.set(null);
        this.errorMessage.set('Failed to delete URL.');
      }
    });
  }

  onLogout(): void {
    this.authService.logout();
    window.location.reload();
  }
}
