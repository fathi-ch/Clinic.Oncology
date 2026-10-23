import { Component, OnInit, ViewChild } from "@angular/core";
import { Router } from "@angular/router";
import { RdvComponent } from "../views/rdv/rdv.component";
import { Rdv } from "../models/rdv/RdvModel";
import { ServicesPatient } from "../services/patient/patient.service";
import { ServicesRdv } from "../services/rdv/rdv.service";
import { ServiceCmnObject } from "../services/ServiceCmnObject";

@Component({
  selector: 'app-accueil',
    templateUrl: './accueil.component.html',
    styleUrls: ['./accueil.component.scss'],

  })
  
 

  export class AcceuilComponent implements OnInit {
    
    @ViewChild('agenda') agenda!: RdvComponent;

    // hero and summary cards
    readonly todayLabel = new Date().toLocaleDateString('fr-FR', { weekday: 'long', day: 'numeric', month: 'long' })
      .replace(/^./, c => c.toUpperCase());
    todaySummary = "";
    todayCount = 0;
    weekCount = 0;
    weekDone = 0;
    nextVisitNote = "";

    constructor(
      private readonly router: Router,
      public readonly PateintService: ServicesPatient,
      public readonly RdvService: ServicesRdv,
      private readonly serviceCmnObject: ServiceCmnObject
    ) 
    {
    }

  
    
    ngOnInit(): void {
    }

    public goToPatients() {
      this.router.navigate(['/patients']);
    }

    // Called whenever the calendar reloads, so the cards follow bookings made from it.
    public async loadSummary() {
      const today = new Date();
      const weekStart = new Date(today);
      weekStart.setDate(today.getDate() - ((today.getDay() + 1) % 7)); // weeks start on Saturday, like the calendar
      const weekEnd = new Date(weekStart);
      weekEnd.setDate(weekStart.getDate() + 6);

      const [todays, week] = await Promise.all([
        this.RdvService.GetRdvByDate(today, today),
        this.RdvService.GetRdvByDate(weekStart, weekEnd),
      ]);

      this.todayCount = todays?.length ?? 0;
      this.weekCount = week?.length ?? 0;
      this.weekDone = (week ?? []).filter(v => v.status === 'TRM').length;
      this.todaySummary = this.todayCount === 0
        ? "Aucun rendez-vous aujourd'hui."
        : `${this.todayCount} rendez-vous prévu${this.todayCount > 1 ? 's' : ''} aujourd'hui.`;

      const now = Date.now();
      const next = (todays ?? [])
        .filter(v => v.startTime && new Date(v.startTime).getTime() >= now)
        .sort((a, b) => new Date(a.startTime!).getTime() - new Date(b.startTime!).getTime())[0];
      this.nextVisitNote = next
        ? `Prochain à ${new Date(next.startTime!).toLocaleTimeString('fr-FR', { hour: '2-digit', minute: '2-digit' })}, ${next.patient?.firstName ?? ''} ${next.patient?.lastName ?? ''}`.trim()
        : this.todayCount === 0 ? "Toute la journée est libre" : "Plus aucun rendez-vous aujourd'hui";
    }
}