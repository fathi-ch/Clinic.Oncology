import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AcceuilComponent } from './main/accueil.component';
import { PatientsComponent } from './views/patients/patients.component';
import { RdvComponent } from './views/rdv/rdv.component';

const routes: Routes = [
  //{ path: '',component:AcceuilComponent },
   { path: 'accueil',component:AcceuilComponent },
   { path: 'patients',component:PatientsComponent },
   { path: 'rdv',component:RdvComponent },
  // { path: 'patients',loadChildren:()=> import('./views/patients/patients.module').then(mod=>mod.PatientsModule)}
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
