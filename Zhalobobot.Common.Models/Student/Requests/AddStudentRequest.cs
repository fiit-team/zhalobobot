namespace Zhalobobot.Common.Models.Student.Requests
{
    public class AddStudentRequest
    {
        public AddStudentRequest(Student student)
        {
            Student = student;
        }

        public Student Student { get; }
    }
}