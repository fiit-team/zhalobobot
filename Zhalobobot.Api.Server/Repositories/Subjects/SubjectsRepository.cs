using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Api.Server.Repositories.Subjects
{
    public class SubjectsRepository : GoogleSheetsRepositoryBase, ISubjectsRepository
    {
        private IConfiguration Configuration { get; }
        private ILogger<SubjectsRepository> Logger { get; }

        public SubjectsRepository(
            IConfiguration configuration, ILogger<SubjectsRepository> logger)
        : base(configuration, configuration["FeedbackSpreadSheetId"])
        {
            Configuration = configuration;
            Logger = logger;
        }
        
        public async Task<Subject[]> Get()
        {
            var request = GetRequest("Subjects!A2:D50");
            request.MajorDimension = SpreadsheetsResource.ValuesResource.GetRequest.MajorDimensionEnum.COLUMNS;
            
            var subjectsRange = await request.ExecuteAsync();
            
            return subjectsRange.Values
                .SelectMany((subjects, category) => subjects.Select(subject => new Subject(subject.ToString()!, (SubjectCategory)category)))
                .ToArray();
        }
    }
}