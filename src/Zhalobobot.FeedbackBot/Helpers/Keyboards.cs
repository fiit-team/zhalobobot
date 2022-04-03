using System;
using System.Collections.Generic;
using System.Linq;
using EnumsNET;
using Telegram.Bot.Types.ReplyMarkups;
using Zhalobobot.Bot.Models;
using Zhalobobot.Common.Helpers.Extensions;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Helpers;
using Zhalobobot.Common.Models.Student;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Bot.Helpers
{
    internal static class Keyboards
    {
        // public static ReplyKeyboardMarkup DefaultKeyboard { get; } = new(
        //     new[]
        //     {
        //         new KeyboardButton[] { Buttons.Subjects },
        //         new KeyboardButton[] { Buttons.GeneralFeedback },
        //         new KeyboardButton[] { Buttons.Alarm },
        //         new KeyboardButton[] { Buttons.Schedule },
        //     })
        // {
        //     ResizeKeyboard = true
        // };

        public static ReplyKeyboardMarkup DefaultKeyboard(Student student) //todo: убрать после того, как добавим обработку 3го курса
        {
            var buttons = new List<KeyboardButton[]>
            {
                new KeyboardButton[] { Buttons.Subjects },
                new KeyboardButton[] { Buttons.GeneralFeedback },
                new KeyboardButton[] { Buttons.Alarm }
            };

            // todo: вернуть, когда починим расписание
            if (student.Course < Course.Third)
            {
                buttons.Add(new KeyboardButton[] { Buttons.Schedule });
            }
            
            return new ReplyKeyboardMarkup(buttons)
            {
                ResizeKeyboard = true
            };
        }

        public static ReplyKeyboardMarkup SubmitKeyboard { get; } = new(
            new KeyboardButton[]
            {
                Buttons.Submit,
                Buttons.MainMenu
            })
        {
            ResizeKeyboard = true
        };

        public static InlineKeyboardMarkup AddCourseKeyboard { get; } =
            new(Enum.GetValues<Course>()
                .Select(course => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        course.AsString(EnumFormat.Description),
                        string.Join(Strings.Separator, CallbackDataPrefix.AddCourse, course))
                }));

        public static InlineKeyboardMarkup AddCourseAndGroupKeyboard(Course course) =>
            new (Enum.GetValues<Group>()
                .Take(course == Course.Third ? 2 : 4)
                .Select(group => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        group.AsString(EnumFormat.Description),
                        string.Join(Strings.Separator, CallbackDataPrefix.AddCourseAndGroup, course, group))
                }));
        
        public static InlineKeyboardMarkup AddCourseAndGroupAndSubgroupKeyboard(Course course, Group group) =>
            new (Enum.GetValues<Subgroup>()
                .Select(subgroup => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        subgroup.AsString(EnumFormat.Description),
                        string.Join(Strings.Separator, CallbackDataPrefix.AddCourseAndGroupAndSubgroup, course, group, subgroup))
                }));

        public static InlineKeyboardMarkup RatingKeyboard { get; } =
            new (Enumerable.Range(1, 5)
                .Select(star => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        string.Join(" ", Enumerable.Repeat(Emoji.Star, star)),
                        string.Join(Strings.Separator, CallbackDataPrefix.Rating, star))
                }));
        
        public static InlineKeyboardMarkup SendFeedbackKeyboard(string subjectName) => 
            new []
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(
                        "Оставить обратную связь",
                        string.Join(Strings.Separator, CallbackDataPrefix.Feedback, subjectName))                
                }
            };

        public static InlineKeyboardMarkup GetSubjectCategoryKeyboard =>  
            new (Enum.GetValues<SubjectCategory>()
                    .Select(category => new[]
                    {
                        InlineKeyboardButton.WithCallbackData(
                            category.AsString(EnumFormat.Description),
                            string.Join(Strings.Separator, CallbackDataPrefix.SubjectCategory, category))
                    }));

        public static InlineKeyboardMarkup ChooseScheduleDayKeyboard(DayOfWeek lastStudyWeekDay)
        {
            var currentDay = DateHelper.EkbTime.DayOfWeek;

            var keyboard = new List<InlineKeyboardButton[]>();

            if (currentDay <= lastStudyWeekDay && currentDay != DayOfWeek.Sunday)
                keyboard.Add(new [] { CreateButton("На сегодня", (int)currentDay) });

            if (currentDay == DayOfWeek.Sunday)
            {
                keyboard.Add(new[] { CreateButton("На завтра", (int)ScheduleDay.NextMonday) });
                keyboard.Add(new[] { CreateButton("На следующую неделю", (int)ScheduleDay.NextWeek) });
            }
            else if (currentDay < lastStudyWeekDay)
            {
                keyboard.Add(new[] { CreateButton("На завтра", (int)currentDay + 1) });
            }
            else
            {
                keyboard.Add(new[] { CreateButton("На понедельник", (int)ScheduleDay.NextMonday) });
                keyboard.Add(new[] { CreateButton("На следующую неделю", (int)ScheduleDay.NextWeek) });
            }

            if ((int)currentDay + 1 < (int)lastStudyWeekDay && currentDay != DayOfWeek.Sunday)
                keyboard.Add(new [] { CreateButton("До конца недели", (int)ScheduleDay.UntilWeekEnd) });

            keyboard.Add(new [] { CreateButton("На эту неделю", (int)ScheduleDay.FullWeek) });

            return new InlineKeyboardMarkup(keyboard);

            InlineKeyboardButton CreateButton(string text, int scheduleDay)
                => InlineKeyboardButton.WithCallbackData(text,
                    string.Join(Strings.Separator, CallbackDataPrefix.ChooseScheduleRange, $"{scheduleDay}"));
        }
        
        public static InlineKeyboardMarkup GetSubjectsKeyboard(IEnumerable<Subject> subjects)
            => GetSubjectsKeyboard(subjects.Select(s => s.Name));
        
        public static InlineKeyboardMarkup GetSubjectsKeyboard(IEnumerable<string> subjects)
        {
            var inlineKeyboard = new InlineKeyboardMarkup(
                subjects
                    .Select(subject => new[]
                    {
                        InlineKeyboardButton.WithCallbackData(
                            subject.Slice(),
                            string.Join(Strings.Separator, CallbackDataPrefix.Subject, subject.GetHashCode()))
                    }));

            return inlineKeyboard;
        }
    }
}