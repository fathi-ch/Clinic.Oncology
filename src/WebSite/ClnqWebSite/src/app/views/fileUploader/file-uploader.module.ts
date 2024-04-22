import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { DevExtremeModule } from 'devextreme-angular';
import { FlexLayoutModule } from '@angular/flex-layout';
import { FileUploaderComponent } from './file-uploader.component';


@NgModule({
    declarations: [FileUploaderComponent],
    imports: [
        CommonModule,
        DevExtremeModule,
        FlexLayoutModule,
    ],
    exports: [FileUploaderComponent
    ],
})
export class FileUploaderModule { }