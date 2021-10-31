using System.Collections.Generic;
using System.Linq;

namespace Zhalobobot.Bot.Services
{
    public class PollService : IPollService
    {
        private static IReadOnlyList<string> DefaultPoints { get; }
            = new List<string>
            {
                "Материал",
                "Стиль рассказа",
                "Домашки",
                "Препод",
                "Тык"
            };

        public IReadOnlyList<string> GetLikedPointsPoll()
            => DefaultPoints;

        public IReadOnlyList<string> GetUnlikedPointsPoll()
            => DefaultPoints;

        public ICollection<string> GetLikedPoints(int[] optionIds)
            => optionIds.Select(index => DefaultPoints[index]).ToList();

        public ICollection<string> GetUnlikedPoints(int[] optionIds)
            => optionIds.Select(index => DefaultPoints[index]).ToList();
    }
}