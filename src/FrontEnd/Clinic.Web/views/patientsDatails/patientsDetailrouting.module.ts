import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { patientsDetailComponent } from './patientsDetail.component';



const routes: Routes = [
  { path: '', component: patientsDetailComponent,  data: { title: 'Patients' } }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class patientsDetailRoutingModule { }