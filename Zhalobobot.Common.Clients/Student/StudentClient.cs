using System;
using System.Net.Http;
using System.Threading.Tasks;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Serialization;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Common.Clients.Student
{
    public class StudentClient : IStudentClient
    {
        private readonly HttpClient client = new();

        private readonly string serverUri;

        public StudentClient(string serverUri = "https://localhost:5001")
        {
            this.serverUri = serverUri;
        }

        public async Task<ZhalobobotResult<AbTestStudent>> GetAbTestStudent(string telegramId)
        {
            var response = await client.GetAsync($"{serverUri}/students/abTest/{telegramId}");

            if (!response.IsSuccessStatusCode)
                throw new Exception();

            var student = (await response.Content.ReadAsStreamAsync()).FromJsonStream<AbTestStudent>();
            
            return new ZhalobobotResult<AbTestStudent>(student, response.IsSuccessStatusCode, response.StatusCode);

        }
    }
}