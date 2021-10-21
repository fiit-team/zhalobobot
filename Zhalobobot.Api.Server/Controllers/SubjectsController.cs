using Microsoft.AspNetCore.Mvc;
using Zhalobobot.Common.Models.Feedback;

namespace Zhalobobot.Api.Server.Controllers
{
    [Route("subjects")]
    public class SubjectsController
    {
        private Subject[] Subjects { get; } = 
        {
            new("Логическое программирование"),
            new("Бэкенд от Контура"),
            new("Фронтенд от Контура"),
            new("Диффуры"),
            new("Теория игр (онлайн-курс)"),
            new("Введение в ОС Unix"),
            new("Нейронные сети и обработка текста"),
            new("Промышленная разработка на Java"),
        };

        [HttpGet]
        public Subject[] Get() => Subjects;
    }
}