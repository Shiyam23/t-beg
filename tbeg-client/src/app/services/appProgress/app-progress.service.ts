import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { Link, State } from 'src/app/graphModel';
import { DialogData } from 'src/app/templates/dialog/dialog.component';

@Injectable({
  providedIn: 'root'
})
export class AppProgressService {

  //TODO: set this to 0 later
  private _appProgress : number = 0;
  private maxProgress : number = 5;

  //Tutorial properties
  tutorial : boolean = false;
  lastData : DialogData;
  lastWidth: string;
  lastHeight : string;

  availableFunctors : {value:string, viewValue:string}[] = new Array();
  selectedFunctor : string;

  //Functor
  selectedLabelArray : string[];
  stateValidator : string[];
  linkValidator : string[];

  //Graph 
  graph : joint.dia.Graph;
  paper : joint.dia.Paper;
  selectedStateView : joint.dia.ElementView | joint.dia.LinkView;
  selectedItem : State | Link;
  isStateWindow : boolean = true;
  highlighter : any;
  updateVertex : any;

  //Setup
  isSpoiler : boolean = true;
  initialPair : Array<State> = new Array<State>(); 

  //Game
  stateNames : Array<string>;
  snackbar : Subject<string> = new Subject();  

  constructor() { }

  forward() : boolean {

    if (this.checkValidProgress) {
      (this._appProgress)++;
      return true
    }
    return false;
  }

  backward() : boolean {
    if (this.checkValidProgress) {
      (this._appProgress)--;
      return true
    }
    return false;
  }

  toStart() : void {
    this._appProgress = 0;
  }

  get appProgress() {
    return this._appProgress;
  }
  
  private checkValidProgress() : boolean {
    return this.appProgress >= 0 && this.appProgress < this.maxProgress
  }

}
