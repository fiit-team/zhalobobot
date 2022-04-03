namespace Zhalobobot.Common.Models.Student.Requests;

public class UpdateStudentRequest
{
    public UpdateStudentRequest(Student student)
    {
        Student = student;
    }

    public Student Student { get; }
}