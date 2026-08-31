import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { ShortUrlService } from '../../services/short-url';
import { ShortUrl } from '../../models/short-url.model';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-short-url-info',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './short-url-info.html',
  styleUrl: './short-url-info.css'
})
export class ShortUrlInfo implements OnInit {
  url = signal<ShortUrl | null>(null);
  isLoading = signal(true);
  errorMessage = signal('');

  readonly shortUrlOrigin = environment.apiUrl.replace('/api', '');

  constructor(
    private route: ActivatedRoute,
    private shortUrlService: ShortUrlService
  ) { }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    this.shortUrlService.getById(id).subscribe({
      next: (data) => {
        this.url.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.errorMessage.set('URL not found.');
        this.isLoading.set(false);
      }
    });
  }
}
