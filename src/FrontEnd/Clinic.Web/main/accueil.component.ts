import { Component, OnInit } from "@angular/core";
import { Rdv } from "../models/rdv/RdvModel";
import { ServicesPatient } from "../services/patient/patient.service";
import { ServicesRdv } from "../services/rdv/rdv.service";
import { ServiceCmnObject } from "../services/ServiceCmnObject";
import { DatePipe, PercentPipe } from "@angular/common";
import { DataChartDaugh } from "../models/charts/DataChartDaugh";
import { DxChartTypes } from "devextreme-angular/ui/chart";

@Component({
  selector: 'app-accueil',
    templateUrl: './accueil.component.html',
    styleUrls: ['./accueil.component.scss'],
    providers: [DatePipe]

  })
  
 

  export class AcceuilComponent implements OnInit {
    
    highAverage = 77;

    lowAverage = 58;
    pipe = new PercentPipe('en-US');
    
    allRdvData: Rdv[]=[];
    dataChart:DataChartDaugh[]=[];
    dataChartBar:DataChartDaugh[]=[];
    chartRang:string[]=["Aujourd'hui","Semaine","Mois"];
    daysOfTheWeek:string[]= ['Samedi','Dimanche', 'Lundi', 'Mardi', 'Mercredi', 'Jeudi', 'Vandredi'];
    chartTitle:string="Avancement des visites d'aujourd'hui";

    customizeText: DxChartTypes.ValueAxisLabel['customizeText'] = ({ valueText }) => `${valueText}&#176F`;

    customizeTooltip = ({ valueText, percent }: { valueText: string, percent: number }) => ({
      text: `${valueText} - ${this.pipe.transform(percent, '1.2-2')}`,
    });

    constructor(
      public readonly PateintService: ServicesPatient,
      public readonly RdvService: ServicesRdv,
      private readonly serviceCmnObject: ServiceCmnObject,
      private datePipe: DatePipe
    ) 
    {
     
      this.chartTitle="Avancement des visites d'aujourd'hui";
      
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
    
    
    public loadChartDaunData()
    {
       this.dataChartBar=[];
       
       this.daysOfTheWeek.forEach(async day=>{
        
        let dataBar=new DataChartDaugh();
        dataBar.dataTitle=day;
        let dateDay=new Date(this.getNextDate()[this.daysOfTheWeek.indexOf(day)]);
        
        await this.RdvService.GetRdvByDate(dateDay,dateDay).then(data => {
          
          if(data && data.length>0)
            {
              dataBar.val=data.length;
            }
            else
            {
              dataBar.val=0;
            }

            this.dataChartBar.push(dataBar);
       
        });

       });
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

    public getNextDate() {
      const days: string[] =['Samedi','Dimanche', 'Lundi', 'Mardi', 'Mercredi', 'Jeudi', 'Vandredi'];
      const today: Date = new Date();
      const today_index: number = days.indexOf(days[today.getDay()]);
      let week_dates: string[] = [];
  
      for (let i = 0; i < 7; i++) {
          let date: Date = new Date();
          date.setDate(today.getDate() - today_index + i - 1);
          week_dates.push(date.toISOString().split('T')[0]);
      }
  
  
      return week_dates;

  
     
  
      
  }
  

    async getAllRdv(start:Date,end:Date)
    {
     
      // rendring all rdvs
      this.dataChart=[];
      await this.RdvService.GetRdvByDate(start,end).then(data => {
        this.allRdvData = data as Rdv[];
        let finishiedRdv=this.allRdvData.filter(x=>x.status=='TRM');
        let inProgressRdv=this.allRdvData.filter(x=>x.status=='PLA');
        this.dataChart.push({dataTitle:'Terminé',val:finishiedRdv.length});
        this.dataChart.push({dataTitle:'En cours',val:inProgressRdv.length});

      });
    }

    ngOnInit(): void {
        this.loadChartDaunData();
    }
}