import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { catchError, map, of, startWith } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ROUTE_DEF as navigator } from '@src/shared/constants/route-const';
import { DiaryService } from '@src/app/services/diary.service';
@Component({
  selector: 'app-home-page',
  imports: [RouterLink],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss',
  host: { ngSkipHydration: 'true' },
})
export class HomePageComponent {
  private diaryService = inject(DiaryService);
  navigator = navigator;

  diaries = toSignal(
    this.diaryService.getDiaries().pipe(
      map((data) => ({ status: 'success' as const, data })),
      catchError((err: HttpErrorResponse) =>
        of({
          status: 'error' as const,
          message:
            err.error?.message ?? err.message ?? 'Failed to load diaries',
        }),
      ),
      startWith({ status: 'loading' as const }),
    ),
  );
}
