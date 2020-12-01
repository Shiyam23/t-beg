import { Injectable } from '@angular/core';
import { Link, State } from 'src/app/graphModel';

@Injectable({
  providedIn: 'root'
})
export class AppProgressService {

  //TODO: set this to 0 later
  appProgress : number = 0;
  maxProgress : number = 5;
  selectedFunctor : string;
  validator : string;
  validatorErrorMessage : string;

  //Graph 
  graph : joint.dia.Graph;
  paper : joint.dia.Paper;
  selectedStateView : joint.dia.ElementView | joint.dia.LinkView;
  selectedItem : State | Link;
  isStateWindow : boolean = true;


  constructor() { }

  forward() : boolean {

    if (this.checkValidProgress) {
      (this.appProgress)++;
      return true
    }
    return false;
  }

  backward() : boolean {
    if (this.checkValidProgress) {
      (this.appProgress)--;
      return true
    }
    return false;
  }
  
  private checkValidProgress() : boolean {
    return this.appProgress >= 0 && this.appProgress < this.maxProgress
  }

}
