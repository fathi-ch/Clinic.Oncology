import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { PatientsRoutingModule } from './patientsrouting.module'
import { DevExtremeModule, DxDataGridComponent, DxDataGridModule, DxTemplateModule } from 'devextreme-angular';
import { FlexLayoutModule } from '@angular/flex-layout';
import { PatientsComponent } from './patients.component';



@NgModule({
    declarations: [PatientsComponent],
    imports: [
    CommonModule,
    DevExtremeModule,
    FlexLayoutModule,
    PatientsRoutingModule,
    DxDataGridModule,
    DxTemplateModule
    ],
    exports: [PatientsComponent],
})
export class PatientsModule { }