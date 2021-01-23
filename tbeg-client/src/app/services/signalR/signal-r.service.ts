import { Injectable } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import * as SignalR from '@aspnet/signalr';
import { Subject } from 'rxjs';
import { Link, State } from 'src/app/graphModel';
import { DialogDataType, DialogData, DialogComponent, DialogDataOption } from 'src/app/templates/dialog/dialog.component';
import { Event, InfoEvent, StepBackEvent } from '../../eventModel';
import { AppProgressService } from '../appProgress/app-progress.service';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: SignalR.HubConnection;

  public connected = new Subject<boolean>();
  public functorList = new Subject<Array<string>>();
  public validator = new Subject<Array<string>>();
  public initGameView = new Subject<boolean>();
  public gameSteps = new Subject<Event>();
  public backSteps = new Subject<StepBackEvent>();
  public info = new Subject<InfoEvent>();

  constructor(
    private progress : AppProgressService,
    private dialog : MatDialog) { }

  public startConnection() {

    this.hubConnection = new SignalR.HubConnectionBuilder()
      .withUrl('https://localhost:5001/api', {
        skipNegotiation: true,
        transport: SignalR.HttpTransportType.WebSockets
      })
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('Hub connection started');
        this.connected.next(true);
        this.connected.complete();
      })
      .catch(_ => this.showError("Error occurred while connecting to Server!\nPlease reload or try again later..."));
      this.hubConnection.onclose( _ => {
        this.progress.toStart();
        this.showError("Connection Error! Please reload this Web Application")
      });
  } 

  public askServer() {
    this.hubConnection.invoke("getFunctors")
      .catch(err => console.log(err));
  }

  public listenToServer() {
    this.hubConnection.on("Result", (functorArray : any) => {
      this.functorList.next(functorArray);
      this.functorList.complete();
    })
  }

  public sendGraph(states : Array<State>, links : Array<Link> , alphabet : Array<string>, functor : string) {
    this.hubConnection.invoke("graph", states, links, alphabet, functor)
  }

  public askValidator(functor : string) {
    this.hubConnection.invoke("getValidator", functor)
  }

  public getValidator() {
    this.hubConnection.on("Validator", (validator : string, message : string) => {
      this.validator.next([validator, message]);
    })
  }

  public stopGetValidator() {
    this.hubConnection.off("Validator");
  }

  public listenOnInitGameView() {
    this.hubConnection.on("InitGameView", () => {
      this.initGameView.next(true);
    })
  }

  public stopListenInitGameView() {
    this.hubConnection.off("InitGameView");
  }

  public initGame(functor : string, initialPair : Array<string>, spoiler : boolean) {
    this.hubConnection.invoke("InitGame", functor, initialPair, spoiler);
  }

  public listenToInfoStep() {
    this.hubConnection.on("InfoStep", (name, pred1, selection, userIsSpoiler, x, y, step) => {
      this.gameSteps.next(new Event(
        name, pred1, selection,
        userIsSpoiler, x, y, step
      ))
    })
  }

  public stopListeningSteps() {
    this.hubConnection.off("InfoStep");
    this.hubConnection.off("InfoText");
    this.hubConnection.off("Stepback");
  }

  public listenToInfoText() {
    this.hubConnection.on("InfoText", (info : string, over: boolean, step : number, userIsSpoiler : boolean) => {
      this.info.next(new InfoEvent(info,over, step, userIsSpoiler))
    })
  }

  public sendStep(functor : string, selection : number , states : Array<Number>) {
    this.hubConnection.invoke("SendStep", functor.toString(), selection, states);
  }

  public sendStepBack(functor : string) {
    this.hubConnection.invoke("SendStepBack", functor.toString());
  }

  public listenToStepBack() {
    this.hubConnection.on("StepBack", (name, pred1, pred2, selection, userIsSpoiler, x, y, step) => {
      this.backSteps.next(new StepBackEvent(
        name, pred1, pred2, selection,
        userIsSpoiler, x, y, step
      ))
    });
  }

  public sendReset(functor: string) {
    this.hubConnection.invoke("SendReset", functor.toString());
  }

  private showError(msg : string) {
    var data : DialogData = {
      option: DialogDataOption.DISMISS,
      type : DialogDataType.ERROR,
      content : msg
    }
    this.dialog.open(DialogComponent, {
      data: data,
      disableClose: true
    })
  }


}