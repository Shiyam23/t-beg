import { Component, ElementRef, OnDestroy, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { Observable, Subscription } from 'rxjs';
import { AppProgressService } from '../services/appProgress/app-progress.service';
import { SignalRService } from '../services/signalR/signal-r.service';
import { Event, InfoEvent, StepBackEvent } from '../eventModel';
import { State } from '../graphModel';
import { MatButtonToggleGroup } from '@angular/material/button-toggle';
import { MatDialog } from '@angular/material/dialog';
import { DialogData, DialogDataType, DialogComponent, DialogDataOption } from '../templates/dialog/dialog.component';

declare var MathJax : any;

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.css']
})
export class GameComponent implements OnInit, OnDestroy {
  

  gameSteps : Subscription;
  backSteps : Subscription;
  info : Subscription;
  actualStep : number;
  resetDisabled : boolean = true;
  backDisabled : boolean = true;
  startDisabled : boolean = true;
  selStatesString : Array<string> = ["{ - }","{ - }","{ - }","{ - }"];
  selStates : Array<Array<State>> = new Array<Array<State>>(4);
  x : number = Number(this.progress.stateNames[0]);
  y : number = Number(this.progress.stateNames[1]);
  disabled1 : boolean = true;
  disabled2 : boolean = true;
  tutorialStep : number;
  
  @ViewChild('selectedState') selectedState : MatButtonToggleGroup;
  @ViewChild('selectedPred') selectedPred : MatButtonToggleGroup;
  @ViewChildren('steps') steps : QueryList<ElementRef>;


  constructor(
    public signalR : SignalRService, 
    public progress : AppProgressService,
    private dialog : MatDialog
    ) {
  }
  

  ngOnInit(): void {
    if (this.progress.tutorial) {
      MathJax.Hub.Typeset();
      let data : DialogData = {
        type: DialogDataType.TUTORIAL,
        option: DialogDataOption.DISMISS,
        content:  "Welcome back! This is the game screen, where you can play the bisimulation game. " + 
                  "On the left side, you can see four steps. Step 1 and Step 3 are performed by the <b>Spoiler</b>. " +
                  "Step 2 and Step 4 are performed by the <b>Duplicator</b>. More information is shown, when you click the notification icon in the according step. " + 
                  " You chose the Duplicator role. " +
                  "The colored step shows the actual step, where violet implies Spoiler and turquoise implies Duplicator. " +
                  "<br><ol>" +
                    "<li>In step 1 the Spoiler selects a state by clicking the according one in the selection box. Additionally he selects a predicate by clicking the states on the canvas.</li>" +
                    "<li>In step 2 the Duplicator selects a predicate by clicking the states on the canvas.</li>" +
                    "<li>In step 3 the Spoiler selects the predicate by clicking the according one in the selection box. Additionally he selects a state by clicking it on the canvas.</li>" +
                    "<li>In step 4 the Duplicator selects a state by clicking it on the canvas.</li>" +
                  "</ol>" +
                  "On the canvas, some states are colored:" +
                    "<ul>" +
                      "<li>Outlined in violet: Selected state for Spoiler" +
                      "<li>Outlined in turquoise: Selected state for Duplicator" +
                      "<li>Filled with violet: Selected predicate Spoiler" +
                      "<li>Filled with turquoise: Selected predicate for Spoiler" +
                    "</ul>" +
                  "At the bottom left corner you see three buttons:" +
                  "<ol>" +
                  "<li>Left button: Restart Game</li>" +
                  "<li>Middle button: Take a step back</li>" +
                  "<li>Right button: Confirm move</li>" +
                  "</ol>" +
                  "",
      }
      this.progress.lastData = data;
      this.progress.lastWidth = '45vw'
      this.dialog.open(DialogComponent, {
        data: data,
        width: '45vw'
      }).afterClosed().subscribe(() => {
        this.prepareGame();
      })
    }
    else {
      MathJax.Hub.Typeset(this.prepareGame);
    }
  }

