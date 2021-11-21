using Zhalobobot.Common.Models.Commons;

namespace Zhalobobot.Common.Models.Student.Requests
{
    public class GetStudentsByCourseAndGroupAndSubgroupRequest
    {
        public GetStudentsByCourseAndGroupAndSubgroupRequest(Course course, Group group, Subgroup subgroup)
        {
            Course = course;
            Group = group;
            Subgroup = subgroup;
        }
        
        public Course Course { get; }
        public Group Group { get; }
        public Subgroup Subgroup { get; }
    }
}