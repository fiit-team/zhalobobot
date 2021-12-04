using System.Net.Http;
using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Student.Requests;

namespace Zhalobobot.Common.Clients.Student
{
    public class StudentClient : ClientBase, IStudentClient
    {
        public StudentClient(HttpClient client, string serverUri)
            : base("students", client, serverUri)
        {
        }

        public Task<ZhalobobotResult<AbTestStudent>> GetAbTestStudent(GetAbTestStudentRequest request)
            => Method<AbTestStudent>("ab-test").CallAsync(request);

        public Task<ZhalobobotResult<Models.Student.Student?>> FindStudent(long telegramId)
            => Method<Models.Student.Student?>($"find/${telegramId}").CallAsync();

        public Task<ZhalobobotResult<Models.Student.Student[]>> GetAll()
            => Method<Models.Student.Student[]>("getAll").CallAsync();

        public Task<ZhalobobotResult<Models.Student.Student[]>> Get(GetStudentsByCourseAndGroupAndSubgroupRequest request)
            => Method<Models.Student.Student[]>("getByCourseAndGroupAndSubgroup").CallAsync(request);

        public Task<ZhalobobotResult> Add(AddStudentRequest request)
            => Method("add").CallAsync(request);
    }
}