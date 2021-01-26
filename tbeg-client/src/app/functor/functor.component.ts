import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Subscription } from 'rxjs';
import { AppProgressService } from '../services/appProgress/app-progress.service';
import { SignalRService } from '../services/signalR/signal-r.service';
import { DialogComponent, DialogData, DialogDataOption, DialogDataType } from '../templates/dialog/dialog.component';

@Component({
  selector: 'app-functor',
  templateUrl: './functor.component.html',
  styleUrls: ['./functor.component.css']
})
export class FunctorComponent implements OnInit, OnDestroy {

  private functorListSub : Subscription;
  private validatorSub : Subscription;
  functors : {value:string, viewValue:string}[] = this.progress.availableFunctors;
  selected : string;

  constructor(
    public progress : AppProgressService, 
    public signalR : SignalRService,
    private dialog : MatDialog
    ) {
  }

  ngOnInit(): void {
    if (this.functors.length == 0) {
      this.signalR.askServer();
      this.signalR.listenToServer();
      this.functorListSub = this.signalR.functorList.subscribe(array => {
        array.forEach( string => {
          this.functors.push({
            value: string,
            viewValue: string
          });
        });
        if (array.length != 0) this.selected = array[0];
      })
    } else {
      this.selected = this.functors[0].viewValue;
    }
  }

  ngOnDestroy() : void {
    this.signalR.stopGetValidator();
    this.validatorSub.unsubscribe();
  }

  public next = () => {
    this.progress.selectedFunctor = this.selected;
    this.signalR.askValidator(this.selected);
    this.signalR.getValidator();
    this.validatorSub = this.signalR.validator.subscribe(array => {
      this.progress.stateValidator = array[0];
      this.progress.linkValidator = array[1];
      this.progress.forward();
    })
  } 

  public clickToolTip = () => {
    let data : DialogData = {
      type: DialogDataType.INFO,
      option: DialogDataOption.DISMISS,
      content:  "A functor describes the branching type of a system. An example is F = Powerset, which enables" + 
                "non-determinism e.g. x can map to a set of states: x -> {x,y}."
    }
    this.dialog.open(DialogComponent, {
      data: data
    })
  }
}
