import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AccueilRoutingModule } from '../main/accueilrouting.module';
import { DevExtremeModule } from 'devextreme-angular';
import { FlexLayoutModule } from '@angular/flex-layout';
import { AcceuilComponent } from './accueil.component';



@NgModule({
   
    imports: [
    CommonModule,
    DevExtremeModule,
    FlexLayoutModule,
    ],
    exports: [AcceuilComponent],
})
export class AccueilModule { }