  private prepareGame = () => {

    this.tutorialStep = 1;
    this.signalR.listenToInfoStep();
    this.signalR.listenToInfoText();
    this.signalR.listenToStepBack();
    this.gameSteps = this.signalR.gameSteps.subscribe((event : Event) => {
      switch (event.step) {
        case 0:
          this.infoStep0(event); break;
          case 1:
          this.infoStep1(event); break;
        case 2:
          this.infoStep2(event); break;
        case 3:
          this.infoStep3(event); break;
        case 4:
          this.infoStep4(event); break;
      }
      this.highlightStep(this.actualStep);
    });
    this.backSteps = this.signalR.backSteps.subscribe((event : StepBackEvent) => this.stepBack(event));
    this.info = this.signalR.info.subscribe((event : InfoEvent) => {
      this.infoMessage(event);
    });
    this.signalR.initGame(
      this.progress.selectedFunctor,
      this.progress.stateNames,
      this.progress.isSpoiler
    )
  }

  ngOnDestroy(): void {
    this.gameSteps.unsubscribe();
    this.backSteps.unsubscribe();
    this.info.unsubscribe();
    this.signalR.stopListeningSteps();
  }

  public loadScript() {
    /* let body = <HTMLDivElement> document.body;
    let configuration = document.createElement('script');
    configuration.type = 'text/x-mathjax-config';
    configuration.innerHTML = 'MathJax.Hub.Config({ showMathMenu: false, tex2jax: {inlineMath: [[\"$\",\"$\"], [\"\\(\",\"\\)\"]],processEscapes: true}});';
    configuration.async = true;
    configuration.defer = true;
    body.appendChild(configuration);
    let sourceScript = document.createElement('script');
    sourceScript.innerHTML = '';
    sourceScript.src = 'https://cdnjs.cloudflare.com/ajax/libs/mathjax/2.7.7/MathJax.js?config=TeX-AMS_SVG';
    sourceScript.async = true;
    sourceScript.defer = true;
    body.appendChild(sourceScript); */
}

  infoMessage(event : InfoEvent) {

    this.closeSnackbar();
    let data : DialogData = {
      option : DialogDataOption.DISMISS,
      type : event.over ? DialogDataType.GAMEOVER : DialogDataType.ERROR,
      content : event.name
    }
    this.dialog.open(DialogComponent, {
      data: data,
    }).afterClosed().subscribe(() => {
      if (event.over && this.progress.tutorial) {
        this.tutorialStepMessage();
      };
    });
    if (!event.over) {
      this.disableControlButtons(false);
      this.progress.paper.trigger('blank:pointerclick')
    } else {
      this.startDisabled = true;
      this.resetDisabled = false;
      this.backDisabled = true;
    }
  }

  infoStep0(event : Event) {
    this.selectedState._buttonToggles.first.checked = true;
    this.resetMarkers();
    this.unbindGraphToArray();
    this.disabled2 = true;
    for (let i = 0; i < 4; i++) {
      this.selStates[i] = new Array<State>();
      this.updateLabel(i)
    }
    this.x = event.x;
    this.y = event.y;
    if (event.userIsSpoiler) {
      this.disabled1 = false;
      this.bindGraphToArray(0, false);
      this.disableControlButtons(false);
      this.closeSnackbar();
    }
    this.actualStep = 1;

  }
  infoStep1(event : Event) {
    this.checkFirstStateButton(this.x == event.selection);
    this.disabled1 = true;
    this.unbindGraphToArray();
    this.setMarkers(event);
    if (!event.userIsSpoiler) {
      let data : DialogData = {
        content: event.name,
        option: DialogDataOption.DISMISS,
        type: DialogDataType.INFO
      }
      this.dialog.open(DialogComponent, {
        data: data
      }).afterClosed().subscribe(() => {
        if (this.progress.tutorial) this.tutorialStepMessage();
      })
      this.bindGraphToArray(1, false);
      this.disableControlButtons(false);
      this.closeSnackbar();
    }
    this.actualStep = 2;
  }

