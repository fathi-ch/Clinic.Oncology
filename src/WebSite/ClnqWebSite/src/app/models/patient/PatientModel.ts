
export class Pateint {
    id?: string;
    firstName?: string;
    lastName?: string;
    birthDate?: Date;
    age?:string;
    nextAppointment?: Date;
    constructor(data: any) {
        Object.assign(this, data);
    }
}
