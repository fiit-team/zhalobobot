using System;
using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Models.Schedule;

public record DayWithoutPairs(
    DateOnly Date,
    string SubjectName,
    Course Course,
    Group Group,
    Subgroup Subgroup,
    TimeOnly? EndTime);