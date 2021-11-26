using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Models.Subject.Requests
{
    public class GetSubjectsRequest
    {
        public SubjectCategory? Category { get; set; } = null;

        public Course? Course { get; set; } = null;
    }
}