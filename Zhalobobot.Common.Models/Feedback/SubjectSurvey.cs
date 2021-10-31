using System.Collections.Generic;

namespace Zhalobobot.Common.Models.Feedback
{
    public class SubjectSurvey
    {
        public int Rating { get; set; } = 0;
        public ICollection<string> LikedPoints { get; set; } = new HashSet<string>();
        public ICollection<string> UnlikedPoints { get; set; } = new HashSet<string>();
    }
}