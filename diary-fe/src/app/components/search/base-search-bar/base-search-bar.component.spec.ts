import { ComponentFixture, TestBed } from '@angular/core/testing';

import { BaseSearchBarComponent } from './base-search-bar.component';

describe('BaseSearchBarComponent', () => {
  let component: BaseSearchBarComponent;
  let fixture: ComponentFixture<BaseSearchBarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [BaseSearchBarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(BaseSearchBarComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
