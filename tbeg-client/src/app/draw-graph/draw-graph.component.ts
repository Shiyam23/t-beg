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
import { MatDialog } from '@angular/material/dialog';
import { DialogComponent } from '../templates/dialog/dialog.component';

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

    private signalR : SignalRService;
    progress : AppProgressService;
    private dialog : MatDialog;

    constructor(signalR : SignalRService, progress : AppProgressService, dialog : MatDialog) {
        this.signalR = signalR;
        this.progress = progress;
        this.dialog = dialog
    }
  

    ngOnInit(): void {


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
                    padding: 7,
                    rx:30,
                    ry:30
                }
            }
        }

        this.progress.highlighter = highlighter;

        this.paper.on('blank:pointerclick', (v,x,y) => {
            if (this.progress.selectedStateView) {
                let selectedBefore = this.progress.selectedStateView.model;
                if (selectedBefore.isLink()) {
                    Link.findLinkByModel(<joint.dia.Link>selectedBefore).select(false)
                }
                else {
                    this.progress.selectedStateView?.unhighlight(null, highlighter);
                }
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
            let selectedBefore = this.progress.selectedStateView;
            if (selectedBefore?.model.isLink()) {
                Link.findLinkByModel(<joint.dia.Link>selectedBefore.model).select(false);
            } else {
                selectedBefore?.unhighlight(null, highlighter)
            }
            this.progress.selectedStateView = cellView;
            this.progress.isStateWindow = true;
            cellView.highlight(null, highlighter);
            var state : State = State.findStateByModel(cellView.model);
            this.progress.selectedItem = state;
        });
        
        this.paper.on('link:pointerclick', (cellView) => {
            var link : Link = Link.findLinkByModel(cellView.model);
            let selectedBefore = this.progress.selectedStateView;
            if (selectedBefore?.model.isLink()) {
                Link.findLinkByModel(<joint.dia.Link>selectedBefore.model).select(false);
            } else {
                selectedBefore?.unhighlight(null, highlighter)
            }
            link.select(true);
            this.progress.selectedStateView = cellView;
            this.progress.isStateWindow = false;
            this.progress.selectedItem = link;
            this.progress.selectedLabelArray = link.name.split(',');
        });

        this.progress.updateVertex = this.updateVertex;

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
        var link : Link = new Link(label,sourceState, targetState,[''], linkModel);
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
                    strokeWidth: 3,
                }
            }
        });
        var state : State = new State(label, circle, '');
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
            },
            
        ];
        let link : Link = Link.findLinkByModel(view.model);
        if (link.source === link.target) {
            this.contextmenuList.push(
                {
                    option:"Rotate Loop",
                    function: () => link.rotate()
                }, 
            )
        } 
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
                let linkModel = this.addLink(selectedState, clickedState, this.label, vertices);
                
                if (selectedState == clickedState) {
                    linkModel.connector('smooth');
                    selectedState.embed(linkModel);
                }
                else {
                    let link = Link.findLinkByModel(linkModel);
                    let reverseLink = Link.allLinks
                    .find(link => link.target.model == selectedState 
                            && link.source.model == clickedState);
                    if (reverseLink != null) {
                        let vertexHandler = this.progress.updateVertex(reverseLink.model, linkModel, selectedState, clickedState);
                        vertexHandler();
                        link.vertexHandler = vertexHandler;
                        reverseLink.vertexHandler = vertexHandler;
                        selectedState.on('change:position', vertexHandler);
                        clickedState.on('change:position', vertexHandler);
                        linkModel.connector('smooth');
                        reverseLink.model.connector('smooth');

                    }
                }
            }
        }
    }

    updateVertex = (link1:joint.dia.Link, link2:joint.dia.Link, state1:joint.dia.Element, state2:joint.dia.Element) => {
        return () => {
            let line = new joint.g.Line(link2.getSourcePoint(), link2.getTargetPoint());
            let vertex1 = line.rotate(line.midpoint(), 90).pointAtLength(line.length()/2-30);
            let vertex2 = line.rotate(line.midpoint(), 180).pointAtLength(line.length()/2-30);
            link1.vertices([vertex1]);
            link2.vertices([vertex2]);
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
        if (link.vertexHandler) {
            let vertexHandler = link.vertexHandler;
            link.source.model.off('change:position', vertexHandler);
            link.target.model.off('change:position', vertexHandler);
            let reverseLink = Link.allLinks.find(link2 => 
                link.source == link2.target
                && link.target == link2.source
                && link != link2);
            reverseLink?.model.vertices([]);
            reverseLink?.model.connector('normal');
            link.vertexHandler = null;
            reverseLink.vertexHandler = null;
        }
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

    lastTutData = () => {
        let data = this.progress.lastData;
        let width : string = this.progress.lastWidth;
        let height = this.progress.lastHeight;
        if (this.progress.lastData != null) {
            this.dialog.open(DialogComponent, {
                data: data,
                width: width,
                height: height
              })
        }
    }

}

