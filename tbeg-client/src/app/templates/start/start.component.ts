import { Component, Input, OnInit } from '@angular/core';
import { start } from 'repl';
import { AppProgressService } from 'src/app/services/appProgress/app-progress.service';

@Component({
  selector: 'app-start',
  templateUrl: './start.component.html',
  styleUrls: ['./start.component.css']
})
export class StartComponent implements OnInit {

  constructor(public progress : AppProgressService) { }

  @Input("text") text;
  @Input("click") click = () => this.progress.forward();

  ngOnInit(): void {
  }

  public start() {
    this.click();
  }
}
