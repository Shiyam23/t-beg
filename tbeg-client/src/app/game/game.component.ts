import { Component, OnInit, ViewChild } from '@angular/core';
import { Subscription } from 'rxjs';
import { AppProgressService } from '../services/appProgress/app-progress.service';
import { SignalRService } from '../services/signalR/signal-r.service';
import { Event, InfoEvent, StepBackEvent } from '../eventModel';
import { State } from '../graphModel';
import { MatButtonToggleGroup } from '@angular/material/button-toggle';
import { MatDialog } from '@angular/material/dialog';
import { DialogComponent } from '../templates/dialog/dialog.component';
import { DialogData, DialogDataType } from '../templates/dialog/dialogData';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})
export class GameComponent implements OnInit {

  gameSteps : Subscription;
  backSteps : Subscription;
  info : Subscription;
  actualStep : number;
  startDisabled : boolean = true;
  selStatesString : Array<string> = ["{ - }","{ - }","{ - }","{ - }"];
  selStates : Array<Array<State>> = new Array<Array<State>>(4);
  x : number = 1;
  y : number = 2;
  disabled1 : boolean = true;
  disabled2 : boolean = true;

  @ViewChild('selectedState') selectedState : MatButtonToggleGroup;
  @ViewChild('selectedPred') selectedPred : MatButtonToggleGroup;


  constructor(
    public signalR : SignalRService, 
    public progress : AppProgressService,
    private dialog : MatDialog
    ) {
  }

  ngOnInit(): void {
    
    this.signalR.listenToInfoStep();
    this.signalR.listenToInfoText();
    this.signalR.listenToStepBack();
    this.gameSteps = this.signalR.gameSteps.subscribe((event : Event) => {
      switch (event.step) {
        case 0:
          this.infoStep0(event); break;
        case 1:
          this.infoStep1(event); break;
        case 2:
          this.infoStep2(event); break;
        case 3:
          this.infoStep3(event); break;
        case 4:
          this.infoStep4(event); break;
      }
    });
    this.backSteps = this.signalR.backSteps.subscribe((event : StepBackEvent) => this.stepBack(event));
    this.info = this.signalR.info.subscribe((event : InfoEvent) => {
      this.infoMessage(event);
    });
  }

  infoMessage(event : InfoEvent) {

    var data : DialogData = {
      type : event.over ? DialogDataType.GAMEOVER : DialogDataType.ERROR,
      content : event.name
    }

    this.dialog.open(DialogComponent, {
      data: data,
    });
    if (!event.over) {
      this.startDisabled = false;
      this.closeSnackbar();
      this.progress.paper.trigger('blank:pointerclick')
    }
  }

  infoStep0(event : Event) {
    this.selectedState._buttonToggles.first.checked = true;
    this.resetMarkers();
    this.unbindGraphToArray();
    this.disabled2 = true;
    for (let i = 0; i < 4; i++) {
      this.selStates[i] = new Array<State>();
      this.updateLabel(i)
    }
    this.x = event.x;
    this.y = event.y;
    if (event.userIsSpoiler) {
      this.disabled1 = false;
      this.bindGraphToArray(0, false);
      this.startDisabled = false;
      this.closeSnackbar();
    }
    this.actualStep = 1;

  }
  infoStep1(event : Event) {
    this.checkFirstStateButton(this.x == event.selection);
    this.disabled1 = true;
    this.unbindGraphToArray();
    this.setMarkers(event);
    if (!event.userIsSpoiler) {
      this.bindGraphToArray(1, false);
      this.startDisabled = false;
      this.closeSnackbar();
    }
    this.actualStep = 2;
  }
  infoStep2(event : Event) {
    this.disabled1 = true;
    this.unbindGraphToArray();
    State.allStates.forEach(state => {
      event.pred1.forEach(
        item => {
          if (state.name == (item+1).toString()) {
            state.setColor("#e0ffff");
            if (event.userIsSpoiler) this.selStates[1].push(state);
          }
        }
      )
    })
    if (event.userIsSpoiler) {
      this.disabled2 = false;
      this.bindGraphToArray(2, true);
      this.startDisabled = false;
      this.closeSnackbar();
    }
    this.updateLabel(1);
    this.actualStep = 3;
  }

  infoStep3(event : Event) {
    this.checkFirstPredButton(event.selection == 0) ;
    this.disabled2 = true;
    this.unbindGraphToArray();
    if (!event.userIsSpoiler) {
      this.selStates[2].push(State.allStates.find(state => state.name == (event.pred1[0]+1).toString()));
      this.updateLabel(2);
      this.bindGraphToArray(3, true);
      this.startDisabled = false;
      this.closeSnackbar();
    }
    this.actualStep = 4;
  }

  infoStep4(event : Event) {
    this.unbindGraphToArray();
    if (event.userIsSpoiler) {
      this.selStates[3].push(State.allStates.find(state => state.name == (event.selection+1).toString()));
      this.updateLabel(3);
    }
  }

