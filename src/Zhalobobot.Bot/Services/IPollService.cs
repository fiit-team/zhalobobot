using System.Collections.Generic;

namespace Zhalobobot.Bot.Services
{
    public interface IPollService
    {
        public IReadOnlyList<string> GetLikedPointsPoll();

        public IReadOnlyList<string> GetUnlikedPointsPoll();

        public ICollection<string> GetLikedPoints(int[] optionIds);

        public ICollection<string> GetUnlikedPoints(int[] optionIds);
    }
}