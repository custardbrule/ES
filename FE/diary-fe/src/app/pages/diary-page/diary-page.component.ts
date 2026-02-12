import { CommonModule } from '@angular/common';
import {
  Component,
  OnInit,
} from '@angular/core';
import { SvgIconComponent } from "@src/app/components/svg-icon/svg-icon.component";
import { DiaryMockService, DiaryDay } from '@src/app/services/diary-mock.service';
import { ModalComponent } from "@src/app/components/modal/modal.component";
import { FormBuilderService } from '@src/app/services/form-builder.service';

@Component({
  selector: 'app-diary-page',
  imports: [SvgIconComponent, CommonModule, ModalComponent],
  templateUrl: './diary-page.component.html',
  styleUrl: './diary-page.component.scss',
})
export class DiaryPageComponent implements OnInit {
  days: DiaryDay[] = [];
  isDescriptionExpanded = false;

  constructor(private diaryService: DiaryMockService, private formService: FormBuilderService) {}

  ngOnInit() {
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
