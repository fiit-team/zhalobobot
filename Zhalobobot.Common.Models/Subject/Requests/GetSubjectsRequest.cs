using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Models.Subject.Requests
{
    public class GetSubjectsRequest
    {
        public GetSubjectsRequest(Course course)
        {
            Course = course;
        }

        public Course Course { get; }
    }
}