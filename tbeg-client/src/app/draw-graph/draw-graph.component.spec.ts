import { ComponentFixture, TestBed } from '@angular/core/testing';

import { DrawGraphComponent } from './draw-graph.component';

describe('DrawGraphComponent', () => {
  let component: DrawGraphComponent;
  let fixture: ComponentFixture<DrawGraphComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ DrawGraphComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(DrawGraphComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
