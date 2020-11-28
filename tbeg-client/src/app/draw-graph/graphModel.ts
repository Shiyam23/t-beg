export class State {


    static allStates : Array<State> = new Array<State>();
    public connections: Array<Connection> = new Array<Connection>();

    constructor(
        private _name: string, 
        public model: joint.dia.Element,
        private _isStartState: boolean, 
        private _isFinalState: boolean
    ) {
        State.allStates.push(this);
    }

    public addLink(link:Link, target: State) {
        this.connections.push({link: link, target: target});
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


    public remove(){
        this.model.remove();
        this.connections.forEach(connection => {
            connection.link.model.remove();
            var labelIndex : number = Link.allLinks.indexOf(connection.link);
            if (labelIndex > -1) Link.allLinks.splice(labelIndex, 1);
        })
        State.allStates.forEach(state => {
            var index : number;
            state.connections.forEach(connection => {
                if (connection.target == this) {
                    connection.link.model.remove();
                    var labelIndex : number = Link.allLinks.indexOf(connection.link);
                    if (labelIndex > -1) Link.allLinks.splice(labelIndex, 1);
                    index = state.connections.indexOf(connection);
                }
            if (index > -1) state.connections.splice(index, 1);
            });
        });
        var index : number = State.allStates.indexOf(this);
        if (index > -1) State.allStates.splice(index, 1);
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
    ) {
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
        const index = Link.allLinks.indexOf(this);
        if (index > -1) {
            Link.allLinks.splice(index, 1);
        }
        State.allStates.forEach(state => {
            var index : number = state.connections.findIndex(connection => connection.link = this)
            if (index > -1) state.connections.splice(index, 1);
        });
    }

    public static findLinkByModel(model:joint.dia.Link) {
        return Link.allLinks.find(item => item.model == model);
    }
}

class Connection {

    constructor(public link : Link, public target : State) {}
}