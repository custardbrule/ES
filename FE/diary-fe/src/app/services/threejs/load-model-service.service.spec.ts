import { TestBed } from '@angular/core/testing';

import { LoadModelServiceService } from './load-model-service.service';

describe('LoadModelServiceService', () => {
  let service: LoadModelServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(LoadModelServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
