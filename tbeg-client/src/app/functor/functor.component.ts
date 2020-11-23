import { Component, OnInit } from '@angular/core';
import { lowerCase } from 'lodash';
import { Subscription } from 'rxjs';
import { AppProgressService } from '../services/app-progress.service';
import { SignalRService } from '../services/signal-r.service';

@Component({
  selector: 'app-functor',
  templateUrl: './functor.component.html',
  styleUrls: ['./functor.component.css']
})
export class FunctorComponent implements OnInit {

  private functorListSub : Subscription;
  functors : {value:string, viewValue:string}[] = new Array();
  constructor(public progress : AppProgressService, public signalR : SignalRService) {
  }

  ngOnInit(): void {
    this.signalR.askServer();
    this.signalR.listenToServer();
    this.functorListSub = this.signalR.functorList.subscribe(array => {
      array.forEach( string => {
        this.functors.push({
          value: lowerCase(string),
          viewValue: string
        })
      })
    })
  }


  selected = 'powerset';

  public next() {
    this.progress.selectedFunctor = this.selected;
    this.progress.forward();
  } 
}
