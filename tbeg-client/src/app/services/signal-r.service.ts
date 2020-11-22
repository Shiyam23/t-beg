import { Injectable } from '@angular/core';
import * as SignalR from '@aspnet/signalr';
import { Subject } from 'rxjs';


@Injectable({
  providedIn: 'root'
})
export class SignalRService {private hubConnection: SignalR.HubConnection;
  public connected = new Subject<boolean>();
  public result = new Subject<number>();

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

  public askServer(a: number, b: number) {

    this.hubConnection.invoke("sum", a, b)
      .catch(err => console.log(err));

    this.hubConnection.stream
  }

  public listenToServer() {
    this.hubConnection.on("Result", (number : number) => {
      this.result.next(number);
    })
  }
}