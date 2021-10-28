using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Common.Helpers.Helpers;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Api.Server.Repositories.AbTest
{
    public class AbTestRepository : GoogleSheetsRepositoryBase, IAbTestRepository
    {
        public AbTestRepository(IConfiguration configuration) 
            : base(configuration, configuration["ABTestSpreadSheetId"])
        {
        }

        public async Task<AbTestStudent> Get(string telegramId)
        {
            var students = await StartGoogleSheetsRequest()
                .SetupRange("A2:H277")
                .ToGetRequest()
                .ExecuteAsync();
            
            var student = students.Values.FirstOrDefault(v => v[6].ToString() == telegramId);
            
            if (student == null)
                return new AbTestStudent(telegramId, Name.Empty, null, null, null, true);

            return new AbTestStudent(
                telegramId,
                ParsingHelper.ParseName(student[1],student[2], student[3]),
                ParsingHelper.ParseNullableInt(student[0]), 
                ParsingHelper.ParseNullableInt(student[4]), 
                ParsingHelper.ParseNullableInt(student[5]),
                student[7] as string == "TRUE");
        }
    }
    
    
}