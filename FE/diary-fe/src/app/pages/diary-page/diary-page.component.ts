import { CommonModule } from '@angular/common';
import {
  Component,
  OnInit,
} from '@angular/core';
import { ReactiveFormsModule, FormGroup } from '@angular/forms';
import { SvgIconComponent } from "@src/app/components/svg-icon/svg-icon.component";
import { DiaryMockService, DiaryDay } from '@src/app/services/diary-mock.service';
import { ModalComponent } from "@src/app/components/modal/modal.component";
import { CheckboxComponent } from "@src/app/components/checkbox/checkbox.component";
import { FormBuilderService } from '@src/app/services/form-builder.service';
import * as rules from '@src/shared/utils/validator-rules';

interface AddDiarySectionRequest {
  timeZoneId: string;
  detail: string;
  isPinned: boolean;
}

@Component({
  selector: 'app-diary-page',
  imports: [SvgIconComponent, CommonModule, ModalComponent, ReactiveFormsModule, CheckboxComponent],
  templateUrl: './diary-page.component.html',
  styleUrl: './diary-page.component.scss',
})
export class DiaryPageComponent implements OnInit {
  days: DiaryDay[] = [];
  isDescriptionExpanded = false;

  sectionForm!: ReturnType<typeof this.formService.create<AddDiarySectionRequest>>;

  constructor(private diaryService: DiaryMockService, private formService: FormBuilderService) {}

  ngOnInit() {
    this.sectionForm = this.formService.create<AddDiarySectionRequest>(b => {
      b.for('timeZoneId', Intl.DateTimeFormat().resolvedOptions().timeZone);
      b.for('detail', '').add(rules.required, 'Write something ?');
      b.for('isPinned', false);
    });
    this.loadDays();
  }

  loadDays() {
    this.diaryService.getDays().subscribe({
      next: (days) => {
        this.days = days;
      },
      error: (error) => {
        console.error('Error loading days:', error);
      }
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
    if (day.isExpanded) {
      day.isExpanded = false;
    } else {
      if (day.sections.length === 0) {
        day.isLoading = true;
        this.diaryService.getSectionsForDay(day.id).subscribe({
          next: (sections) => {
            day.sections = sections;
            day.isExpanded = true;
            day.isLoading = false;
          },
          error: (error) => {
            console.error('Error loading sections:', error);
            day.isLoading = false;
          }
        });
      } else {
        day.isExpanded = true;
      }
    }
  }
}
