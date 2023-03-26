using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Clinic.Core.Contracts
{
    public class PatientResponse
    {
        public Guid Id { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public int Age { get; set; }
        public  DateTime NextAppointment { get; set; }
        public int TotalDocuments { get; set; }

    }


}
