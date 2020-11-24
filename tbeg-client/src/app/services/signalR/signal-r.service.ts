import { Injectable } from '@angular/core';
import * as SignalR from '@aspnet/signalr';
import { Subject } from 'rxjs';


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
}