import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { SignalRService } from './services/signalR/signal-r.service';
import { FormsModule } from '@angular/forms';
import { AppProgressService } from './services/appProgress/app-progress.service';
import { StartComponent } from './templates/start/start.component';
import { FunctorComponent } from './functor/functor.component';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MatSelectModule, } from '@angular/material/select';
import { MatMenuModule } from '@angular/material/menu'
import { MatButtonModule} from '@angular/material/button';
import { DrawGraphComponent } from './draw-graph/draw-graph.component';
import { ContextMenuComponent } from './templates/context-menu/context-menu.component';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { GraphPanelComponent } from './graph-panel/graph-panel.component';




@NgModule({
  declarations: [
    AppComponent,
    StartComponent,
    FunctorComponent,
    DrawGraphComponent,
    ContextMenuComponent,
    GraphPanelComponent
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    BrowserAnimationsModule,
    MatButtonModule,
    MatSelectModule,
    MatMenuModule,
    FormsModule,
    MatInputModule,
    MatFormFieldModule,
    MatCheckboxModule
  ],
  providers: [SignalRService, AppProgressService],
  bootstrap: [AppComponent]
})
export class AppModule { }
