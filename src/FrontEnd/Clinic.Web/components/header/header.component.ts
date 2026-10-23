import { Component } from '@angular/core';
import { CLINIC } from '../../clinic-info';

@Component({
  selector: 'app-header',
  templateUrl: './header.component.html',
  styleUrls: ['./header.component.scss'],
})
export class HeaderComponent {
  readonly clinic = CLINIC;
}
