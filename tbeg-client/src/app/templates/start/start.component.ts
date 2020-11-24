import { Component, Input, OnInit } from '@angular/core';
import { AppProgressService } from 'src/app/services/appProgress/app-progress.service';

@Component({
  selector: 'app-start',
  templateUrl: './start.component.html',
  styleUrls: ['./start.component.css']
})
export class StartComponent implements OnInit {

  constructor(public progress : AppProgressService) { }

  @Input("text") text;
  
  ngOnInit(): void {
  }

  public start() {
    this.progress.forward();
  }
}
