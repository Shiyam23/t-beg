import { Component, AfterViewInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { SignalRService } from './services/signal-r.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements AfterViewInit  {
  
  private connected;
  public result;  
  private calcSubscription : Subscription;
  private resultSubscription : Subscription;

  constructor(private service : SignalRService ){}
  
  ngAfterViewInit(): void {
    this.service.startConnection();
    this.resultSubscription = this.service.result.subscribe(number => this.result = number)
    this.calcSubscription = this.service.connected.subscribe( connected => this.connected = connected)
  }

  login(value:any) {
    if (this.connected) {
      this.service.askServer(+value.a,+value.b);
      this.service.listenToServer();
    }
  }
}
