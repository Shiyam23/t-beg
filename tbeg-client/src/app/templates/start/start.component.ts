import { Component, Input, OnInit } from '@angular/core';
import { start } from 'repl';
import { AppProgressService } from 'src/app/services/appProgress/app-progress.service';
import { SignalRService } from 'src/app/services/signalR/signal-r.service';

@Component({
  selector: 'app-start',
  templateUrl: './start.component.html',
  styleUrls: ['./start.component.css']
})
export class StartComponent implements OnInit {

  constructor(public progress : AppProgressService, public signalR : SignalRService) { }

  @Input("text") text;
  @Input("click") click = () => this.progress.forward();

  ngOnInit(): void {
  }

  public start() {
    this.click();
  }
}
