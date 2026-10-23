import { Component } from '@angular/core';
import { CLINIC } from '../../clinic-info';

@Component({
  selector: 'app-contact',
  templateUrl: './contact.component.html',
  styleUrls: ['./contact.component.scss'],
})
export class ContactComponent {
  readonly clinic = CLINIC;

  call(): void {
    window.location.href = this.clinic.phoneHref;
  }

  openMap(): void {
    window.open(this.clinic.mapHref, '_blank', 'noopener');
  }
}