  stepBack(event : StepBackEvent) {
    if (event.x != null && event.y != null) {
      this.x = event.x;
      this.y = event.y;
      if (event.step == -3 || event.step == -4) {
        this.checkFirstStateButton(event.x == event.selection);
      }
      this.setMarkers(event);
    }
    /* if (event.pred2 != null) {
      State.allStates.forEach((state) => {
        event.pred2.forEach((item) => {
          if (Number(state.name) == item) this.selStates[1].push(state)
        })
      });
      this.updateLabel(1);
    } */
  }

  bindGraphToArray(i : number, singleItem : boolean) {
    this.unbindGraphToArray();
    this.progress.paper.on('blank:pointerclick', () => {
      this.resetSelection(i)
      this.selStates[i] = new Array<State>();
      this.updateLabel(i);
    })
    this.progress.paper.on('element:pointerclick', (cellView, event, x, y) => {
      if (!singleItem || this.selStates[i].length < 1) {
        var selectedState : State = State.findStateByModel(<joint.dia.Element>(cellView.model));
        if (this.selStates[i].indexOf(selectedState) == -1) {
          cellView.highlight(null, this.progress.highlighter);
          this.selStates[i].push(selectedState);
          this.updateLabel(i);
        }
      }
    })
  }

  unbindGraphToArray() {
    this.progress.paper.unbind('blank:pointerclick');
    this.progress.paper.unbind('element:pointerclick');
  }

  resetSelection(i : number) {
    this.selStates[i].forEach(state => {
      this.progress.paper.findViewByModel(state.model).unhighlight(null, this.progress.highlighter)
    })
  }

  updateLabel(i : number) {
    var string;
    if (this.selStates[i].length > 0) {
      string = 
        "{ " + 
        this.selStates[i]
        .map(state => state.name)
        .reduce((acc,value) => acc + ", " + value) + 
        " }";
    }
    else string = "{ - }"
    this.selStatesString[i] = string;
  }

  startClick = () => {
    var step = this.actualStep;
    var selection : number;
    if (step == 1) {
      selection = Number(this.selectedState.value);
    }
    else if (step == 3) selection = Number(this.selectedPred.value);
    else selection = 0;
    var states : Array<number> = this.selStates[step-1].map(state => Number(state.name)-1);
    if (states.length > 0) {
      this.signalR.sendStep(this.progress.selectedFunctor, selection, states);
      this.startDisabled = true;
      this.openSnackbar();
      this.resetSelection(step-1);
    }
    
  }
  
  backClick = () => {
    this.resetMarkers();
    for (let i : number = 0; i < 4; i++) {
      this.selStates[i] = new Array<State>();
      this.updateLabel(i);
    }
    this.actualStep -= 3;
    this.signalR.sendStepBack(this.progress.selectedFunctor);
    this.startDisabled = true;
    this.openSnackbar();
  }



  resetMarkers() {
    State.allStates.forEach(state => {
      if (state.name == this.x.toString() || state.name == this.y.toString())
        state.setStrokeColor("#333333"); 
    })
    if (this.selStates[0] != null && this.selStates[2] != null) {
      this.selStates[0].forEach(state => state.setColor('#ffffff'))
      this.selStates[1].forEach(state => state.setColor('#ffffff'))
    }
  }

  setMarkers(event: Event | StepBackEvent) {
    State.allStates.forEach(state => {
      if (this.selectedState._buttonToggles.first.checked) {
        if(state.name == this.x.toString()) state.setStrokeColor("#8b008b");
        if(state.name == this.y.toString()) state.setStrokeColor("#00ffff");
      }
      else {
        if(state.name == this.y.toString()) state.setStrokeColor("#8b008b");
        if(state.name == this.x.toString()) state.setStrokeColor("#00ffff");
      }
      event.pred1.forEach(
        item => {
          if(state.name == (item+1).toString()) {
            state.setColor("#dda0dd");
            if (!event.userIsSpoiler || event.step < 0) this.selStates[0].push(state);
          }
        }
      );
      if (event instanceof StepBackEvent && event.pred2 != null) {
        event.pred2.forEach(
          item => {
            if(state.name == (item+1).toString()) {
              state.setColor("#e0ffff");
              this.selStates[1].push(state);
            }
          }
        );
        this.updateLabel(1);
      }
    })
    this.updateLabel(0);
  }

  openSnackbar() {
    this.progress.snackbar.next("Waiting for Server ...");
  }

  closeSnackbar() {
    this.progress.snackbar.next(null);
  }

  checkFirstStateButton(bool : boolean) {
    this.selectedState._buttonToggles.first.checked = bool;
    this.selectedState._buttonToggles.last.checked = !bool;
  }
  checkFirstPredButton(bool : boolean) {
    this.selectedPred._buttonToggles.first.checked = bool;
    this.selectedPred._buttonToggles.last.checked = !bool;
  }
}
