using Zhalobobot.Bot.Models;

namespace Zhalobobot.Bot.Services
{
    public class SubjectsService : ISubjectsService
    {
        private Subject[] Subjects { get; } = new[]
        {
            new Subject("Логическое программирование"),
            new Subject("Бэкенд от Контура"),
            new Subject("Фронтенд от Контура"),
            new Subject("Диффуры"),
            new Subject("Теория игр (онлайн-курс)"),
            new Subject("Введение в ОС Unix"),
            new Subject("Нейронные сети и обработка текста"),
            new Subject("Промышленная разработка на Java"),
        };

        public Subject[] GetSubjects() => this.Subjects;
    }
}
