import { Component, ElementRef, Input, OnInit, ViewChild} from '@angular/core';
import * as jQuery from 'jquery';
import * as _ from 'lodash';
import * as $ from 'backbone';
import * as joint from 'jointjs';
import { SignalRService } from '../services/signalR/signal-r.service';
import { FormControl, Validators } from '@angular/forms';
import { ContextMenuComponent } from '../templates/context-menu/context-menu.component';
import { State, Link } from '../graphModel'
import { AppProgressService } from '../services/appProgress/app-progress.service';
import { saveAs } from 'file-saver';

@Component({
  selector: 'app-draw-graph',
  templateUrl: './draw-graph.component.html',
  styleUrls: ['./draw-graph.component.css', ]
})
export class DrawGraphComponent implements OnInit{


    showContextMenu : String;
    graph : joint.dia.Graph;
    paper : joint.dia.Paper;
    label : string = 'a';
    event : any;


    @ViewChild('menu')
    cmenu: ContextMenuComponent;

    
    
    contextmenuList : Array<{
        option:string,
        function:Function
      }>;

    signalR : SignalRService;
    progress : AppProgressService;
    validator : RegExp;
    errorMessage : string;

    constructor(signalR : SignalRService, progress : AppProgressService) {
        this.signalR = signalR;
        this.progress = progress;
        this.validator = new RegExp(progress.validator);
        this.errorMessage = progress.validatorErrorMessage;
    }
  

    ngOnInit(): void {

        console.log(this.progress.appProgress);

        if (!this.graph)
        this.graph = new joint.dia.Graph({}, {cellNamespace: joint.shapes});

        if (!this.paper)
        this.paper = new joint.dia.Paper({
        el: document.getElementById("paper"),
        width: "70vw",
        height: "35vw",
        model: this.graph,
        cellViewNamespace: joint.shapes,
        defaultConnector: { name: 'normal' },
        interactive: { 
            addLinkFromMagnet: false,
            linkMove: false,
            useLinkTools: false,
            elementMove: true,
            arrowheadMove: false,
            labelMove: false,
        },
        frozen: true
        });

        this.progress.graph = this.graph;
        this.progress.paper = this.paper;


        let highlighter = {
            highlighter: {
                name: 'stroke',
                options: {
                    rx:30,
                    ry:30
                }
            }
        }

        this.progress.highlighter = highlighter;

        this.paper.on('blank:pointerclick', (v,x,y) => {
            if (this.progress.selectedStateView) {
                this.progress.selectedStateView?.unhighlight(null, highlighter);
                this.progress.selectedStateView = null;
                this.progress.selectedItem = null;
            }
        });

        this.paper.on('blank:contextmenu', (event,x,y) => {
            if (this.progress.appProgress == 3)
            this.openBlankMenu(event, x, y);
        });

        this.paper.on('element:contextmenu', (view, event,x,y) => {
            if (this.progress.appProgress == 3)
            this.openStateMenu(event,view, x, y);
        })

        this.paper.on('link:contextmenu', (view, event,x,y) => {
            if (this.progress.appProgress == 3)
            this.openLinkMenu(event,view, x, y);
        })

        this.paper.on('element:pointerclick', (cellView) => {
            this.progress.selectedStateView?.unhighlight(null, highlighter)
            this.progress.selectedStateView = cellView;
            this.progress.isStateWindow = true;
            cellView.highlight(null, highlighter);
            var state : State = State.findStateByModel(cellView.model);
            this.progress.selectedItem = state;
        });
        
        this.paper.on('link:pointerclick', (cellView) => {
            this.progress.selectedStateView?.unhighlight(null, highlighter)
            this.progress.selectedStateView = cellView;
            this.progress.isStateWindow = false;
            cellView.highlight(null, highlighter);
            var link : Link = Link.findLinkByModel(cellView.model);
            this.progress.selectedItem = link;
        });

        this.paper.unfreeze();
    }

