import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { SignalRService } from './services/signal-r.service';
import { FormsModule } from '@angular/forms';
import { AppProgressService } from './services/app-progress.service';
import { StartComponent } from './templates/start/start.component';
import { FunctorComponent } from './functor/functor.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatSelectModule} from '@angular/material/select';
import { MatButtonModule} from '@angular/material/button';
import { DrawGraphComponent } from './draw-graph/draw-graph.component';


@NgModule({
  declarations: [
    AppComponent,
    StartComponent,
    FunctorComponent,
    DrawGraphComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    MatButtonModule,
    MatSelectModule,
    FormsModule
  ],
  providers: [SignalRService, AppProgressService],
  bootstrap: [AppComponent]
})
export class AppModule { }
