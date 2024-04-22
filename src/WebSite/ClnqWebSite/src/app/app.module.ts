import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';

import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { DevExtremeModule, DxButtonModule, DxContextMenuComponent, DxDataGridComponent, DxDataGridModule, DxLoadPanelModule, DxTemplateModule } from 'devextreme-angular';
import { AppConfig } from './services/app.config';
import { HttpClientModule } from '@angular/common/http';
import { HeaderComponent } from './components/header/header.component';
import { AcceuilComponent } from './main/accueil.component';
import { PatientsComponent } from './views/patients/patients.component';
import {RouterModule, Routes } from '@angular/router';
import { ServicesPatient } from './services/patient/patient.service';
import { SpinnerModule } from './spinner/spinner.module';
import { RdvComponent } from './views/rdv/rdv.component';
import { ServicesRdv } from './services/rdv/rdv.service';
import { RdvDetailsComponent } from './views/rdvDetails/rdvDetails.component';
import { patientsDetailComponent } from './views/patientsDatails/patientsDetail.component';
import { FileUploaderComponent } from './views/fileUploader/file-uploader.component';

export function initializeApp(appConfig: AppConfig) {
  return () => appConfig.load();
}


@NgModule({
  declarations: [
    AppComponent,
    HeaderComponent,
    PatientsComponent,
    AcceuilComponent,
    RdvComponent,
    RdvDetailsComponent,
    patientsDetailComponent,
    FileUploaderComponent
  
  ],
  imports: [
    BrowserModule,
    AppRoutingModule,
    HttpClientModule,
    DxButtonModule,
    DxLoadPanelModule,
    DevExtremeModule,
    DxDataGridModule,
    DxTemplateModule,
    SpinnerModule
  
  ],
  exports: [AppComponent
  ],

  providers: [ ServicesPatient,ServicesRdv,AppConfig,
    { provide: APP_INITIALIZER,
      useFactory: initializeApp,
      deps: [AppConfig], multi: true }],
      
  bootstrap: [AppComponent]
})
export class AppModule { }
