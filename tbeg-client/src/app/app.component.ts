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
  private connectedSub : Subscription;

  constructor(private service : SignalRService, public progress: AppProgressService){}
  
  ngAfterViewInit(): void {
    
    
    this.service.startConnection();
    this.connectedSub = this.service.connected.subscribe( connected => {
      this.connected = connected;
      if (connected) {
        this.progress.forward();
      }
    });
  }
}
