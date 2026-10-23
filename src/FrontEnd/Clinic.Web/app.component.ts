import { Component, OnInit } from '@angular/core';
import { AppConfig } from './services/app.config';
import frMessages from './fr.json';
import { ServiceCmnObject } from './services/ServiceCmnObject';
import { loadMessages, locale } from 'devextreme/localization';
import { delay } from 'rxjs';
import { CLINIC } from './clinic-info';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  title = 'ClnqWebSite';
  public spinnerVisible = false;
  readonly clinic = CLINIC;
  protected urlApi=AppConfig.settings.apiServer.clnqApi;

  constructor(
    private readonly serviceCmnObject:ServiceCmnObject
  ) {

    loadMessages(frMessages);
    locale("fr-FR");
    
    // delay(0): pages toggle the spinner while they are being created, which Angular rejects mid-render
    this.serviceCmnObject.spinnerLoading.pipe(delay(0)).subscribe(spin => this.spinnerVisible = spin);
  }
  ngOnInit(): void {
    
  }
  helloGoffa() {
    alert(this.urlApi);
}

}
