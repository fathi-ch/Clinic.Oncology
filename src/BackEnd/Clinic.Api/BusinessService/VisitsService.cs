using Clinic.Core.Contracts;
using Clinic.Core.Dto;
using Clinic.Core.Mappers;
using Clinic.Core.Services;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace Clinic.Api.BusinessService
{
    public class VisitsService: IVisitsService
    {

        private readonly IVisitRepository _visitRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IPatientRepository _patientRepository;


        public VisitsService(IVisitRepository _visitRepository, IDocumentRepository documentRepository, IPatientRepository patientRepository)
        {
            this._visitRepository = _visitRepository;
            this._documentRepository = documentRepository;
            _patientRepository = patientRepository;
        }

        public async Task<IEnumerable<VisitResponse>> GetAllAsync()
        {
            var result=await _visitRepository.GetAllAsync();

            if(result != null)
            {
                foreach (var item in result)
                {
                    item.Patient = await _patientRepository.GetByIdAsync(item.PatientId);
                }

                return result.OrderByDescending(x => x.StartTime);
            }

            return Enumerable.Empty<VisitResponse>();




        }
       
      
        public async Task<IEnumerable<VisitResponse>> GetBydDateAsync(DateTime fromDate, DateTime toDate)
        {
            
            var result =await _visitRepository.GetBydDateAsync(new DateTime(fromDate.Year, fromDate.Month, fromDate.Day, 0, 0, 0),
                new DateTime(toDate.Year, toDate.Month, toDate.Day, 23, 0, 0));

            if (result != null)
            {
                foreach (var item in result)
                {
                    item.Patient = await _patientRepository.GetByIdAsync(item.PatientId);
                }

                return result.OrderByDescending(x=>x.StartTime);
            }
            return Enumerable.Empty<VisitResponse>();

        }
       

        public async Task<VisitResponse> GetByIdAsync(int id)
        {
            var result = await _visitRepository.GetByIdAsync(id);

            if(result != null)
            {
                result.Patient= await _patientRepository.GetByIdAsync(result.PatientId);
            }

            return result;
        }
        
          
        public async Task<VisitResponse> CreateAsync(VisitDto VisitDto)
        {
            var result = await _visitRepository.CreateAsync(VisitDto);
            if (result != null)
            {
                result.Patient = await _patientRepository.GetByIdAsync(result.PatientId);
            }

            return result;
        }
      

        public async Task<IEnumerable<PatientDocumentResponse>> GetDocumentsByVisitIdAsync(int visitid)
        => await _documentRepository.GetByVisitIdAsync(visitid); 

        public async Task DeleteByIdAsync(int id)
        {
            var rdv = await _visitRepository.GetByIdAsync(id);
            if(rdv is not null)
            {
                // delete visit documents 
                await _documentRepository.DeleteByVisitIdAsync(id);

                await _visitRepository.DeleteByIdAsync(id);
                // update next appointment value
                var patient = await _patientRepository.GetByIdAsync(rdv.PatientId);

                if (patient is not null)
                {
                    await UpdateNextAppointmentValue(patient.ToPatientDto(patient.Id));

                }

            }
           
        }

        public async Task<VisitResponse> UpdateByIdAsync(int id, VisitDto visitDto)
        {
            var rdv = await _visitRepository.UpdateByIdAsync(id, visitDto);

            if(rdv != null)
            {
                // update next appointment value
                var patient = await _patientRepository.GetByIdAsync(rdv.PatientId);

                if (patient is not null)
                {
                    await UpdateNextAppointmentValue(patient.ToPatientDto(patient.Id));

                }
            }

            return rdv;

        }

        private async Task UpdateNextAppointmentValue(PatientDto patient)
        {

            var allRdv = await _visitRepository.GetAllAsync();

            if (allRdv is not null && allRdv.Any())
            {
                allRdv = allRdv.Where(rdv => rdv.PatientId == rdv.PatientId).ToList();
                if (allRdv is not null && allRdv.Any())
                {
                    var nextDate = allRdv.Max(visit => visit.StartTime);
                    patient.NextAppointment = nextDate;

                }
                else
                {
                    patient.NextAppointment = null;
                }
                await _patientRepository.UpdateByIdAsync(patient.id, patient);
            }
            else
            {
                patient.NextAppointment = null;
            }
            await _patientRepository.UpdateByIdAsync(patient.id, patient);

        
        }
        
          
    }
}
