using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Vostok.Hosting.Abstractions;
using Vostok.Hosting.Abstractions.Requirements;
using Zhalobobot.Bot.Api.Repositories.Common;
using Zhalobobot.Bot.Settings;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Helpers;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Bot.Api.Repositories.Subjects;

[RequiresSecretConfiguration(typeof(BotSecrets))]
public class SubjectRepository : GoogleSheetsRepositoryBase, ISubjectRepository
{
    private string SubjectsRange { get; }

    public SubjectRepository(IVostokHostingEnvironment environment)
        : base(environment, environment.SecretConfigurationProvider.Get<BotSecrets>().ScheduleSpreadSheetId)
    {
        SubjectsRange = environment.SecretConfigurationProvider.Get<BotSecrets>().SubjectsRange;
    }

    public async Task<Subject[]> GetAll()
    {
        var semester = SemesterHelper.Current;
            
        var subjectsRange = await GetRequest(SubjectsRange).ExecuteAsync();
            
        return subjectsRange.Values.Select(subject => new Subject(
                subject[0] as string ?? throw new ValidationException("Empty subject name"),
                ParsingHelper.ParseEnum<Course>(subject[1]),
                ParsingHelper.ParseEnum<Semester>(subject[2]),
                ParsingHelper.ParseSubjectCategory(subject[3]),
                ParsingHelper.ParseInt(subject[4])))
            .Where(s => s.Semester == semester)
            .ToArray();
    }
}