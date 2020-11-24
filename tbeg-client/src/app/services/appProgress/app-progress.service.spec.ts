import { TestBed } from '@angular/core/testing';

import { AppProgressService } from './app-progress.service';

describe('AppProgressService', () => {
  let service: AppProgressService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(AppProgressService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
