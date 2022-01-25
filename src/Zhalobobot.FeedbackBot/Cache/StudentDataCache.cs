using System.Collections.Generic;
using Zhalobobot.Common.Helpers.Extensions;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Bot.Cache
{
    public class StudentDataCache : EntityCacheBase<StudentData>
    {
        private readonly Dictionary<string, StudentData> telegramUsernameIndex = new();

        public StudentDataCache(StudentData[] entities)
            : base(entities)
        {
            foreach (var studentData in All)
            {
                telegramUsernameIndex.Add(studentData.TelegramId, studentData);
            }
        }

        public StudentData? Find(string telegramUsername) => telegramUsernameIndex.Find(telegramUsername);
    }
}