import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { SvgIconComponent } from '@src/app/components/svg-icon/svg-icon.component';
import { ModalComponent } from '@src/app/components/modal/modal.component';
import { CheckboxComponent } from '@src/app/components/checkbox/checkbox.component';
import { FormBuilderService } from '@src/app/services/form-builder.service';
import { DiaryService } from '@src/app/services/diary.service';
import { Diary } from '@src/app/models/diary.models';
import * as rules from '@src/shared/utils/validator-rules';

interface AddDiarySectionRequest {
  timeZoneId: string;
  detail: string;
  isPinned: boolean;
}

interface DiarySection {
  time: string;
  content: string;
  isPinned: boolean;
}

interface DiaryDay {
  date: string;
  isExpanded: boolean;
  isLoading: boolean;
  sections: DiarySection[];
}

interface DiaryState extends Diary {
  days: DiaryDay[];
}

@Component({
  selector: 'app-diary-page',
  imports: [SvgIconComponent, CommonModule, ModalComponent, ReactiveFormsModule, CheckboxComponent],
  templateUrl: './diary-page.component.html',
  styleUrl: './diary-page.component.scss',
})
export class DiaryPageComponent implements OnInit {
  isDescriptionExpanded = false;
  diary: DiaryState | null = null;

  sectionForm!: ReturnType<typeof this.formService.create<AddDiarySectionRequest>>;

  constructor(
    private formService: FormBuilderService,
    private diaryService: DiaryService,
    private route: ActivatedRoute,
    private router: Router,
  ) {
    const nav = this.router.getCurrentNavigation();
    const state = nav?.extras?.state?.['diary'] as Diary | null ?? null;
    if (state) this.diary = { ...state, days: [] };
  }

  ngOnInit() {
    this.sectionForm = this.formService.create<AddDiarySectionRequest>(b => {
      b.for('timeZoneId', Intl.DateTimeFormat().resolvedOptions().timeZone);
      b.for('detail', '').add(rules.required, 'Write something ?');
      b.for('isPinned', false);
    });
    if (!this.diary) this.loadDays();
  }

  loadDays() {
    const id = this.route.snapshot.paramMap.get('id')!;
    this.diaryService.getDiaryById(id).subscribe({
      next: (vm) => {
        this.diary = {
          ...vm,
          days: vm.days.map(d => ({
            date: d.date,
            isExpanded: false,
            isLoading: false,
            sections: d.sections.map(s => ({
              time: s.createdAt,
              content: s.detail,
              isPinned: s.isPinned,
            })),
          })),
        };
      },
      error: (err) => {
        console.error('Error loading diary:', err);
      },
    });
  }

  onSubmitSection(modal: ModalComponent) {
    if (this.sectionForm.invalid) return;
    console.log(this.sectionForm.value);
    // TODO: call API
    this.sectionForm.reset({ timeZoneId: Intl.DateTimeFormat().resolvedOptions().timeZone, detail: '', isPinned: false });
    modal.close();
  }

  toggleDay(day: DiaryDay) {
    day.isExpanded = !day.isExpanded;
  }
}
