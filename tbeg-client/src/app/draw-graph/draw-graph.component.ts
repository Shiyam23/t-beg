import { Component, OnInit} from '@angular/core';
import * as jQuery from 'jquery';
import * as _ from 'lodash';
import * as $ from 'backbone';
import * as joint from 'jointjs';

@Component({
  selector: 'app-draw-graph',
  templateUrl: './draw-graph.component.html',
  styleUrls: ['./draw-graph.component.css', ]
})
export class DrawGraphComponent implements OnInit {

    states : joint.shapes.basic.Circle[] = [];
    showContextMenu : String;
    selectedState;

    constructor() { }
  

    ngOnInit(): void {

        let graph = new joint.dia.Graph;

        let paper = new joint.dia.Paper({
        el: document.getElementById("paper"),
        width: "70vw",
        height: "35vw",
        model: graph,
        defaultConnector: { name: 'smooth' },
        interactive: { 
            linkMove: false,
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

        paper.on('blank:pointerclick', (v,x,y) => {
            console.log("test");
            this.selectedState?.unhighlight(null, highlighter);
            //this.states.push(state(x-30,y-30,this.states.length+1))
        });

        paper.on('blank:contextmenu', (_,x,y) => {
            console.log('test2')
        })

        
    
        paper.on('element:pointerclick', (cellView) => {
            this.selectedState?.unhighlight(null, highlighter)
            this.selectedState = cellView;
            cellView.highlight(null, highlighter);
        });


        function state(x, y, label) {
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
            return circle.addTo(graph);
        }

        function initState(x, y) {
            var start = new joint.shapes.standard.Circle({
                position: { x: x, y: y },
                size: { width: 20, height: 20 },
                attrs: {
                    body: {
                        fill: '#333333'
                    }
                }
            });
            return start.addTo(graph);
        }

        function addLink(source, target, label, vertices) {
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
            return link.addTo(graph);
        }   

        var test = state(100,100, 'test');
        var test2 = state(200,100, 'test2');
        addLink(test, test2, "testLink", []);

        paper.unfreeze();


    }
}
