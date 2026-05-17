import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AccountService } from '../services/account-service';

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {

  // bỏ qua refresh token endpoint
  if (req.url.includes('refresh-token')) {
    return next(req);
  }

  const accountService = inject(AccountService);
  const user = accountService.currentUser();

  if (user) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${user.token}`
      }
    });
  }

  return next(req);
};