import { TestBed } from '@angular/core/testing';

import { RendererServiceService } from './renderer-service.service';

describe('RendererServiceService', () => {
  let service: RendererServiceService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(RendererServiceService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
