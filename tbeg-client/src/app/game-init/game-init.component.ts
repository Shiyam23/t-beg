import { Component, OnInit } from '@angular/core';
import { MatButtonToggleChange } from '@angular/material/button-toggle';
import { ceil } from 'lodash';
import { State } from '../graphModel';
import { AppProgressService } from '../services/appProgress/app-progress.service';

@Component({
  selector: 'app-game-init',
  templateUrl: './game-init.component.html',
  styleUrls: ['./game-init.component.css']
})
export class GameInitComponent implements OnInit {

  selectedStatesLabel : String = "(?,?)";

  selectedStates : Array<State>;
  paper : joint.dia.Paper;
  graph : joint.dia.Graph;
  highlighter : joint.dia.Graph;


  constructor(public progress :  AppProgressService) {
    this.selectedStates = new Array<State>();
    this.paper = this.progress.paper;
    this.graph = this.progress.graph;
    this.highlighter = this.progress.highlighter;
  }

  ngOnInit(): void {

    
    this.paper.trigger('blank:pointerclick');
    this.paper.unbind('blank:pointerclick');
    this.paper.on('blank:pointerclick', (v,x,y) => {
      this.selectedStates.forEach( state => {
        this.paper.findViewByModel(state.model).unhighlight(null, this.highlighter)
      })
      this.selectedStates = new Array<State>();
      this.updateState();
    });

    // var state1 : State = State.allStates[0];
    // var state2 : State = State.allStates[1];
    // var vw1 = this.paper.findViewByModel(state1.model);
    // var vw2 = this.paper.findViewByModel(state2.model);
    // vw1.highlight(null, this.progress.highlighter);
    // vw2.highlight(null, this.progress.highlighter);
    // this.selectedStates[0] = state1;
    // this.selectedStates[1] = state2;

    this.paper.unbind('element:pointerclick');
    this.paper.on('element:pointerclick', (cellView : joint.dia.CellView, evt, x, y) => {
      if (this.selectedStates.length < 2) {
        var selectedState : State = State.findStateByModel(<joint.dia.Element>(cellView.model));
        this.selectedStates.push(selectedState);
        cellView.highlight(null, this.progress.highlighter);
      }
      this.updateState();
    });
  }

  public selectRole = (event : MatButtonToggleChange) => {
    this.progress.isSpoiler = event.value === "True"
  }

  public startClick = () => {
    this.progress.selectedStates = this.selectedStates;
    this.progress.forward();
  }

  public updateState() {

    this.selectedStatesLabel =
      "(" +
      (this.selectedStates[0]?.name ?? "?") +
      "," +
      (this.selectedStates[1]?.name ?? "?") +
      ")"
  }

  public swap() {
    if (this.selectedStates.length == 2) {
      var tempState : State = this.selectedStates[0];
      this.selectedStates[0] = this.selectedStates[1];
      this.selectedStates[1] = tempState;
      this.updateState();
    }
  }

}
