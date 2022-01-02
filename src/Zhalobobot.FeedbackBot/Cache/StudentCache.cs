using System.Collections.Generic;
using Zhalobobot.Common.Helpers.Extensions;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Student;

namespace Zhalobobot.Bot.Cache
{
    public class StudentCache : EntityCacheBase<Student>
    {
        private readonly Dictionary<long, Student> chatIdIndex = new();
        private readonly Dictionary<(Course, Group, Subgroup), List<Student>> courseGroupSubgroupIndex = new();

        public StudentCache(Student[] students) 
            : base(students)
        {
            foreach (var student in students)
            {
                chatIdIndex.Add(student.Id, student);
                courseGroupSubgroupIndex.AddOrUpdate((student.Course, student.Group, student.Subgroup), new List<Student> { student }, s => s.Add(student));
            }
        }

        public Student? Find(long chatId) => chatIdIndex.Find(chatId);
        public Student Get(long chatId) => chatIdIndex.Get(chatId);
        public List<Student> Get((Course, Group, Subgroup) tuple) => courseGroupSubgroupIndex.GetOrCreate(tuple, _ => new List<Student>());
        public void Add(Student student) => chatIdIndex.Add(student.Id, student);
    }
}