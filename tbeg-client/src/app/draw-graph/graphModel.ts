export class State {

    static allStates : Array<State> = new Array<State>();
    private fromLinks: Array<Link> = new Array<Link>();
    private toLinks: Array<Link> = new Array<Link>();

    constructor(
        private _name: string, 
        public model: joint.dia.Element,
        private _isStartState: boolean, 
        private _isFinalState: boolean
    ) {
        State.allStates.push(this);
    }

    public addToLink(link:Link) {
        this.toLinks.push(link);
    }

    public addFromLink(link:Link) {
        this.fromLinks.push(link);
    }

    public setName(name:string) {
        this._name = name;
        this.model.attr({
            label: {
                text: name
            }
        })
    }

    public setColor(color:string) {
        this.model.attr({
            body: {
                fill: color
            }
        })
    }

    public removeLink(link:Link) {
        const index = this.fromLinks.indexOf(link);
        if (index > -1) {
            this.fromLinks.splice(index, 1);
          }
        else {
            const index2 = this.toLinks.indexOf(link);
            if (index2 > -1) {
                this.toLinks.splice(index, 1);
            }
        }
    }

    public remove(){
        this.model.remove();
        this.fromLinks.forEach(link => link.remove())
        this.toLinks.forEach(link => link.remove())
        const index = State.allStates.indexOf(this);
        if (index > -1) {
            State.allStates.splice(index, 1);
          }
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

    
}

export class Link {

    static allLinks : Array<Link> = new Array<Link>();

    constructor(
        private _name: string, 
        public value: string, 
        public model: joint.dia.Link ,
        public sourceState: State,
        public sinkState: State
    ) {
        this.sourceState.addFromLink(this);
        this.sinkState.addToLink(this);
        Link.allLinks.push(this);
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
        this.sourceState.removeLink(this);
        this.sinkState.removeLink(this);
        const index = Link.allLinks.indexOf(this);
        if (index > -1) {
            Link.allLinks.splice(index, 1);
          }
    }

    public static findLinkByModel(model:joint.dia.Link) {
        return Link.allLinks.find(item => item.model == model);
    }
}