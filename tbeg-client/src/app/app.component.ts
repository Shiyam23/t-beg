import { Component, AfterViewInit } from '@angular/core';
import { Subscription } from 'rxjs';
import { AppProgressService } from './services/app-progress.service';
import { SignalRService } from './services/signal-r.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements AfterViewInit  {
  
  public title = "T-BEG";
  private connected;
  public result;  
  private calcSubscription : Subscription;
  private resultSubscription : Subscription;

  constructor(private service : SignalRService, public progress: AppProgressService){}
  
  ngAfterViewInit(): void {
    
    setTimeout(() => {
      this.progress.forward()
    }, 1500) 
    // this.service.startConnection();
    // this.resultSubscription = this.service.result.subscribe(number => this.result = number)
    // this.calcSubscription = this.service.connected.subscribe( connected => this.connected = connected)
  }

  login(value:any) {
    if (this.connected) {
      this.service.askServer(+value.a,+value.b);
      this.service.listenToServer();
    }
  }
}
