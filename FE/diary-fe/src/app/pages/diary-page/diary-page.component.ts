import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { SvgIconComponent } from '@src/app/components/svg-icon/svg-icon.component';
import { ModalComponent } from '@src/app/components/modal/modal.component';
import { CheckboxComponent } from '@src/app/components/checkbox/checkbox.component';
import { FormBuilderService } from '@src/app/services/form-builder.service';
import { DiaryService } from '@src/app/services/diary.service';
import { AddSectionByDayRequest, AddSectionRequest, Diary } from '@src/app/models/diary.models';
import * as rules from '@src/shared/utils/validator-rules';

interface AddDiarySectionRequest {
  timeZoneId: string;
  detail: string;
  isPinned: boolean;
}

interface AddDiarySectionByDayRequest {
  date: string;
  eventTime: string;
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
  imports: [
    SvgIconComponent,
    CommonModule,
    ModalComponent,
    ReactiveFormsModule,
    CheckboxComponent,
  ],
  templateUrl: './diary-page.component.html',
  styleUrl: './diary-page.component.scss',
})
export class DiaryPageComponent implements OnInit {
  isDescriptionExpanded = false;
  diary: DiaryState | null = null;

  sectionForm!: ReturnType<
    typeof this.formService.create<AddDiarySectionRequest>
  >;

  sectionByDayForm!: ReturnType<
    typeof this.formService.create<AddDiarySectionByDayRequest>
  >;

  constructor(
    private formService: FormBuilderService,
    private diaryService: DiaryService,
    private route: ActivatedRoute,
    private router: Router,
  ) {
    const nav = this.router.getCurrentNavigation();
    const state = (nav?.extras?.state?.['diary'] as Diary | null) ?? null;
    if (state) this.diary = { ...state, days: [] };
  }

  ngOnInit() {
    this.sectionForm = this.formService.create<AddDiarySectionRequest>((b) => {
      b.for('timeZoneId', Intl.DateTimeFormat().resolvedOptions().timeZone);
      b.for('detail', '').add(rules.required, 'Write something ?');
      b.for('isPinned', false);
    });

    const d = new Date();
    const pad = (n: number) => n.toString().padStart(2, '0');
    const todayStr = `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}`;
    const timeStr = `${pad(d.getHours())}:${pad(d.getMinutes())}`;
    this.sectionByDayForm = this.formService.create<AddDiarySectionByDayRequest>((b) => {
      b.for('date', todayStr).add(rules.required, 'Date is required');
      b.for('eventTime', timeStr).add(rules.required, 'Time is required');
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
          days: vm.dailyDiaries.map((d) => ({
            date: d.date,
            isExpanded: false,
            isLoading: false,
            sections: d.sections.map((s) => ({
              time: s.eventTime,
              content: s.detail,
              isPinned: s.isPinned,
            })),
          })),
        };
        this.sortDaysDesc();
      },
      error: (err) => {
        console.error('Error loading diary:', err);
      },
    });
  }

  onSubmitSection(modal: ModalComponent) {
    if (this.sectionForm.invalid || !this.diary) return;

    const body: AddSectionRequest = {
      diaryId: this.diary.id,
      ...(this.sectionForm.value as Omit<AddSectionRequest, 'diaryId'>),
    };

    this.diaryService.addSection(body).subscribe({
      next: (vm) => {
        const sectionDate = new Date(vm.eventTime).toISOString().split('T')[0];
        const day = this.diary!.days.find((d) => d.date === sectionDate);
        if (day) {
          day.sections.unshift({ time: vm.eventTime, content: vm.detail, isPinned: vm.isPinned });
        } else {
          this.diary!.days.unshift({
            date: sectionDate,
            isExpanded: true,
            isLoading: false,
            sections: [{ time: vm.eventTime, content: vm.detail, isPinned: vm.isPinned }],
          });
        }
        this.sortDaysDesc();
        this.sectionForm.reset({
          timeZoneId: Intl.DateTimeFormat().resolvedOptions().timeZone,
          detail: '',
          isPinned: false,
        });
        modal.close();
      },
      error: (err) => {
        console.error('Failed to add section:', err);
      },
    });
  }

  onSubmitSectionByDay(modal: ModalComponent) {
    if (this.sectionByDayForm.invalid || !this.diary) return;

    const formVal = this.sectionByDayForm.value as AddDiarySectionByDayRequest;
    const body: AddSectionByDayRequest = {
      diaryId: this.diary.id,
      date: new Date(formVal.date).toISOString(),
      eventTime: new Date(`${formVal.date}T${formVal.eventTime}`).toISOString(),
      detail: formVal.detail,
      isPinned: formVal.isPinned,
    };

    this.diaryService.addSectionByDay(body).subscribe({
      next: (vm) => {
        const sectionDate = new Date(vm.eventTime).toISOString().split('T')[0];
        const day = this.diary!.days.find((d) => d.date === sectionDate);
        if (day) {
          day.sections.unshift({ time: vm.eventTime, content: vm.detail, isPinned: vm.isPinned });
        } else {
          this.diary!.days.unshift({
            date: sectionDate,
            isExpanded: true,
            isLoading: false,
            sections: [{ time: vm.eventTime, content: vm.detail, isPinned: vm.isPinned }],
          });
        }
        this.sortDaysDesc();
        const r = new Date();
        const rp = (n: number) => n.toString().padStart(2, '0');
        this.sectionByDayForm.reset({
          date: `${r.getFullYear()}-${rp(r.getMonth() + 1)}-${rp(r.getDate())}`,
          eventTime: `${rp(r.getHours())}:${rp(r.getMinutes())}`,
          detail: '',
          isPinned: false,
        });
        modal.close();
      },
      error: (err) => {
        console.error('Failed to add section by day:', err);
      },
    });
  }

  private sortDaysDesc() {
    this.diary!.days.sort((a, b) => b.date.localeCompare(a.date));
    this.diary!.days.forEach((day) =>
      day.sections.sort((a, b) => b.time.localeCompare(a.time)),
    );
  }

  toggleDay(day: DiaryDay) {
    day.isExpanded = !day.isExpanded;
  }
}
