import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Auth } from './auth';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(Auth);
  const token = auth.token();

  const isApiCall = req.url.startsWith(environment.apiUrl);
  const authenticatedReq =
    token && isApiCall
      ? req.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
      : req;

  return next(authenticatedReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && isApiCall && !req.url.includes('/auth/')) {
        auth.logout();
      }
      return throwError(() => error);
    }),
  );
};
