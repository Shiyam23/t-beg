import { StringMap } from '@angular/compiler/src/compiler_facade_interface';

export interface DialogData {

    type : DialogDataType,
    content: string

}

export enum DialogDataType{

    INFO,
    ERROR,
    GAMEOVER

}