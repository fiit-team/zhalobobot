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

        public Task<ZhalobobotResult<Models.Student.Student[]>> Get()
            => Method<Models.Student.Student[]>("get").CallAsync();

        public Task<ZhalobobotResult> Add(AddStudentRequest request)
            => Method("add").CallAsync(request);
    }
}