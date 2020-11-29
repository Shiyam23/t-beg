import { Injectable } from '@angular/core';
import * as SignalR from '@aspnet/signalr';
import { linkSync } from 'fs';
import { Subject } from 'rxjs';
import { Link, State } from 'src/app/draw-graph/graphModel';


@Injectable({
  providedIn: 'root'
})
export class SignalRService {private hubConnection: SignalR.HubConnection;

  public connected = new Subject<boolean>();
  public functorList = new Subject<Array<string>>();

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
    })
  }

  public sendGraph(states : Array<State>, links : Array<Link> , alphabet : Array<string>) {
    this.hubConnection.invoke("graph", states, links, alphabet)
  }

  public askValidator(functor : string) {
    console.log("getting Validator");
    this.hubConnection.invoke("getValidator", functor)
  }

  public getValidator() {
    this.hubConnection.on("Validator", (validator : string) => {
      console.log(validator);
    })
  }
}