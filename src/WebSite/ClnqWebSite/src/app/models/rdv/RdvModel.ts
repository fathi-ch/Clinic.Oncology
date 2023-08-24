import { Pateint } from "../patient/PatientModel";


export class Rdv {
    id?: number;
    startTime?:Date;
    endTime?:Date;
    visitType?:string;
    title?:string;
    description?:string;
    status?:string;
    patient?:Pateint;
}
