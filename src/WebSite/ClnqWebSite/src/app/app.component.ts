import { AfterViewInit, Component, OnInit } from '@angular/core';
import { AppConfig } from './services/app.config';
import { Router } from '@angular/router';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit, AfterViewInit {
  title = 'ClnqWebSite';

  protected urlApi=AppConfig.settings.apiServer.clnqApi;

   lastElement: any;

  constructor(
    private readonly router: Router
  ) {}
  ngOnInit(): void {
    
  }
  helloGoffa() {
    alert(this.urlApi);
}

ngAfterViewInit(): void {
  this.router.events.pipe().subscribe((route: any) => {
    if (route.url && !this.lastElement) {
      if (route.url.indexOf('patients') >= 0) {
        this.lastElement = document.getElementById('imgNavigation_patients');
      } else {
        this.lastElement = document.getElementById('imgNavigation_' + route.url.replace('/', ''));
      }
      if (this.lastElement)
        this.lastElement.classList.add("imgNavigationActive");
    }

  });
}

navigateTo(key: any) {
  this.router.navigate([key]);
  if (this.lastElement) {
    this.lastElement.classList.remove("imgNavigationActive")
    this.lastElement = document.getElementById('imgNavigation_' + key);
    this.lastElement.classList.add("imgNavigationActive");
  } else {
    this.lastElement = document.getElementById('imgNavigation_' + key);
    this.lastElement.classList.add("imgNavigationActive");
    // this.lastElement.classList.remove("imgNavigationActive");
  }
}



}
