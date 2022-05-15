using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.UserCommon;

namespace Zhalobobot.Common.Models.Student
{
    public record Student(
        long Id,
        string? Username,
        Course Course,
        Group Group,
        Subgroup Subgroup,
        Name? Name,
        string[] SpecialCourseNames);
    // todo: переписать на dto-шки и сделать так, чтобы контроллер сервера их возвращал
    // todo: выделить слой сервиса, который отвечает за бизнес-логику
    // todo: сделать общий пересыльщик сообщений, который автоматически будет пересылать markdown
}