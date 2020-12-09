import { Injectable } from '@angular/core';
import * as SignalR from '@aspnet/signalr';
import { Subject } from 'rxjs';
import { Link, State } from 'src/app/graphModel';
import { Event, InfoEvent } from '../../eventModel';


@Injectable({
  providedIn: 'root'
})
export class SignalRService {private hubConnection: SignalR.HubConnection;

  public connected = new Subject<boolean>();
  public functorList = new Subject<Array<string>>();
  public validator = new Subject<Array<string>>();
  public initGameView = new Subject<boolean>();
  public gameSteps = new Subject<Event>();
  public info = new Subject<InfoEvent>();

  constructor() { }


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
      .catch(err => console.log('Error while starting connection' + err))
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
      this.validator.complete();
    })
  }

  public listenOnInitGameView() {
    this.hubConnection.on("InitGameView", () => {
      this.initGameView.next(true);
      this.initGameView.complete();
    })
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

  public listenToInfoText() {
    this.hubConnection.on("InfoText", (info : string, over: boolean, step : number, userIsSpoiler : boolean) => {
      this.info.next(new InfoEvent(info,over, step, userIsSpoiler))
    })
  }

  public sendStep(functor : string, selection : number , states : Array<Number>) {
    this.hubConnection.invoke("SendStep", functor.toString(), selection, states);
  }
}