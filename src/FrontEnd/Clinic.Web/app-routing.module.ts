import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AcceuilComponent } from './main/accueil.component';
import { PatientsComponent } from './views/patients/patients.component';
import { RdvComponent } from './views/rdv/rdv.component';
import { RdvDetailsComponent } from './views/rdvDetails/rdvDetails.component';
import { patientsDetailComponent } from './views/patientsDatails/patientsDetail.component';
import { PatientFileComponent } from './views/patient-file/patient-file.component';
import { ContactComponent } from './views/contact/contact.component';
import { StatisticsComponent } from './views/statistics/statistics.component';

const routes: Routes = [
   { path: '', redirectTo: 'accueil', pathMatch: 'full' },
   { path: 'accueil',component:AcceuilComponent },
   { path: 'patients',component:PatientsComponent },
   { path: 'patients/:id', component: PatientFileComponent },
   { path: 'contact', component: ContactComponent },
   { path: 'statistiques', component: StatisticsComponent },
   { path: 'patientsDetail',component:patientsDetailComponent },
   { path: 'rdv',component:RdvComponent },
   { path: 'rdvDetails',component:RdvDetailsComponent },
   { path: '**', redirectTo: 'accueil' },
  // { path: 'patients',loadChildren:()=> import('./views/patients/patients.module').then(mod=>mod.PatientsModule)}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
