import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '@src/app/services/auth.service';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      switch (error.status) {
        case 401:
          if (authService.isLoggedIn) authService.logout();
          break;
        // case 403:
        //   router.navigate(['/forbidden']);
        //   break;
        // case 500:
        // case 503:
        //   router.navigate(['/error']);
        //   break;
      }

      return throwError(() => error);
    }),
  );
};
