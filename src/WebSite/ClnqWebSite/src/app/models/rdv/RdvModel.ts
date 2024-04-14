import { Pateint } from "../patient/PatientModel";


export class Rdv {
    id?: number;
    patientId?:number;
    startTime?:Date;
    endTime?:Date;
    visitType?:string;
    title?:string;
    description?:string;
    status?:string;
    patient?:Pateint;
    price?:number;
}
