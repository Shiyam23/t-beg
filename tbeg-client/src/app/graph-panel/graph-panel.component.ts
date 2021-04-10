import { AfterViewInit, Component, OnDestroy, ViewChild } from '@angular/core';
import { AppProgressService } from '../services/appProgress/app-progress.service';
import { SignalRService } from '../services/signalR/signal-r.service';
import { State, Link} from '../graphModel'
import { saveAs } from 'file-saver';
import { MatDialog } from '@angular/material/dialog';
import { DialogData, DialogDataType, DialogComponent, DialogDataOption } from '../templates/dialog/dialog.component';
import { Subscription } from 'rxjs';
import * as joint from 'jointjs';
import { NoopScrollStrategy, ScrollStrategyOptions } from '@angular/cdk/overlay';


@Component({
  selector: 'app-graph-panel',
  templateUrl: './graph-panel.component.html',
  styleUrls: ['./graph-panel.component.css']
})
export class GraphPanelComponent implements OnDestroy, AfterViewInit {

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

  ngAfterViewInit() : void {
    if (this.progress.tutorial) {
        let data : DialogData = {
            type: DialogDataType.TUTORIAL,
            option: DialogDataOption.DISMISS,
            content:  "Welcome to the graph editor! In the following you can see the instructions for creating the graph:" + 
                      "<ul>" +
                        "<li>Create State: Right click on the canvas and choose <b>Create State</b></li>" +
                        "<li>Create Edge: <b>Select</b> the source state (left click) and right click on the target state and choose <b>Create Link</b></li>" +
                        "<li>Change state/edge label: <b>Select</b> the state/edge (left click) and type the label in the <b>name</b> textfield on the left side</li>" +
                        "<li>Delete state/edge: <b>Right click</b> the state/edge and choose <b>Delete State</b> or <b>Delete Link</b></li>" + 
                        "<li>Clear selection: Just <b>click on an empty space</b> (This also applies to the following pages)</li>" + 
                      "</ul>" + 
                      "If you have multiple edges (e.g. a and b) with the same states as source and target, you have to draw one edge and give both labels together, but " + 
                      "separated with a comma (e.g. \'a,b\'). " + 
                      "Be aware that for specific functors you have to give values for all states and/or for all edges." + 
                      " You can save this graph with <b>Save</b> and load graphs you saved previously with <b>Load</b>. " + 
                      "\n\nIn the following you can see the last tutorial message by clicking on <b>Tutorial</b> on the top right corner." + 
                      " Try to create the following graph with the instructions mentioned before. Please consider the label of the states and edges!",
            image: "assets/img/dfa_example.svg",
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

  startClick = () => {
    if (State.allStates.length < 2) {
        var data : DialogData = {
            option: DialogDataOption.DISMISS,
            type : DialogDataType.ERROR,
            content: "Graph cannot be empty! At least two States are expected."
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

    if (this.progress.tutorial && !this.checkTutGraph()) return;

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

  private checkTutGraph() {
    let graphValid = true;
    if (State.allStates.length != 6) {
        var data : DialogData = {
            option: DialogDataOption.DISMISS,
            type : DialogDataType.TUTORIAL,
            content: "The graphs are not identical. The graph you should draw contains 6 states. Your graph has " + 
                    State.allStates.length + 
                    " state(s). Click on <b>Tutorial</b> to see the graph you should draw."
        };
        graphValid = false;
    }
    if (graphValid && Link.allLinks.length != 10) {
        var data : DialogData = {
            option: DialogDataOption.DISMISS,
            type : DialogDataType.TUTORIAL,
            content: "The graphs are not identical. The graph you should draw contains 10 edges. Your graph has " + 
                    Link.allLinks.length + 
                    " edge(s). Please consider that you have to draw the \'a,b\' edges as one edge. Click on <b>Tutorial</b> to see the graph you should draw."
        };
        graphValid = false;
    }

    if (graphValid) {
        let l = 0;
        Link.allLinks.forEach(link => {
            let l1 = link.source.name == '1' && link.target.name == '2' && link.name == 'a';
            let l2 = link.source.name == '1' && link.target.name == '3' && link.name == 'b';
            let l3 = link.source.name == '2' && link.target.name == '3' && link.name == 'a,b';
            let l4 = link.source.name == '3' && link.target.name == '2' && link.name == 'a,b';
            let l5 = link.source.name == '4' && link.target.name == '5' && link.name == 'a';
            let l6 = link.source.name == '4' && link.target.name == '6' && link.name == 'b';
            let l7 = link.source.name == '5' && link.target.name == '4' && link.name == 'a';
            let l8 = link.source.name == '5' && link.target.name == '6' && link.name == 'b';
            let l9 = link.source.name == '6' && link.target.name == '4' && link.name == 'b';
            let l10 = link.source.name == '6' && link.target.name == '5' && link.name == 'a';
            if (l1 || l2 || l3 || l4 || l5 || l6 || l7 || l8 || l9 || l10) l++;
        })
        if (l != 10) {
            var data : DialogData = {
                option: DialogDataOption.DISMISS,
                type : DialogDataType.TUTORIAL,
                content: "The graphs are not identical. Check the transitions. " + 
                        "Click on <b>Tutorial</b> to see the graph you should draw."
            };
            graphValid = false;
        }
    }

    if (graphValid) {
        let s = 0;
        State.allStates.forEach(state => {
            let s1 = (state.name == '1') && (state.value == '0');
            let s2 = (state.name == '2') && (state.value == '1');
            let s3 = (state.name == '3') && (state.value == '1');
            let s4 = (state.name == '4') && (state.value == '0');
            let s5 = (state.name == '5') && (state.value == '1');
            let s6 = (state.name == '6') && (state.value == '1');
            if (s1 || s2 || s3 || s4 || s5 || s6) s++;
        })
        if (s != 6) {
            var data : DialogData = {
                option: DialogDataOption.DISMISS,
                type : DialogDataType.TUTORIAL,
                content: "The graphs are not identical. Check the values for your states. If you need help, click on the notification icon next to the \'value\' field. " + 
                        "Click on <b>Tutorial</b> to see the graph you should draw."
            };
            graphValid = false;
        }
    }

    if (!graphValid) {
        this.dialog.open(DialogComponent, {
            data : data,
            width: '26vw'
        });
    }
    return graphValid;
  }

}
