using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Zhalobobot.Bot.Api.Repositories.Common;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Bot.Api.Repositories.FiitStudentsData;

public class FiitStudentsDataRepository : GoogleSheetsRepositoryBase, IFiitStudentsDataRepository
{
    private string StudentsDataRange { get; }
    public FiitStudentsDataRepository(IConfiguration configuration) 
        : base(configuration, configuration["FiitStudentsDataSpreadSheetId"])
    {
        StudentsDataRange = configuration["StudentsDataRange"];
    }

    public async Task<IEnumerable<StudentData>> GetAll()
    {
        var values = await GetRequest(StudentsDataRange).ExecuteAsync();

        return values.Values
            .Where(data => data.Count == 7 && (data[6] as string ?? "").StartsWith("@"))
            .Select(data => new StudentData(
                ParsingHelper.ParseEnum<Course>(data[0]),
                ParsingHelper.ParseEnum<Group>(data[4]),
                ParsingHelper.ParseEnum<Subgroup>(data[5]),
                data[6] as string ?? "",
                new Name(data[1] as string ?? "", data[2] as string ?? "", null)));
    }
}