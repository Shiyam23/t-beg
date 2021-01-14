import { util } from 'jointjs';

export class State {

    static allStates : Array<State> = new Array<State>();

    constructor(
        private _name: string, 
        public model: joint.dia.Element,
        private _isStartState: boolean, 
        private _isFinalState: boolean
    ) {
        State.allStates.push(this);
    }

    public setName(name:string) {
        this._name = name;
        this.model.attr({
            label: {
                text: name
            }
        })
    }

    public setColor(color: string) {
        this.model.transition('attrs/body/fill', color, {
            valueFunction: util.interpolate.hexColor,
            timingFunction: util.timing.linear,
            duration: 500
        })
        /* this.model.attr({
            body: {
                fill: color
            }
        }) */
    }

    public setStrokeColor(color:string) {
        this.model.attr({
            body: {
                stroke: color,
            }
        })
    }

    public remove(){
        this.model.remove();
        Link.allLinks = Link.allLinks.filter( link => link.source.name != this.name && link.target.name != this.name);
        var stateIndex : number = State.allStates.indexOf(this);
        if (stateIndex > -1) State.allStates.splice(stateIndex, 1);
    }

    public get name() : string {
        return this._name;
    }
    
    public static findStateByModel(model:joint.dia.Element) {
        return State.allStates.find(item => item.model == model)
    }

    public setFinalState(isFinalState : boolean) {
        this._isFinalState = isFinalState;
        this.model.attr({
            body: {
                strokeDasharray: isFinalState? 10 : 0
            }
        })
    }

    public setStartState(isStartState : boolean) {
        this._isStartState = isStartState;
        this.model.attr({
            body: {
                fill: isStartState? 'green' : 'white'
            }
        })
    }

    public get isStartState() : boolean {
        return this._isStartState;
    }


    public get isFinalState() : boolean {
        return this._isFinalState
    }

    toJSON() {

        return {
            name: Number.parseInt(this._name),
            isStartState: this._isStartState,
            isFinalState: this._isFinalState,
        }
    }
    
}

export class Link {

    static allLinks : Array<Link> = new Array<Link>();

    constructor(
        private _name: string,
        public source : State,
        public target : State,
        public value: string | string[], 
        public model: joint.dia.Link | null ,
    ) {
        if (model != null) Link.allLinks.push(this);
    }
    
    public get name() : string {
        return this._name;
    }
    
    public setName(name:string) {
        this._name = name;
        this.model.label(0, {
            attrs: {
                text: {
                    text: name
                }
            }
        })
    }

    public remove() {
        this.model.remove();
        const index = Link.allLinks.indexOf(this);
        if (index > -1) {
            Link.allLinks.splice(index, 1);
        }
    }

    public static findLinkByModel(model:joint.dia.Link) {
        return Link.allLinks.find(item => item.model == model);
    }

    toJSON() {
        return {
            name: this._name.toString(),
            source: this.source,
            target: this.target,
            value: this.value
        }
    }
}