  private tutorialStepMessage() {

    if (this.tutorialStep == 1) {
      var content = "You just saw a message from the server. It will notify you after each Step 1 (if it is the Spoiler). " + 
                    "It just told you that your next move in Step 2 has to hold the equation, which you can see on the left side in Step 2. " +
                    "<br><br>So our predicate has to hold (0,{(a,1),(b,1)}), which means the selected state (outlined in turquoise, so in our case 4) has to be a non-accepting state (which it is already). " + 
                    "Additionally your predicate has to be atleast a selection, which contains the a - successor and the b - successor of the selected state (4). " + 
                    "<br><br>So you need to <b>pick 5 and 6</b> and <b>confirm</b> the move.";
    }
    if (this.tutorialStep == 2) {
      var content = "Ok, now it picked state 2 in Step 3. Now you should select a bisimilar state. Let's <b>pick 5</b>!";
    }
    if (this.tutorialStep == 3) {
      var content = "Ok, now it picked state 2. And it picked 2,3,5 and 6 as a predicate. It also said that our predicate has to hold " + 
                    "(1,{(a,1),(b,1)}. That means our selected state (5) has to be an accepting state (which it is already). Additionally the " + 
                    "a - successor and b - successor of the selected state (5) have to be in our predicate. So we need to <b>pick 4 and 6</b>!";
    }
    if (this.tutorialStep == 4) {
      var content = "Ok, now it picked state 2 again. We have to pick a bisimilar state here. Let's see what happens when we select state 4...";
    }
    if (this.tutorialStep == 5) {
      var content = "Ok, now it picked state 2 ... again! And all the states for his predicate. The server said that our predicate has to hold " + 
                    "(1,{(a, 1),(b, 1)}), which means our selected state has to be an accepting state. <br><br>Wait a second! We picked state 4, which is not an " + 
                    "accepting state. So we can't fulfill this condition at all. We shouldn't have picked state 4. Thats your fault! Just <b>pick any states</b> " + 
                    "and finish this game.";
    }
    if (this.tutorialStep == 6) {
      var content = "We lost this game. But let's have a look. The server said that we lost this game because we didn't fulfill the condition. But 1 and 4 weren't " + 
                    "behavioural equivalent at all. It also gave us a distinguishing formula. " + 
                    "<br><br>Now you can restart this game, by clicking the button in the bottom left corner, and play this tutorial again. Or you can refresh this web app and play this game yourself without any 'help'.";
    }

    let data : DialogData = {
      type: DialogDataType.TUTORIAL,
      option: DialogDataOption.DISMISS,
      content:  content,
    };
    this.progress.lastData = data;
    this.progress.lastWidth = '45vw';
    this.dialog.open(DialogComponent, {
      data: data,
      width: '45vw'
    });
  }


  infoStep2(event : Event) {
    this.disabled1 = true;
    this.unbindGraphToArray();
    State.allStates.forEach(state => {
      event.pred1.forEach(
        item => {
          if (state.name == (item+1).toString()) {
            state.setColor("#e0ffff");
            if (event.userIsSpoiler) this.selStates[1].push(state);
          }
        }
      )
    })
    if (event.userIsSpoiler) {
      this.disabled2 = false;
      this.bindGraphToArray(2, true);
      this.disableControlButtons(false);
      this.closeSnackbar();
    }
    this.updateLabel(1);
    this.actualStep = 3;
  }

  infoStep3(event : Event) {
    this.checkFirstPredButton(event.selection == 0) ;
    this.disabled2 = true;
    this.unbindGraphToArray();
    if (!event.userIsSpoiler) {
      this.selStates[2].push(State.allStates.find(state => state.name == (event.pred1[0]+1).toString()));
      this.updateLabel(2);
      this.bindGraphToArray(3, true);
      this.disableControlButtons(false);
      this.closeSnackbar();
      if (this.progress.tutorial) this.tutorialStepMessage();
    }
    this.actualStep = 4;
  }

  infoStep4(event : Event) {
    this.unbindGraphToArray();
    if (event.userIsSpoiler) {
      this.selStates[3].push(State.allStates.find(state => state.name == (event.selection+1).toString()));
      this.updateLabel(3);
    }
  }

  stepBack(event : StepBackEvent) {
    if (event.x != null && event.y != null) {
      this.x = event.x;
      this.y = event.y;
      if (event.step == -3 || event.step == -4) {
        this.checkFirstStateButton(event.x == event.selection);
      }
      this.setMarkers(event);
    }
    /* if (event.pred2 != null) {
      State.allStates.forEach((state) => {
        event.pred2.forEach((item) => {
          if (Number(state.name) == item) this.selStates[1].push(state)
        })
      });
      this.updateLabel(1);
    } */
  }

