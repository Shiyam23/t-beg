import { Component, OnInit } from '@angular/core';
import { AppProgressService } from '../services/app-progress.service';

@Component({
  selector: 'app-functor',
  templateUrl: './functor.component.html',
  styleUrls: ['./functor.component.css']
})
export class FunctorComponent implements OnInit {

  constructor(public progress : AppProgressService) { }

  ngOnInit(): void {
  }

  functors = [
    {value: 'powerset', viewValue: 'Powerset'},
    {value: 'dx+1', viewValue: 'DX + 1'},
    {value: 'mealy', viewValue: 'Mealy'}
  ];

  selected = 'powerset';

  public next() {
    this.progress.selectedFunctor = this.selected;
    this.progress.forward();
  } 
}
