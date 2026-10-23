
import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
})

export class HeaderComponent implements OnInit {

  constructor(
    private readonly router: Router

  ) { }



  lastElement: any;


  ngOnInit(): void {
    this.setMenuIndex(window.location.pathname.substring(1,window.location.pathname.length));
  }

  setMenuIndex(key:any)
  {

    switch (key) {
      case "patients":
        this.lastElement = document.getElementById('accueil');
        this.lastElement.classList.remove("active");
        // this.lastElement = document.getElementById('rdv');
        // this.lastElement.classList.remove("active");
        this.lastElement = document.getElementById('assitante');
        this.lastElement.classList.remove("active");
        this.lastElement = document.getElementById('parametres');
        this.lastElement.classList.remove("active");
        this.lastElement = document.getElementById('patients');
        this.lastElement.classList.add("active");
        break;
      case "accueil":
        this.lastElement = document.getElementById('patients');
        this.lastElement.classList.remove("active");
        // this.lastElement = document.getElementById('rdv');
        // this.lastElement.classList.remove("active");
        this.lastElement = document.getElementById('assitante');
        this.lastElement.classList.remove("active");
        this.lastElement = document.getElementById('parametres');
        this.lastElement.classList.remove("active");
        this.lastElement = document.getElementById('accueil');
        this.lastElement.classList.add("active");
        break;

        case "rdv":
        this.lastElement = document.getElementById('patients');
        this.lastElement.classList.remove("active");
        // this.lastElement = document.getElementById('rdv');
        // this.lastElement.classList.remove("active");
        this.lastElement = document.getElementById('assitante');
        this.lastElement.classList.remove("active");
        this.lastElement = document.getElementById('parametres');
        this.lastElement.classList.remove("active");
        this.lastElement = document.getElementById('rdv');
        this.lastElement.classList.add("active");
        break;

      default:
        this.lastElement = document.getElementById('accueil');
        this.lastElement.classList.remove("active");
        // this.lastElement = document.getElementById('rdv');
        // this.lastElement.classList.remove("active");
        this.lastElement = document.getElementById('assitante');
        this.lastElement.classList.remove("active");
        this.lastElement = document.getElementById('parametres');
        this.lastElement.classList.remove("active");
        this.lastElement = document.getElementById('patients');
        this.lastElement.classList.remove("active");
        break;
    }
  }
  navigateTo(key: any) {
    this.router.navigate([key]);
    this.setMenuIndex(key);
  }
}
