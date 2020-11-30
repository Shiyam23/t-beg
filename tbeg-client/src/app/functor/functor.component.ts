import { Component, OnInit } from '@angular/core';
import { lowerCase } from 'lodash';
import { Subscription } from 'rxjs';
import { AppProgressService } from '../services/appProgress/app-progress.service';
import { SignalRService } from '../services/signalR/signal-r.service';

@Component({
  selector: 'app-functor',
  templateUrl: './functor.component.html',
  styleUrls: ['./functor.component.css']
})
export class FunctorComponent implements OnInit {

  private functorListSub : Subscription;
  private validatorSub : Subscription;
  functors : {value:string, viewValue:string}[] = new Array();
  constructor(public progress : AppProgressService, public signalR : SignalRService) {
  }

  ngOnInit(): void {
    this.signalR.askServer();
    this.signalR.listenToServer();
    this.functorListSub = this.signalR.functorList.subscribe(array => {
      array.forEach( string => {
        this.functors.push({
          value: string,
          viewValue: string
        })
      })
    })
  }


  selected = 'Powerset';

  public next = () => {
    this.progress.selectedFunctor = this.selected;
    this.signalR.askValidator(this.selected);
    this.signalR.getValidator();
    this.validatorSub = this.signalR.validator.subscribe(array => {
      this.progress.validator = array[0];
      this.progress.validatorErrorMessage = array[1];
      this.progress.forward();
    })
    
  } 
}
