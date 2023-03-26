using Clinic.Core.Data;
using Clinic.Core.Models;
using System.IO;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text;
using System.Globalization;

namespace Presentation.Pages.PatientList
{
    public class CreateModel : PageModel
    {
        private readonly ISqliteDbConnectionFactory _connectionFactory;

        [BindProperty]
        public Patient Patient { get; set; }

        public CreateModel(ISqliteDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }
        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPost(IEnumerable<IFormFile> files)
        {


            using var connection = await _connectionFactory.CreateDbConnectionAsync();

            using var transaction = connection.BeginTransaction();

            try
            {

                const string query = "INSERT INTO Patients (Id, FirstName, LastName, BirthDate)" +
                    " VALUES (@Id, @FirstName, @LastName, @BirthDate);";
                Patient.Id = Guid.NewGuid();
                Patient.FirstName = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(Patient.FirstName);
                Patient.LastName = Patient.LastName.ToUpper();
                var result = await connection.ExecuteAsync(query,
                   new { Patient.Id, Patient.FirstName, Patient.LastName, Patient.BirthDate });

                if (files.Any())
                {
                    const string queryLastPatient = "select * from Patients where Id = @Id";

                    var lastPatient = await connection.QuerySingleOrDefaultAsync<Patient>(
                        queryLastPatient, new { Id = Patient.Id });


                   
                    foreach (var file in files)
                    {
                        var docId = Guid.NewGuid();
                        var originalFileExtention = Path.GetExtension(file.FileName);
                        string currentTimeMs = (DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond).ToString();
                        string[] stringsToJoin = { Patient.FirstName, docId.ToString(), DateTime.Now.ToString("yyyyMMdd_HHmmss"), currentTimeMs };

                        string fileName = string.Join("_", stringsToJoin);
                        var pathPic = Path.Combine(@"D:\Clinic\Documents\", fileName + originalFileExtention);
                        using (var fileStream = new FileStream(pathPic, FileMode.Create))
                        {
                            await file.CopyToAsync(fileStream);
                        }
                        const string documentQuery = "INSERT INTO Documents (Id, PatientId, Path) VALUES (@Id, @PatientId, @Path)";
                        await connection.ExecuteAsync(documentQuery, new { Id = docId, PatientId = lastPatient.Id, Path = pathPic });
                    }
                }

                transaction.Commit();

            }
            catch (Exception)
            {
                transaction.Rollback();
                throw;
            }

            return RedirectToPage("Index");

        }
    }
}
