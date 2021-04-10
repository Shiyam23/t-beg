import { NoopScrollStrategy } from '@angular/cdk/overlay';
import { AfterViewInit, Component, OnDestroy, OnInit } from '@angular/core';
import { MatButtonToggleChange } from '@angular/material/button-toggle';
import { MatDialog } from '@angular/material/dialog';
import { Link, State } from '../graphModel';
import { AppProgressService } from '../services/appProgress/app-progress.service';
import { SignalRService } from '../services/signalR/signal-r.service';
import { DialogComponent, DialogData, DialogDataType, DialogDataOption} from "../templates/dialog/dialog.component";

@Component({
  selector: 'app-game-init',
  templateUrl: './game-init.component.html',
  styleUrls: ['./game-init.component.css']
})
export class GameInitComponent implements OnInit, AfterViewInit{

  selectedStatesLabel : String = "(?,?)";

  selectedStates : Array<State>;
  paper : joint.dia.Paper;
  graph : joint.dia.Graph;
  highlighter : joint.dia.Graph;


  constructor(
    public progress :  AppProgressService,
    public signalR : SignalRService,
    private dialog : MatDialog
    ) {
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

    this.paper.unbind('link:pointerclick');
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

  ngAfterViewInit() : void {
    if (this.progress.tutorial) {
      let data : DialogData = {
        type: DialogDataType.TUTORIAL,
        option: DialogDataOption.DISMISS,
        content:  "You made it! Now you have to setup this game:" + 
                  "<ol>" +
                  "<li>Choose your role:" +
                    "<ul>" +
                      "<li>Spoiler: Tries to show that the selected states are not behaviourally equivalent</li>" +
                      "<li>Duplicator: Tries to show that the selected states are behaviourally equivalent</li>" +
                      "<li>For the purpose of this tutorial, <b>choose Duplicator</b>" +
                    "</ul>" +
                  "</li>" +
                  "<li>Select the initial pair of states for which the behavioural equivalence should be checked. For this tutorial, <b>choose 1 and 4</b>"
      }
      let width = '45vw';
      this.progress.lastData = data;
      this.progress.lastWidth = width;
      this.dialog.open(DialogComponent, {
          data: data,
          width: width,
          scrollStrategy: new NoopScrollStrategy()
      })
    }
}




  public selectRole = (event : MatButtonToggleChange) => {
    this.progress.isSpoiler = event.value === "True"
  }

  public startClick = () => {
    
    if (this.selectedStates.length < 2) {
      let data : DialogData = {
        option: DialogDataOption.DISMISS,
        type: DialogDataType.ERROR,
        content: "You have to select two states!"
      }
      this.dialog.open(DialogComponent,{
        data: data
      })
      return;
    }

    if (this.progress.tutorial 
        && (!(this.selectedStates[0].name == '1' && this.selectedStates[1].name == '4')
        || this.progress.isSpoiler == true)) {
          var data : DialogData = {
            option: DialogDataOption.DISMISS,
            type : DialogDataType.TUTORIAL,
            content:  "For this tutorial, please choose the <b>Duplicator</b> role and <b>(1,4)</b> as the initial pair. " +
                      "Please consider that the order of the pair matters! So (4,1) is not valid for this tutorial."
        };
        this.dialog.open(DialogComponent, {
          data : data,
          width: '26vw'
        });
        return;
      }

    this.progress.initialPair = this.selectedStates;
    var stateNames = this.selectedStates.map(state => state.name.toString())
    this.progress.stateNames = stateNames;
    this.paper.trigger('blank:pointerclick');
    this.paper.unbind('element:pointerclick');
    this.paper.unbind('blank:pointerclick');
    this.progress.forward();
    
  }

  public goBack = () => {
    this.paper.trigger('blank:pointerclick');
    this.progress.isSpoiler = true;
    this.paper.unbind('blank:pointerclick');
    this.paper.on('blank:pointerclick', (v,x,y) => {
      if (this.progress.selectedStateView) {
          let selectedBefore = this.progress.selectedStateView.model;
          if (selectedBefore.isLink()) {
              Link.findLinkByModel(<joint.dia.Link>selectedBefore).select(false)
          }
          else {
              this.progress.selectedStateView?.unhighlight(null, this.highlighter);
          }
          this.progress.selectedStateView = null;
          this.progress.selectedItem = null;
      }
    });
    this.paper.unbind('element:pointerclick');
    this.paper.on('element:pointerclick', (cellView) => {
      let selectedBefore = this.progress.selectedStateView;
      if (selectedBefore?.model.isLink()) {
          Link.findLinkByModel(<joint.dia.Link>selectedBefore.model).select(false);
      } else {
          selectedBefore?.unhighlight(null, this.highlighter)
      }
      this.progress.selectedStateView = cellView;
      this.progress.isStateWindow = true;
      cellView.highlight(null, this.highlighter);
      var state : State = State.findStateByModel(cellView.model);
      this.progress.selectedItem = state;
  });
  this.paper.on('link:pointerclick', (cellView) => {
    var link : Link = Link.findLinkByModel(cellView.model);
    let selectedBefore = this.progress.selectedStateView;
    if (selectedBefore?.model.isLink()) {
        Link.findLinkByModel(<joint.dia.Link>selectedBefore.model).select(false);
    } else {
        selectedBefore?.unhighlight(null, this.highlighter)
    }
    link.select(true);
    this.progress.selectedStateView = cellView;
    this.progress.isStateWindow = false;
    this.progress.selectedItem = link;
    this.progress.selectedLabelArray = link.name.split(',');
});
    this.progress.backward();
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
