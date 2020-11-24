import { Component, ContentChild, OnInit, ViewChild} from '@angular/core';
import * as jQuery from 'jquery';
import * as _ from 'lodash';
import * as $ from 'backbone';
import * as joint from 'jointjs';
import { SignalRService } from '../services/signalR/signal-r.service';
import { MatMenuTrigger } from '@angular/material/menu';
import { ContextMenuComponent } from '../templates/context-menu/context-menu.component';
import { state } from '@angular/animations';
import { element } from 'protractor';

@Component({
  selector: 'app-draw-graph',
  templateUrl: './draw-graph.component.html',
  styleUrls: ['./draw-graph.component.css', ]
})
export class DrawGraphComponent implements OnInit{


    states : joint.shapes.basic.Circle[] = [];
    loopLinks : joint.dia.Link[] = [];
    showContextMenu : String;
    selectedState;
    graph;
    paper;
    label = 1;

    @ViewChild('menu')
    cmenu: ContextMenuComponent;
    

    contextmenuList : Array<{
        option:string,
        function:Function
      }>;
    
    constructor(public signalR : SignalRService) { }
  

    ngOnInit(): void {

        this.graph = new joint.dia.Graph;

        this.paper = new joint.dia.Paper({
        el: document.getElementById("paper"),
        width: "70vw",
        height: "35vw",
        model: this.graph,
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

        let highlighter = {
            highlighter: {
                name: 'stroke',
                options: {
                    rx:30,
                    ry:30
                }
            }
        }

        this.paper.on('blank:pointerclick', (v,x,y) => {
            this.selectedState?.unhighlight(null, highlighter);
        });

        this.paper.on('blank:contextmenu', (event,x,y) => {
            this.openBlankMenu(event, x, y);
        })

        this.paper.on('element:contextmenu', (view, evt, x, y) => {
            if (this.selectedState) {


                // are they neighbors? Then dont add another link
                var neigh = this.graph.isNeighbor(this.selectedState.model, view.model, {
                    deep: false,
                    outbound: true,
                    inbound: false,
                    indirect: false
                });

                if (!neigh) {
                    var vertices = [] 
                    if (this.selectedState.model == view.model) {
                        var position = view.model.attributes.position;
                        vertices = [
                            {x: position.x, y: position.y-45},
                            {x: position.x+60, y: position.y-45}
                        ];
                    }
                    var link = this.addLink(this.selectedState.model, view.model, this.label++, vertices);
                    if (this.selectedState.model == view.model) link.connector('smooth');
                }
            }
        })

        this.paper.on('cell:pointerclick', (cellView) => {
            this.selectedState?.unhighlight(null, highlighter)
            this.selectedState = cellView;
            cellView.highlight(null, highlighter);
        });

        this.paper.unfreeze();
    }
        
    initState(x, y) {
        var start = new joint.shapes.standard.Circle({
            position: { x: x, y: y },
            size: { width: 20, height: 20 },
            attrs: {
                body: {
                    fill: '#333333'
                }
            }
        });
        return start.addTo(this.graph);
    }

    addLink(source, target, label, vertices) {
        var link = new joint.shapes.standard.Link({
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
        return link.addTo(this.graph);
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
                    strokeWidth: 3
                }
            }
        });
        circle.on('change:position', (circle, pos) => {
            var list : Array<joint.dia.Link> = this.graph.getConnectedLinks(circle);
            list
            .filter(link => link.hasLoop())
            .forEach(link => {
                link.vertices([
                    {x: pos.x, y: pos.y-45},
                    {x: pos.x+60, y: pos.y-45}
                ])
            });
        })
        return circle.addTo(this.graph);
    }

    
    openBlankMenu(event: MouseEvent, x:number, y:number) {
        event.preventDefault();
        this.contextmenuList = [
            {
                option:"Add state",
                function: () => {
                    var len = this.states.length.toString()
                    this.states.push(this.state(x-30, y-30, len))
                }
            }
        ];
        this.cmenu.onContextMenu(event);
    }
}
