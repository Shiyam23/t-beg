import { Component, AfterViewInit } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Subject, Subscription } from 'rxjs';
import { AppProgressService } from './services/appProgress/app-progress.service';
import { SignalRService } from './services/signalR/signal-r.service';

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
  private snackbarSub : Subscription;

  constructor(
    private service : SignalRService, 
    public progress: AppProgressService,
    private _snackBar: MatSnackBar
  ){}
  
  ngAfterViewInit(): void {
    
    this.snackbarSub = this.progress.snackbar.subscribe(string => {
      if (string == null) {
        this._snackBar.dismiss();
      } else {
        this._snackBar.open(string, "Close", {
          announcementMessage: string,
          duration: -1,
        })
      }
    })

    this.service.startConnection();
    this.connectedSub = this.service.connected.subscribe(connected => {
      this.connected = connected;
      if (connected) {
        this.progress.forward();
      }
    });
  }
}
