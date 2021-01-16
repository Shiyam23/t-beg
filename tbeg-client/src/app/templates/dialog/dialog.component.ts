import { Component, Inject, OnInit } from '@angular/core';
import { MAT_DIALOG_DATA } from '@angular/material/dialog';

@Component({
  selector: 'app-dialog',
  templateUrl: './dialog.component.html',
  styleUrls: ['./dialog.component.css']
})
export class DialogComponent implements OnInit {

  DialogDataType = DialogDataType;
  DialogDataOption = DialogDataOption;

  constructor(@Inject(MAT_DIALOG_DATA) public data: DialogData) { }

  ngOnInit(): void {
  }
}

export interface DialogData {

  type : DialogDataType,
  option: DialogDataOption,
  content: string

}

export enum DialogDataType{

  INFO,
  ERROR,
  GAMEOVER

}

export enum DialogDataOption{

  DISMISS,
  ACCEPT

}