import { Component, OnDestroy, ViewChild } from '@angular/core';
import { AppProgressService } from '../services/appProgress/app-progress.service';
import { SignalRService } from '../services/signalR/signal-r.service';
import { State, Link} from '../graphModel'
import { saveAs } from 'file-saver';
import { MatDialog } from '@angular/material/dialog';
import { DialogData, DialogDataType, DialogComponent, DialogDataOption } from '../templates/dialog/dialog.component';
import { Subscription } from 'rxjs';
import * as joint from 'jointjs';


@Component({
  selector: 'app-graph-panel',
  templateUrl: './graph-panel.component.html',
  styleUrls: ['./graph-panel.component.css']
})
export class GraphPanelComponent implements OnDestroy {

  initGameViewSub : Subscription;

  @ViewChild('linkValue')
  linkValue;

  @ViewChild('stateValue')
  stateValue;

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

    if (State.allStates.some(state => {
        if (!RegExp(this.progress.stateValidator[0]).test(state.value)) {
            let data : DialogData = {
                option: DialogDataOption.DISMISS,
                type : DialogDataType.ERROR,
                content: `You need to give a value for state ${state.name}`
            };
            this.dialog.open(DialogComponent, {
                data : data
            });
            return true;
        }
    })) return;

    if (Link.allLinks.some(link => {
        let checkArray : string[];
        if (!Array.isArray(link.value)) checkArray = [<string>link.value];
        else checkArray = link.value;
        return checkArray.some((value,i) => {
            if (!RegExp(this.progress.linkValidator[0]).test(value)) {
                let chars = link.name.split(',');
                let data : DialogData = {
                    option: DialogDataOption.DISMISS,
                    type : DialogDataType.ERROR,
                    content: `${value} You need to give a value for link ${chars[i]} from ${link.source.name} to ${link.target.name}`
                };
                this.dialog.open(DialogComponent, {
                    data : data
                });
                return true;
            }
        })
    })) return;

    var states : Array<State> = State.allStates.sort( (a,b) => Number(a.name) - Number(b.name));
    var alphabet : Array<string> = new Array<string>();
    var links : Array<Link> = new Array<Link>();
    Link.allLinks.forEach(link => {
        link.name.toString().split(',').forEach((char,index) => {
            if (alphabet.indexOf(char) == -1) alphabet.push(char);
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
                    var newState : State = new State(state.name.toString(), model, state.value);
                });

                json["links"].forEach( (link,index) => {
                    var id : string = <string>(json["linkIDs"][index]);
                    var model  =  <joint.dia.Link> this.progress.graph.getCell(id);
                    let source = State.allStates.find(state => state.name == link.source.name);
                    let target = State.allStates.find(state => state.name == link.target.name);
                    var newLink : Link = new Link(link.name, source, target, link.value, model);
                });
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
        fr.onloadend = this.checkDoubleLinks;
    }
    
  }

  private checkDoubleLinks = () => {
    Link.allLinks.forEach(link => {
        let link2 : Link = Link.allLinks.find(link2 => 
            link.source == link2.target 
            && link.target == link2.source
            && link != link2);
        if (link2) {
            let vertexHandler = this.progress.updateVertex(link2.model, link.model, link.source.model, link.target.model);
            vertexHandler();
            if (link.vertexHandler == null) {
                link.vertexHandler = vertexHandler;
                link2.vertexHandler = vertexHandler;
                link.source.model.on('change:position', vertexHandler);
                link.target.model.on('change:position', vertexHandler);
                link.model.connector('smooth');
                link2.model.connector('smooth');
            }
        }
    })
  }



  setLinkValue(event : any, index : number) {
    if (!this.linkValue.hasError('pattern'))
    (<Array<String>>(<Link>this.progress.selectedItem).value)[index] = event;
  }

  setStateValue(event : any) {
    if (!this.stateValue.hasError('pattern'))
    (<State>this.progress.selectedItem).value = event;
  }


  setLinkName(event : string) {
      if (event == null || event == '' || event.startsWith(',') || event.endsWith(',')) return; 
      var link : Link = (<Link>this.progress.selectedItem);
      link.value = Array<string>(this.progress.selectedLabelArray.length);
      link.value.fill('');
      var array : string[] = event.split(',');
      if (array[array.length-1] == "") array.pop();
      this.progress.selectedLabelArray = array;
      link.setName(event);
  }

  setStateName(event : string) {
      if (
          event.length != 0
          && State.allStates.findIndex(state => state.name == event) == -1
          )
      (<State>this.progress.selectedItem).setName(event);
  }

  valueInfoClick = (content : string) => {
    let data : DialogData = {
        content: content,
        option: DialogDataOption.DISMISS,
        type: DialogDataType.INFO
    }
    this.dialog.open(DialogComponent, {
        data: data
    })
  }

}
