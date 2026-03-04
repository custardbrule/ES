import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { toSignal } from '@angular/core/rxjs-interop';
import { ReactiveFormsModule } from '@angular/forms';
import { KeyValuePipe } from '@angular/common';
import { catchError, map, of, startWith, Subject, switchMap } from 'rxjs';
import { HttpErrorResponse } from '@angular/common/http';
import { ROUTE_DEF as navigator } from '@src/shared/constants/route-const';
import { DiaryService } from '@src/app/services/diary.service';
import { FormBuilderService } from '@src/app/services/form-builder.service';
import { AuthService } from '@src/app/services/auth.service';
import { ModalComponent } from '@src/app/components/modal/modal.component';
import {
  EDiaryVisibility,
  CreateDiaryRequest,
} from '@src/app/models/diary.models';
import * as rules from '@src/shared/utils/validator-rules';

interface CreateDiaryForm {
  name: string;
  description: string;
  diaryVisibility: EDiaryVisibility;
}

@Component({
  selector: 'app-home-page',
  imports: [RouterLink, ModalComponent, ReactiveFormsModule, KeyValuePipe],
  templateUrl: './home-page.component.html',
  styleUrl: './home-page.component.scss',
  host: { ngSkipHydration: 'true' },
})
export class HomePageComponent {
  private diaryService = inject(DiaryService);
  private formService = inject(FormBuilderService);
  private authService = inject(AuthService);
  navigator = navigator;
  EDiaryVisibility = EDiaryVisibility;

  private refresh$ = new Subject<void>();

  diaries = toSignal(
    this.refresh$.pipe(
      startWith(undefined),
      switchMap(() =>
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
      ),
    ),
  );

  diaryForm = this.formService.create<CreateDiaryForm>((b) => {
    b.for('name', '').add(rules.required, 'Name is required');
    b.for('description', '');
    b.for('diaryVisibility', EDiaryVisibility.Public);
  });

  onSubmit(modal: ModalComponent): void {
    if (this.diaryForm.invalid) return;

    const value: CreateDiaryRequest = {
      ...(this.diaryForm.value as Omit<CreateDiaryRequest, 'authorId'>),
      authorName: this.authService.getUserInfo()?.name,
      authorId: this.authService.getUserInfo()?.sub,
    };
    this.diaryService.addDiary(value).subscribe({
      next: () => {
        this.diaryForm.reset({
          name: '',
          description: '',
          diaryVisibility: EDiaryVisibility.Public,
        });
        modal.close();
        this.refresh$.next();
      },
      error: (err: HttpErrorResponse) => {
        console.error('Failed to add diary:', err);
      },
    });
  }
}
