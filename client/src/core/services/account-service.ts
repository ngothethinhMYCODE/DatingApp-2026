import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { tap } from 'rxjs/internal/operators/tap';
import { RegisterCreds, User } from '../../types/user';
import { environment } from '../../environments/environment';
import { LikesService } from './likes-service';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  private http = inject(HttpClient);
  private likesService=inject(LikesService);
  currentUser = signal<User | null>(null);
  private baseUrl = environment.apiUrl;
  register(creds: RegisterCreds) {
    return this.http.post<User>(this.baseUrl + 'account/register', creds).pipe(
      tap(user => {
        if (user) {
          this.setCurrentUser(user);
        }
      })
    )
  }
  login(creds: any) {
    return this.http.post<User>(this.baseUrl + 'account/login', creds).pipe(
      tap(user => {
        if (user) {
          this.setCurrentUser(user);
        }
      })
    )
  }
  setCurrentUser(user: User) {
    localStorage.setItem('user', JSON.stringify(user));
    this.currentUser.set(user);
    this.likesService.getLikeIds();
  }
  logout() {
    localStorage.removeItem('user');
    localStorage.removeItem('filter');
    this.likesService.clearLikeIds();
    this.currentUser.set(null);
  }
}
