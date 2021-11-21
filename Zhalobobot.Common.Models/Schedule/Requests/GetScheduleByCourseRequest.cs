using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Models.Schedule.Requests
{
    public class GetScheduleByCourseRequest
    {
        public GetScheduleByCourseRequest(Course course)
        {
            Course = course;
        }
        
        public Course Course { get; }
    }
}