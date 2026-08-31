import { Routes } from '@angular/router';
import { Login } from './pages/login/login';
import { ShortUrlsTable } from './pages/short-urls-table/short-urls-table';
import { ShortUrlInfo } from './pages/short-url-info/short-url-info';
import { authGuard } from './guards/auth-guard';

export const routes: Routes = [
  { path: '', component: ShortUrlsTable },
  { path: 'login', component: Login },
  { path: 'urls/:id', component: ShortUrlInfo, canActivate: [authGuard] }
];