  bindGraphToArray(i : number, singleItem : boolean) {
    this.unbindGraphToArray();
    this.progress.paper.on('blank:pointerclick', () => {
      this.resetSelection(i)
      this.selStates[i] = new Array<State>();
      this.updateLabel(i);
    })
    this.progress.paper.on('element:pointerclick', (cellView, event, x, y) => {
      if (!singleItem || this.selStates[i].length < 1) {
        var selectedState : State = State.findStateByModel(<joint.dia.Element>(cellView.model));
        if (this.selStates[i].indexOf(selectedState) == -1) {
          cellView.highlight(null, this.progress.highlighter);
          this.selStates[i].push(selectedState);
          this.updateLabel(i);
        }
      }
    })
  }

  unbindGraphToArray() {
    this.progress.paper.unbind('blank:pointerclick');
    this.progress.paper.unbind('element:pointerclick');
  }

  resetSelection(i : number) {
    this.selStates[i].forEach(state => {
      this.progress.paper.findViewByModel(state.model).unhighlight(null, this.progress.highlighter)
    })
  }

  updateLabel(i : number) {
    var string;
    if (this.selStates[i].length > 0) {
      string = 
        "{ " + 
        this.selStates[i]
        .map(state => state.name)
        .reduce((acc,value) => acc + ", " + value) + 
        " }";
    }
    else string = "{ - }"
    this.selStatesString[i] = string;
  }

  startClick = () => {
    var step = this.actualStep;
    var selection : number;
    if (step == 1) {
      selection = Number(this.selectedState.value);
    }
    else if (step == 3) selection = Number(this.selectedPred.value);
    else selection = 0;
    var states : Array<number> = this.selStates[step-1].map(state => Number(state.name)-1);
    if (this.progress.tutorial && !this.validTutorialMove(states)) return;
    if (states.length > 0) {
      this.signalR.sendStep(this.progress.selectedFunctor, selection, states);
      this.disableControlButtons(true);
      this.openSnackbar();
      this.resetSelection(step-1);
    }
    
  }

  private validTutorialMove(states : Array<number>) : boolean {
    let valid = true;
    let message = '';
    if (this.tutorialStep == 1) {
      if (states.length != 2 || !states.includes(4) || !states.includes(5)) {
            message = 'You need to <b>select 5 and 6</b>!'
            valid = false;
          } 
    }
    if (this.tutorialStep == 2) {
      if (!states.includes(4)) {
        message = 'Please <b>select 5</b> for this tutorial!'
        valid = false;
      } 
    }
    if (this.tutorialStep == 3) {
      if (states.length != 2 || !states.includes(3) || !states.includes(5)) {
            message = 'You need to <b>select 4 and 6</b>!'
            valid = false;
          } 
    }
    if (this.tutorialStep == 4) {
      if (!states.includes(3)) {
            message = 'State 4 looks interesting... <b>Select state 4</b>!'
            valid = false;
          } 
    }
    if (!valid) {
      let data : DialogData = {
        type: DialogDataType.TUTORIAL,
        option: DialogDataOption.DISMISS,
        content:  message,
      };
      this.dialog.open(DialogComponent, {
        data: data,
        width: '25vw'
      });
      return false;
    }
    this.tutorialStep++;
    return true;
  }
  
  backClick = () => {
    if (
      this.x == Number(this.progress.stateNames[0]) &&
      this.y == Number(this.progress.stateNames[1]) &&
      (this.actualStep == 1 || this.actualStep == 2)
    ) {
      let data : DialogData = {
        type: DialogDataType.ERROR,
        option: DialogDataOption.DISMISS,
        content: "You can not go back any further!"
      }
      this.dialog.open(DialogComponent, {
        data: data,
      })
      return;
    }
    this.resetMarkers();
    for (let i : number = 0; i < 4; i++) {
      this.selStates[i] = new Array<State>();
      this.updateLabel(i);
    }
    this.actualStep = (this.actualStep - 3) % 4;
    this.signalR.sendStepBack(this.progress.selectedFunctor);
    this.disableControlButtons(true);
    this.openSnackbar();
    this.tutorialStep--;
  }

