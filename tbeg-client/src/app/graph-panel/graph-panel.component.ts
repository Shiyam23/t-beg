import { Component, OnDestroy, ViewChild } from '@angular/core';
import { AppProgressService } from '../services/appProgress/app-progress.service';
import { SignalRService } from '../services/signalR/signal-r.service';
import { State, Link} from '../graphModel'
import { saveAs } from 'file-saver';
import { MatDialog } from '@angular/material/dialog';
import { DialogData, DialogDataType, DialogComponent, DialogDataOption } from '../templates/dialog/dialog.component';
import { Subscription } from 'rxjs';


@Component({
  selector: 'app-graph-panel',
  templateUrl: './graph-panel.component.html',
  styleUrls: ['./graph-panel.component.css']
})
export class GraphPanelComponent implements OnDestroy {

  initGameViewSub : Subscription;

  @ViewChild('linkValue')
  linkValue;

  @ViewChild('fileSelector')
  fileSelector;


  constructor(
        public progress : AppProgressService,
        public signalR : SignalRService,
        private dialog : MatDialog
    ) { }

  ngOnDestroy(): void {
    this.signalR.stopListenInitGameView();
    if (this.initGameViewSub != null) this.initGameViewSub.unsubscribe();
  }

  startClick = () => {
      
    if (State.allStates.length < 2) {
        var data : DialogData = {
            option: DialogDataOption.DISMISS,
            type : DialogDataType.ERROR,
            content: "Graph cannot be empty! Atleast two States are expected."
        };
        this.dialog.open(DialogComponent, {
            data : data
        });
        return;
    }

    var states : Array<State> = State.allStates.sort( (a,b) => Number(a.name) - Number(b.name));
    var alphabet : Array<string> = new Array<string>();
    var links : Array<Link> = new Array<Link>();
    Link.allLinks.forEach(link => {
        link.name.toString().split(',').forEach((char,index) => {
            if (alphabet.indexOf(char) == -1) alphabet.push(char);
            var newLink : Link = link;
            links.push(new Link(char, link.source, link.target, link.value[index], null));
        });
    });
    this.signalR.listenOnInitGameView();
    this.signalR.sendGraph(states, links, alphabet, this.progress.selectedFunctor);
    this.initGameViewSub = this.signalR.initGameView.subscribe( bool => {
        if (bool) this.progress.forward();
    })
  }

  goBack = () => {
      State.allStates = new Array<State>();
      Link.allLinks = new Array<Link>();
      this.progress.backward();
  }

    saveAsJson = () => {
        if (State.allStates.length == 0) {
            let data : DialogData = {
                option: DialogDataOption.DISMISS,
                type : DialogDataType.ERROR,
                content: "Empty graph can not be saved!"
            };
            this.dialog.open(DialogComponent, {
                data : data
            });
            return;
        }
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

    load = (event : any) => {
        if (State.allStates.length != 0) {
            let data : DialogData = {
                type: DialogDataType.WARNING,
                option: DialogDataOption.ACCEPT,
                content:    "Your actual graph is going to be deleted if you load another graph from file." +
                            "You want to proceed?"
            }   
            let dialogRef = this.dialog.open(DialogComponent, {
                data: data,
                disableClose: true,    
            })
            dialogRef.afterClosed().subscribe(result => {
                if (result == "Accept") this.loadFromJson(event);
            })   
        }
        else this.loadFromJson(event);
    }

  loadFromJson = (event : any) => {
    
    this.progress.selectedItem = null;
    this.progress.selectedStateView = null;
    var file = event.target.files[0];
    var fr = new FileReader();
    fr.onload = () => {
        try {
            var json : JSON = JSON.parse(<string>fr.result);
            if (json["functor"] == this.progress.selectedFunctor) {
                this.progress.graph.fromJSON(json["graph"]);
                State.allStates = new Array<State>();
                Link.allLinks = new Array<Link>();
                json["states"].forEach( (state,index) => {
                    var id : string = <string>(json["stateIDs"][index]);
                    var model  =  <joint.dia.Element> this.progress.graph.getCell(id);
                    var newState : State = new State(state.name.toString(), model, state.isStartState, state.isFinalState);
                });

                json["links"].forEach( (link,index) => {
                    var id : string = <string>(json["linkIDs"][index]);
                    var model  =  <joint.dia.Link> this.progress.graph.getCell(id);
                    let source = State.allStates.find(state => state.name == link.source.name);
                    let target = State.allStates.find(state => state.name == link.target.name);
                    var newLink : Link = new Link(link.name, source, target, link.value, model);
                });
                console.log(State.allStates);
                console.log(Link.allLinks);
            }
            else {
                //TODO create Error message
                var data : DialogData = {
                    option: DialogDataOption.DISMISS,
                    type : DialogDataType.ERROR,
                    content :   "The functor you chose is not the same as the functor used in the file!" +
                                "\nYou chose: " + this.progress.selectedFunctor +
                                "\nFunctor used in file: " + json["functor"]
                }
                this.dialog.open(DialogComponent, {data: data});
            }
        } catch (e) {
            var data : DialogData = {
                option: DialogDataOption.DISMISS,
                type : DialogDataType.ERROR,
                content : "An error occurred while reading this file!"
            }
            this.dialog.open(DialogComponent, {data: data});
        }
    }
    if (file != null) {
        fr.readAsText(file);
        event.target.value = "";
    }
    
  }

  setLinkValue(event : any, index : number) {
    if (!this.linkValue.hasError('pattern'))
    (<Array<String>>(<Link>this.progress.selectedItem).value)[index] = event;
  }
  setLinkName(event : string) {
      if (event == null || event == '') return; 
      var link : Link = (<Link>this.progress.selectedItem);
      link.value = Array<string>(this.progress.selectedLabelArray.length);
      var array : string[] = event.split(',');
      if (array[array.length-1] == "") array.pop();
      this.progress.selectedLabelArray = array;
      link.setName(event);
  }

  setStateStart(event : any) {
      (<State>this.progress.selectedItem).setStartState(event);
  }

  setStateFinal(event : any) {
      (<State>this.progress.selectedItem).setFinalState(event);
  }

  setStateName(event : string) {
      if (
          event.length != 0
          && State.allStates.findIndex(state => state.name == event) == -1
          )
      (<State>this.progress.selectedItem).setName(event);
  }

}
