import { AfterViewInit, ChangeDetectorRef, Component, Input } from "@angular/core";


@Component({
  selector: 'app-spinner',
  templateUrl: './spinner.component.html',
  styleUrls: ['./spinner.component.scss']
})

export class SpinnerComponent implements AfterViewInit  {
  @Input('visible') visible: boolean = false;
  constructor(private readonly cdr :ChangeDetectorRef){}

  ngAfterViewInit(): void {
   this.cdr.detectChanges();
  }

 
}