  resetClick = () => {
    let data : DialogData = {
      type: DialogDataType.WARNING,
      option: DialogDataOption.ACCEPT,
      content: "If you reset the game you will go back to the game setup screen. Do you really want to reset?"
    }
    let dialogRef = this.dialog.open(DialogComponent, {
      data: data,
      disableClose: true
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result == "Accept") this.resetGame();
    })
  }

  resetGame = () => {
    this.resetMarkers();
    this.signalR.sendReset(this.progress.selectedFunctor);
    for (let i : number = 0; i < 4; i++) {
      this.selStates[i] = new Array<State>();
    }
    this.actualStep -= 0;
    this.progress.backward();
  }


  resetMarkers() {
    State.allStates.forEach(state => {
      if (state.name == this.x.toString() || state.name == this.y.toString())
        state.setStrokeColor("#333333"); 
    })
    if (this.selStates[0] != null && this.selStates[2] != null) {
      this.selStates[0].forEach(state => state.setColor('#ffffff'))
      this.selStates[1].forEach(state => state.setColor('#ffffff'))
    }
  }

  setMarkers(event: Event | StepBackEvent) {
    State.allStates.forEach(state => {
      if (this.selectedState._buttonToggles.first.checked) {
        if(state.name == this.x.toString()) state.setStrokeColor("#8b008b");
        if(state.name == this.y.toString()) state.setStrokeColor("#00ffff");
      }
      else {
        if(state.name == this.y.toString()) state.setStrokeColor("#8b008b");
        if(state.name == this.x.toString()) state.setStrokeColor("#00ffff");
      }
      event.pred1.forEach(
        item => {
          if(state.name == (item+1).toString()) {
            state.setColor("#dda0dd");
            if (!event.userIsSpoiler || event.step < 0) this.selStates[0].push(state);
          }
        }
      );
      if (event instanceof StepBackEvent && event.pred2 != null) {
        event.pred2.forEach(
          item => {
            if(state.name == (item+1).toString()) {
              state.setColor("#e0ffff");
              this.selStates[1].push(state);
            }
          }
        );
        this.updateLabel(1);
      }
    })
    this.updateLabel(0);
  }

  openSnackbar() {
    this.progress.snackbar.next("Waiting for Server ...");
  }

  closeSnackbar() {
    this.progress.snackbar.next(null);
  }

  disableControlButtons(bool: boolean) {
    this.backDisabled = bool;
    this.resetDisabled = bool;
    this.startDisabled = bool;
  }

  checkFirstStateButton(bool : boolean) {
    this.selectedState._buttonToggles.first.checked = bool;
    this.selectedState._buttonToggles.last.checked = !bool;
  }
  checkFirstPredButton(bool : boolean) {
    this.selectedPred._buttonToggles.first.checked = bool;
    this.selectedPred._buttonToggles.last.checked = !bool;
  }

  highlightStep(i : number) {
    this.steps.forEach((item : ElementRef, j) => {
      if (i - 1 == j) item.nativeElement.style.backgroundColor = j % 2 == 0 ? "#dda0dd" : "#e0ffff";
      else item.nativeElement.style.backgroundColor = "white"
    })
  }

  toolTipClick(step : number) {
    let content;
    switch (step) {
      case 0:
        content = "ts: Your transition system\n" +
                  "s: The state chosen for the Spoiler\n" +
                  "t: The state chosen for the Duplicator"
        break;
      case 1:
        content = "Spoiler chooses one state s and a predicate: X -> {0,1}, which corresponds to a "+
                  "subset of the state space X of your transition system."
        break;
      case 2:
        content = "The value Fp_1(ts(s)) in F{0,1} dervid from the move of the Spoiler has to be at "+
                  "least matched by the Duplicator. Otherwise the states are not bisimilar."
        break;
      case 3:
        content = "If the Duplicator was able to mimic the move in Step 2, Step 3 enables the Spoiler "+
                  "to show that the Duplicator has cheated in Step 2, i.e. D has included at least one state "+
                  "into p_2, such that no bisimilar state in p_1 exists."
                  "least matched by the Duplicator. Otherwise the states are not bisimilar."
        break;
      case 4:
        content = "Here the Duplicator tries to choose a bisimilar state y' to the one chosen by S in Step 3. "+
                  "If there is no such state available, D proceeds with another state and the game proceeds with "+
                  "a non-bisimilar state pair (x',y'). This means that S has a winning-strategy. If the game "+
                  "proceeds with a bismilar state-pair, D has a winning-strategy."
        break;
    }
    let data : DialogData = {
      content: content,
      option: DialogDataOption.DISMISS,
      type: DialogDataType.INFO
    }
    this.dialog.open(DialogComponent,{
      data : data
    });
  }

  
}
