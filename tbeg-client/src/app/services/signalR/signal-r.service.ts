import { Injectable } from '@angular/core';
import * as SignalR from '@aspnet/signalr';
import { linkSync } from 'fs';
import { Subject } from 'rxjs';
import { Link, State } from 'src/app/graphModel';


@Injectable({
  providedIn: 'root'
})
export class SignalRService {private hubConnection: SignalR.HubConnection;

  public connected = new Subject<boolean>();
  public functorList = new Subject<Array<string>>();
  public validator = new Subject<Array<string>>();
  public initGameView = new Subject<boolean>();

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
}