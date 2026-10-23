import { Component, OnInit } from "@angular/core";
import { PercentPipe } from "@angular/common";
import { DxChartTypes } from "devextreme-angular/ui/chart";
import { DataChartDaugh } from "../../models/charts/DataChartDaugh";
import { Rdv } from "../../models/rdv/RdvModel";
import { ServicesRdv } from "../../services/rdv/rdv.service";

// Visit statistics, moved off the home page so the agenda gets the space.
@Component({
  selector: 'app-statistics',
  templateUrl: './statistics.component.html',
})
export class StatisticsComponent implements OnInit {
    highAverage = 77;

    lowAverage = 58;
    pipe = new PercentPipe('en-US');
    
    allRdvData: Rdv[]=[];
    dataChart:DataChartDaugh[]=[];
    dataChartBar:DataChartDaugh[]=[];
    chartRang:string[]=["Aujourd'hui","Semaine","Mois"];
    daysOfTheWeek:string[]= ['Samedi','Dimanche', 'Lundi', 'Mardi', 'Mercredi', 'Jeudi', 'Vendredi'];
    chartTitle:string="Avancement des visites d'aujourd'hui";

    customizeText: DxChartTypes.ValueAxisLabel['customizeText'] = ({ valueText }) => `${valueText}&#176F`;

    customizeTooltip = ({ valueText, percent }: { valueText: string, percent: number }) => ({
      text: `${valueText} - ${this.pipe.transform(percent, '1.2-2')}`,
    });

    constructor(public readonly RdvService: ServicesRdv) {
    }

    public onSelectionChanged(e:any)
    {
     
       if(e.selectedItem==this.chartRang[0])
        {

          let start = new Date(Date.now());
          let end = new Date(Date.now());

          this.getAllRdv(start,end);
          this.chartTitle="Avancement des visites d'aujourd'hui";
        }
        if(e.selectedItem==this.chartRang[1])
        {
  
            let start =new Date(this.getSatrtWeekRange());
            let end = new Date(this.getEndWeekRange());
  
            this.getAllRdv(start,end);
            this.chartTitle="Avancement des visites de la semaine";
        }
        if(e.selectedItem==this.chartRang[2])
          {
    
              let start =new Date(this.getStartOfTheMonth());
              let end = new Date(this.getEndOfTheMonth());
    
              this.getAllRdv(start,end);
              this.chartTitle="Avancement des visites du mois";
          }
      
    }
    
    
    // One bar per day of the current week (Saturday first). The chart only redraws when it receives a
    // new array, so the counts are gathered first and assigned once, in day order.
    public async loadChartDaunData()
    {
       const dates = this.getNextDate();
       this.dataChartBar = await Promise.all(this.daysOfTheWeek.map(async (day, i) => {
         const dateDay = new Date(dates[i]);
         const visits = await this.RdvService.GetRdvByDate(dateDay, dateDay);
         return { dataTitle: day, val: visits?.length ?? 0 } as DataChartDaugh;
       }));
    }

    public getSatrtWeekRange()
    {
      const now = new Date();
      const dayOfWeek = now.getDay(); // 0-6, 0 is Sunday, 1 is Monday, etc.
      const numDay = now.getDate();
      
      const startOfWeek = new Date(now); // Start of week
      startOfWeek.setDate(numDay - ((dayOfWeek + 1) % 7));
     return  startOfWeek.setHours(0, 0, 0, 0); // Set hours, minutes, seconds and milliseconds to 0
      
    }

    public getEndWeekRange()
    {
      const now = new Date();
      const dayOfWeek = now.getDay(); // 0-6, 0 is Sunday, 1 is Monday, etc.
      const numDay = now.getDate();
      
      const startOfWeek = new Date(now); // Start of week
      startOfWeek.setDate(numDay - dayOfWeek);
      startOfWeek.setHours(0, 0, 0, 0); // Set hours, minutes, seconds and milliseconds to 0
      
      const endOfWeek = new Date(startOfWeek); // End of week
      endOfWeek.setDate(startOfWeek.getDate() + 6);
      return endOfWeek.setHours(23, 59, 59, 999); //
    }

    public getStartOfTheMonth()
    {
      const now = new Date();

      const startOfMonth = new Date(now.getFullYear(), now.getMonth(), 1); // Start of month
      return startOfMonth.setHours(0, 0, 0, 0); // Set hours, minutes, seconds and milliseconds to 0
    }

    public getEndOfTheMonth()
    {
      const now = new Date();

      const endOfMonth = new Date(now.getFullYear(), now.getMonth() + 1, 0); // End of month
      return endOfMonth.setHours(23, 59, 59, 999); // Set hours, minutes, seconds and milliseconds to their max values

    }

    // The 7 days of the current week, Saturday first like the calendar, in local time.
    public getNextDate(): Date[] {
      const start = new Date();
      start.setHours(0, 0, 0, 0);
      start.setDate(start.getDate() - ((start.getDay() + 1) % 7));
      return Array.from({ length: 7 }, (_, i) => new Date(start.getFullYear(), start.getMonth(), start.getDate() + i));
    }
  

    async getAllRdv(start:Date,end:Date)
    {
     
      // rendring all rdvs
      this.allRdvData = (await this.RdvService.GetRdvByDate(start, end)) ?? [];
      this.dataChart = [
        { dataTitle: 'Terminé', val: this.allRdvData.filter(x => x.status == 'TRM').length },
        { dataTitle: 'En cours', val: this.allRdvData.filter(x => x.status == 'PLA').length },
      ];
    }

    ngOnInit(): void {
        // the period selector loads the doughnut chart itself when it starts on "Aujourd'hui"
        this.loadChartDaunData();
    }
}
