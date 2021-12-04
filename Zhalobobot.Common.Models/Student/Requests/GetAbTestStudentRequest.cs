namespace Zhalobobot.Common.Models.Student.Requests
{
    public class GetAbTestStudentRequest
    {
        public GetAbTestStudentRequest(string username)
        {
            Username = username;
        }

        public string Username { get; }
    }
}