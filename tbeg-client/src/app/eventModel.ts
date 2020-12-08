export class Event {
    
    name : string;
    pred1 : number[];
    selection : number;
    userIsSpoiler : boolean;
    x : number;
    y : number;
    step : number;

    constructor (name : string, pred1 : number[], selection : number, userIsSpoiler : boolean,x : number, y : number, step : number) {
        this.name = name;
        this.pred1 = pred1;
        this.selection = selection;
        this.userIsSpoiler = userIsSpoiler;
        this.x = x;
        this.y = y;
        this.step = step;
    }
}
