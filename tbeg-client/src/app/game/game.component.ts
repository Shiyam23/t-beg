import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { Subscription } from 'rxjs';
import { AppProgressService } from '../services/appProgress/app-progress.service';
import { SignalRService } from '../services/signalR/signal-r.service';
import { Event } from '../eventModel';
import { State } from '../graphModel';
import { state } from '@angular/animations';
import { MatButtonToggleChange, MatButtonToggleGroup } from '@angular/material/button-toggle';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})
export class GameComponent implements OnInit {

  gameSteps : Subscription;
  actualStep : number;
  startDisabled : boolean = true;
  selStatesString : Array<string> = ["{ - }","{ - }","{ - }","{ - }"];
  selStates : Array<Array<State>> = new Array<Array<State>>(4);
  x : number = 1;
  y : number = 2;
  disabled1 : boolean = true;
  disabled2 : boolean = true;
  selection : number = 1;

  @ViewChild('selectedState') selectedState : MatButtonToggleGroup;
  @ViewChild('selectedPred') selectedPred : MatButtonToggleGroup;


  constructor(public signalR : SignalRService, public progress : AppProgressService) { }

  ngOnInit(): void {
    
    this.signalR.listenToInfoStep();
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
        default:
          break;
      }
    });
  }

  infoStep0(event : Event) {
    this.selStates[0] = new Array<State>();
    this.x = event.x;
    this.y = event.y;
    if (event.userIsSpoiler) {
      this.disabled1 = false;
      this.bindGraphToArray(0, false);
      this.startDisabled = false;
    }
    this.actualStep = 1;

  }
  infoStep1(event : Event) {
    this.disabled1 = true;
    if (Number(this.selectedState.value) == this.x) {
      State.allStates.forEach(state => {
        if(state.name == this.x.toString()) state.setStrokeColor("darkmagenta");
        if(state.name == this.y.toString()) state.setStrokeColor("cyan");
        event.pred1.forEach(
          item => {
            if(state.name == (item+1).toString()) state.setColor("plum");
          }
        )
      })
    }
    else {
      State.allStates.forEach(state => {
        if(state.name == this.y.toString()) state.setStrokeColor("darkmagenta");
        if(state.name == this.x.toString()) state.setStrokeColor("cyan");
        event.pred1.forEach(
          item => {
            if(state.name == (item-1).toString()) state.setColor("plum");
          }
        )
      })
    }
    

  }
  infoStep2(event : Event) {
    
  }
  infoStep3(event : Event) {

  }
  infoStep4(event : Event) {

  }

  bindGraphToArray(i : number, singleItem : boolean) {
    this.progress.paper.unbind('blank:pointerclick');
      this.progress.paper.on('blank:pointerclick', () => {
        this.selStates[i].forEach(state => {
          this.progress.paper.findViewByModel(state.model).unhighlight(null, this.progress.highlighter)
        })
        this.selStates[i] = new Array<State>();
        this.updateLabel(i);
      })
      this.progress.paper.unbind('element:pointerclick');
      this.progress.paper.on('element:pointerclick', (cellView, event, x, y) => {
        if (!singleItem || this.selStates[i].length < 1) {
          var selectedState : State = State.findStateByModel(<joint.dia.Element>(cellView.model));
          this.selStates[i].push(selectedState);
          cellView.highlight(null, this.progress.highlighter);
          this.updateLabel(i);
        }
      })
  }

  updateLabel(i : number) {
    
    var string;
    if (this.selStates[i].length > 0) {
      string = 
        "{" + 
        this.selStates[i]
        .map(state => state.name)
        .reduce((acc,value) => acc + ", " + value) + 
        "}";
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
    this.signalR.sendStep(this.progress.selectedFunctor, selection, states);

  }

  selectionChange(event) {
    this.selection = Number(event.value);
  }


}
