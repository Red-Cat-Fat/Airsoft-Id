import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const token = authService.getToken();

  const isAuthEndpoint = req.url.includes('/auth/login')
    || req.url.includes('/auth/register')
    || req.url.includes('/auth/refresh-token');

  if (token && !isAuthEndpoint) {
    req = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status !== 401) {
        return throwError(() => error);
      }

      const hasRefreshToken = !!authService.getRefreshToken();
      const isRefreshRequest = req.url.includes('/auth/refresh-token');

      if (hasRefreshToken && !isAuthEndpoint && !isRefreshRequest) {
        return authService.refreshToken().pipe(
          switchMap(() => {
            const refreshedToken = authService.getToken();
            if (!refreshedToken) {
              return throwError(() => error);
            }

            const retriedRequest = req.clone({
              setHeaders: {
                Authorization: `Bearer ${refreshedToken}`
              }
            });

            return next(retriedRequest);
          }),
          catchError((refreshError) => {
            authService.logout('/unauthorized', false);
            return throwError(() => refreshError);
          })
        );
      }

      authService.logout('/unauthorized', false);
      return throwError(() => error);
    })
  );
};
