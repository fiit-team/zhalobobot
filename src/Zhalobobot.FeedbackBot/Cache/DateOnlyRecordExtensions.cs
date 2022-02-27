using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zhalobobot.Bot.Cache;

public static class DateOnlyRecordExtensions
{
    public static async Task<DateOnlyRecord> AsRecord(this Task<DateOnly> task)
        => new (await task);

    public static async Task<DateOnlyRecord[]> AsRecord(this Task<DateOnly[]> task)
        => (await task)
            .Select(date => new DateOnlyRecord(date))
            .ToArray();

    public static DateOnly FromRecord(this DateOnlyRecord record)
        => record.DateOnly;
    
    public static DateOnly[] FromRecord(this IEnumerable<DateOnlyRecord> records)
        => records
            .Select(r => r.DateOnly)
            .ToArray();
}