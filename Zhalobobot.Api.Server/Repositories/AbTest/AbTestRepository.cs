using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Common.Helpers.Helpers;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Api.Server.Repositories.AbTest
{
    public class AbTestRepository : GoogleSheetsRepositoryBase, IAbTestRepository
    {
        private string AbTestRange { get; }

        public AbTestRepository(IConfiguration configuration) 
            : base(configuration, configuration["ABTestSpreadSheetId"])
        {
            AbTestRange = configuration["AbTestRange"];
        }

        public async Task<AbTestStudent> Get(string telegramId)
        {
            var students = await GetRequest(AbTestRange).ExecuteAsync();

            var student = students.Values.FirstOrDefault(v => v[6].ToString() == telegramId);
            
            if (student == null)
                return new AbTestStudent(telegramId, null, null, null, null, true);

            return new AbTestStudent(
                telegramId,
                ParsingHelper.ParseName(student[1],student[2], student[3]),
                ParsingHelper.ParseNullableInt(student[0]), 
                ParsingHelper.ParseNullableInt(student[4]), 
                ParsingHelper.ParseNullableInt(student[5]),
                ParsingHelper.ParseBool(student[7]));
        }
    }
}