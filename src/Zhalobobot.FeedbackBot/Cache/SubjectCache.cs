using System.Collections.Generic;
using Zhalobobot.Common.Helpers.Extensions;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Bot.Cache
{
    public class SubjectCache : EntityCacheBase<Subject>
    {
        private readonly Dictionary<Course, List<Subject>> courseIndex = new();
        private readonly Dictionary<SubjectCategory, List<Subject>> categoryIndex = new();
        private readonly Dictionary<(Course, SubjectCategory), List<Subject>> courseAndCategoryIndex = new();
        private readonly Dictionary<string, List<Subject>> subjectNameIndex = new();

        public SubjectCache(Subject[] subjects) 
            : base(subjects)
        {
            foreach (var subject in subjects)
            {
                courseIndex.AddOrUpdate(subject.Course, new List<Subject> {subject}, subj => subj.Add(subject));
                categoryIndex.AddOrUpdate(subject.Category, new List<Subject> {subject}, subj => subj.Add(subject));
                courseAndCategoryIndex.AddOrUpdate((subject.Course, subject.Category), new List<Subject> {subject}, subj => subj.Add(subject));
                subjectNameIndex.AddOrUpdate(subject.Name, new List<Subject> {subject}, subj => subj.Add(subject));
            }
        }

        public List<Subject> Get(Course course) => courseIndex.GetOrCreate(course, _ => new List<Subject>());
        public List<Subject> Get(SubjectCategory category) => categoryIndex.GetOrCreate(category, _ => new List<Subject>());
        public List<Subject> Get((Course, SubjectCategory) tuple) => courseAndCategoryIndex.GetOrCreate(tuple, _ => new List<Subject>());
        public List<Subject> Get(string subjectName) => subjectNameIndex.GetOrCreate(subjectName, _ => new List<Subject>());
    }
}