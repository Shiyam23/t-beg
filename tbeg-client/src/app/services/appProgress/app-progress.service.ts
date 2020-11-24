import { Injectable } from '@angular/core';

@Injectable({
  providedIn: 'root'
})
export class AppProgressService {

  //TODO: set this to 0 later
  appProgress : number = 3;
  maxProgress : number = 5;
  selectedFunctor : String;

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
