import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RdvDetailsRoutingModule } from './rdvDetailsrouting.module'
import { DevExtremeModule, DxDataGridComponent, DxDataGridModule, DxTemplateModule } from 'devextreme-angular';
import { FlexLayoutModule } from '@angular/flex-layout';
import { RdvDetailsComponent } from './rdvDetails.component';



@NgModule({
    declarations: [RdvDetailsComponent],
    imports: [
    CommonModule,
    DevExtremeModule,
    FlexLayoutModule,
    RdvDetailsRoutingModule,
    DxDataGridModule,
    DxTemplateModule
    ],
    exports: [RdvDetailsComponent],
})
export class RdvDetailsModule { }