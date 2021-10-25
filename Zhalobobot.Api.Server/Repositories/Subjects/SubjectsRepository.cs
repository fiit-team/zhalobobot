using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Common.Models.Feedback;

namespace Zhalobobot.Api.Server.Repositories.Subjects
{
    public class SubjectsRepository : GoogleSheetsRepositoryBase, ISubjectsRepository
    {
        private ILogger<ISubjectsRepository> Logger { get; }
        private IConfiguration Configuration { get; }

        public SubjectsRepository(
            ILogger<ISubjectsRepository> logger, 
            IConfiguration configuration)
        : base(configuration, configuration["FeedbackSpreadSheetId"], "FEEDBACK_SPREADSHEET_CREDENTIALS")
        {
            Logger = logger;
            Configuration = configuration;
        }
        
        public async Task<Subject[]> Get()
        {
            var subjects = await StartGoogleSheetsRequest()
                .SetupRange("Subjects")
                .ToGetRequest()
                .ExecuteAsync();

            return subjects.Values.Select(s => new Subject(s.First().ToString()!)).ToArray();
        }
    }
}