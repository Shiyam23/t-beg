import { Component, OnInit, ViewChild } from '@angular/core';
import { AppProgressService } from '../services/appProgress/app-progress.service';
import { SignalRService } from '../services/signalR/signal-r.service';
import { State, Link} from '../graphModel'

@Component({
  selector: 'app-graph-panel',
  templateUrl: './graph-panel.component.html',
  styleUrls: ['./graph-panel.component.css']
})
export class GraphPanelComponent implements OnInit {


  @ViewChild('linkValue')
  linkValue;

  @ViewChild('fileSelector')
  fileSelector;

  constructor(public progress : AppProgressService, public signalR : SignalRService) { }

  ngOnInit(): void {
  }

  startClick = () => {
    var states : Array<State> = State.allStates.sort( (a,b) => Number(a.name) - Number(b.name));
    var alphabet : Array<string> = new Array<string>();
    var links : Array<Link> = new Array<Link>();
    Link.allLinks.forEach(link => {
        link.name.toString().split(',').forEach(char => {
            if (alphabet.indexOf(char) == -1) alphabet.push(char);
            var newLink : Link = link;
            links.push(new Link(char, link.source, link.target, link.value, null));
        });
    });
    this.progress.forward();
    this.progress.paper.freeze();
    //this.signalR.sendGraph(states, links, alphabet, this.progress.selectedFunctor)
  }

  saveAsJson = () => {
    var graph = this.progress.graph.toJSON();
    var stateIDs = State.allStates.map(state => state.model.id);
    var linkIDs = Link.allLinks.map(link => link.model.id);
    var data = {
        functor: this.progress.selectedFunctor,
        graph: graph,
        states: State.allStates,
        stateIDs: stateIDs,
        links: Link.allLinks,
        linkIDs: linkIDs,
    }
    var blob = new Blob([JSON.stringify(data)], {type: "text/plain;charset=utf-8"});
    saveAs(blob, "graph.txt");
}


  loadFromJson = (event : any) => {


    this.progress.selectedItem = null;
    this.progress.selectedStateView = null;

    var file = event.target.files[0];
    var fr = new FileReader();
    fr.onload = () => {

        var json : JSON = JSON.parse(<string>fr.result);
        if (json["functor"] == this.progress.selectedFunctor) {
            this.progress.graph.fromJSON(json["graph"]);
            State.allStates = new Array<State>();
            Link.allLinks = new Array<Link>();
            json["states"].forEach( (state,index) => {
                var id : string = <string>(json["stateIDs"][index]);
                var model  =  <joint.dia.Element> this.progress.graph.getCell(id);
                var newState : State = new State(state.name, model, state.isStartState, state.isFinalState);
            });

            json["links"].forEach( (link,index) => {
                var id : string = <string>(json["linkIDs"][index]);
                var model  =  <joint.dia.Link> this.progress.graph.getCell(id);
                var newLink : Link = new Link(link.name, link.source, link.target, link.value, model);
            });
        }
        else {
            //TODO create Error message
            console.log("Not the same functor!!")
        }
        
    }
    fr.readAsText(file);
  }

  setLinkValue(event : any) {
    if (!this.linkValue.hasError('pattern'))
    (<Link>this.progress.selectedItem).value = event;
  }
  setLinkName(event : any) {
      (<Link>this.progress.selectedItem).setName(event);
  }

  setStateStart(event : any) {
      (<State>this.progress.selectedItem).setStartState(event);
  }

  setStateFinal(event : any) {
      (<State>this.progress.selectedItem).setFinalState(event);
  }

  setStateName(event : any) {
      if (State.allStates.findIndex(state => state.name == event) == -1)
      (<State>this.progress.selectedItem).setName(event);
  }

}