    addLink(source, target, label, vertices) {
        var linkModel = new joint.shapes.standard.Link({
            source: { id: source.id },
            target: { id: target.id },
            attrs: {
                line: {
                    strokeWidth: 2
                }
            },
            labels: [{
                position: {
                    distance: 0.5,
                    offset: 10,
                    args: {
                        keepGradient: true,
                        ensureLegibility: true
                    }
                },
                attrs: {
                    text: {
                        text: label,
                        fontWeight: 'bold'
                    }
                }
            }],
            vertices: vertices
        });
        var sourceState = State.findStateByModel(source);
        var targetState = State.findStateByModel(target);
        var link : Link = new Link(label,sourceState, targetState,"", linkModel);
        return linkModel.addTo(this.graph);
    }   

    state(x, y, label) {
        var circle = new joint.shapes.standard.Circle({
            position: { x: x, y: y },
            size: { width: 60, height: 60 },
            attrs: {
                label : {
                    text: label,
                    event: 'element:label:pointerdown',
                    fontWeight: 'bold',
                },
                body: {
                    strokeWidth: 2,
                }
            }
        });
        var state : State = new State(label, circle, false, false);
        circle.addTo(this.graph);
        return state;
    }

    
    openBlankMenu(event: MouseEvent, x:number, y:number) {
        event.preventDefault();
        this.contextmenuList = [
            {
                option:"Add state",
                function: () => {
                    var name = this.nextSlot();
                    this.state(x-30, y-30, name);
                }
            }
        ];
        this.cmenu.onContextMenu(event);
    }

    openStateMenu(event: MouseEvent,view: joint.dia.ElementView, x:number, y:number) {
        event.preventDefault();
        var contextmenuList = []

        if (this.progress.selectedStateView) {
            contextmenuList.push({
                option:"Add Link to here",
                function: () => this.addLinktoClickedState(view)
            });
        }
        contextmenuList.push({
            option:"Remove State",
            function: () => this.removeState(view)
        });
        this.contextmenuList = contextmenuList;
        this.cmenu.onContextMenu(event);
    }

    openLinkMenu(event: MouseEvent,view: joint.dia.LinkView, x:number, y:number) {
        event.preventDefault();
        this.contextmenuList = [
            {
                option:"Remove Link",
                function: () => this.removeLink(view)
            }
        ];
        this.cmenu.onContextMenu(event);
    }

    addLinktoClickedState(view: joint.dia.ElementView) {
        if (this.progress.selectedStateView && this.progress.selectedStateView instanceof joint.dia.ElementView) {

            var selectedState = this.progress.selectedStateView.model;
            var clickedState = view.model;
            var neigh = this.graph.isNeighbor(<joint.dia.Element>selectedState, clickedState, {
                deep: false,
                outbound: true,
                inbound: false,
                indirect: false
            });

            if (!neigh) {
                var vertices = [] 
                if (selectedState == clickedState) {
                    var position = view.model.attributes.position;
                    vertices = [
                        {x: position.x, y: position.y-45},
                        {x: position.x+60, y: position.y-45}
                    ];
                }
                var link = this.addLink(selectedState, clickedState, this.label, vertices);

                if (selectedState == clickedState) {
                    link.connector('smooth');
                    selectedState.embed(link);
                }
            }
        }
    }

    removeState(view: joint.dia.ElementView) {
        if (view == this.progress.selectedStateView) {
            this.progress.selectedStateView = null;
        };
        var state = State.findStateByModel(view.model);
        state.remove();
    }

    removeLink(view: joint.dia.LinkView) {
        if (view == this.progress.selectedStateView) {
            this.progress.selectedStateView = null;
        };
        var link = Link.findLinkByModel(view.model);
        link.remove();
    }

    nextSlot() {
        var list : Array<State> = State.allStates;
        for (let index = 0; index < list.length; index++) {
            var listIndex = list.findIndex(state => state.name == (index+1).toString())
            if (listIndex == -1) return (index+1).toString()
        }
        return (list.length+1).toString();
    }

    

}

