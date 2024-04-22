import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { patientsDetailRoutingModule } from './patientsDetailrouting.module'
import { DevExtremeModule, DxDataGridComponent, DxDataGridModule, DxTemplateModule } from 'devextreme-angular';
import { FlexLayoutModule } from '@angular/flex-layout';
import { patientsDetailComponent } from './patientsDetail.component';



@NgModule({
    declarations: [patientsDetailComponent],
    imports: [
    CommonModule,
    DevExtremeModule,
    FlexLayoutModule,
    patientsDetailRoutingModule,
    DxDataGridModule,
    DxTemplateModule
    ],
    exports: [patientsDetailComponent],
})
export class patientsDetailModule { }