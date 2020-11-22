import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { SignalRService } from './services/signal-r.service';
import { FormsModule } from '@angular/forms';
import { AppProgressService } from './services/app-progress.service';
import { StartComponent } from './templates/start/start.component';


@NgModule({
  declarations: [
    AppComponent,
    StartComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    FormsModule
  ],
  providers: [SignalRService, AppProgressService],
  bootstrap: [AppComponent]
})
export class AppModule { }
