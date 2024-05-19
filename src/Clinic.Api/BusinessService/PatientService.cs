using Clinic.Core.Contracts;
using Clinic.Core.Services;
using Microsoft.AspNetCore.Mvc;
using Clinic.Core.Mappers;
using Clinic.Core.Dto;

namespace Clinic.Api.BusinessService
{
    public class PatientService : IPatientService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IVisitRepository _visitRepository;
        private readonly IDocumentRepository _documentRepository;
        public PatientService(IPatientRepository patientRepository, IVisitRepository visitRepository, IDocumentRepository documentRepository) 
        {
           this._patientRepository = patientRepository;
           this._visitRepository = visitRepository;
           this._documentRepository = documentRepository;
        }

        public async Task<IEnumerable<PatientResponse>> GetAllAsync()
        =>await _patientRepository.GetAllAsync();

        public async Task<IEnumerable<VisitResponse>> GetVisitesByPatientIdAsync(int id)
        {

            var result = await _visitRepository.GetAllAsync();
            var patient = await _patientRepository.GetByIdAsync(id);

            if (result != null && patient != null)
            {
                result= result.Where(visit => visit.PatientId == id);

                foreach (var item in result)
                {
                    item.Patient = patient;
                }

                return result.OrderByDescending(x => x.StartTime);
            }

            return Enumerable.Empty<VisitResponse>();

           
        }
      

        public async Task<IEnumerable<PatientResponse>> GetAllByNameAsync(string firstName)
        => (await _patientRepository.GetAllByNameAsync(firstName));

        public async Task<PatientResponse> GetAsync(int id)
        =>  await _patientRepository.GetByIdAsync(id);
        
        public async Task<IEnumerable<VisitResponse>> GetByDateAsync(int id)
            =>await _visitRepository.GetAllAsync();

        public async Task<PatientResponse> CreateAsync(PatientDto patientDto)
        =>await _patientRepository.CreateAsync(patientDto);

        public async Task DeleteByIdAsync(int id)
        {
          

            // delete visits
            var vsists = await this.GetVisitesByPatientIdAsync(id);
            foreach (var vsist in vsists)
            {
                // delete visit documents 
                await _documentRepository.DeleteByVisitIdAsync(vsist.Id);

                await _visitRepository.DeleteByIdAsync(vsist.Id);
            }

            await _patientRepository.DeleteByIdAsync(id);
        }
       

            
        public async Task<PatientResponse> UpdateAsync(int id, PatientDto patientDto)
        =>await _patientRepository.UpdateByIdAsync(id, patientDto);
          
    }
}
