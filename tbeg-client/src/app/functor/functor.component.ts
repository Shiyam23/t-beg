import { Component, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { lowerCase } from 'lodash';
import { Subscription } from 'rxjs';
import { AppProgressService } from '../services/appProgress/app-progress.service';
import { SignalRService } from '../services/signalR/signal-r.service';
import { DialogComponent, DialogData, DialogDataOption, DialogDataType } from '../templates/dialog/dialog.component';

@Component({
  selector: 'app-functor',
  templateUrl: './functor.component.html',
  styleUrls: ['./functor.component.css']
})
export class FunctorComponent implements OnInit {

  private functorListSub : Subscription;
  private validatorSub : Subscription;
  functors : {value:string, viewValue:string}[] = new Array();
  constructor(
    public progress : AppProgressService, 
    public signalR : SignalRService,
    private dialog : MatDialog
    ) {
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
