import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RdvRoutingModule } from './rdvrouting.module'
import { DevExtremeModule, DxDataGridComponent, DxDataGridModule, DxTemplateModule } from 'devextreme-angular';
import { FlexLayoutModule } from '@angular/flex-layout';
import { RdvComponent } from './rdv.component';



@NgModule({
    declarations: [RdvComponent],
    imports: [
    CommonModule,
    DevExtremeModule,
    FlexLayoutModule,
    RdvRoutingModule,
    DxDataGridModule,
    DxTemplateModule
    ],
    exports: [RdvComponent],
})
export class RdvModule { }