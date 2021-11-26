using System;
using System.Linq;
using EnumsNET;
using Telegram.Bot.Types.ReplyMarkups;
using Zhalobobot.Bot.Models;
using Zhalobobot.Common.Helpers;
using Zhalobobot.Common.Models.Commons;
using Zhalobobot.Common.Models.Subject;

namespace Zhalobobot.Bot.Helpers
{
    public static class WellKnownKeyboards
    {
        public static ReplyKeyboardMarkup DefaultKeyboard { get; } = new(
            new[]
            {
                new KeyboardButton[] { Buttons.Subjects },
                new KeyboardButton[] { Buttons.GeneralFeedback },
                new KeyboardButton[] { Buttons.Alarm }
            })
        {
            ResizeKeyboard = true
        };

        public static ReplyKeyboardMarkup SubmitKeyboard { get; } = new(
            new KeyboardButton[]
            {
                Buttons.Submit,
                Buttons.MainMenu
            })
        {
            ResizeKeyboard = true
        };

        public static InlineKeyboardMarkup ChooseCourseKeyboard { get; } =
            new(Enum.GetValues<Course>()
                .Select(course => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        course.AsString(EnumFormat.Description),
                        Utils.Join(Strings.Separator, CallbackDataPrefix.Course, course))
                }));

        public static InlineKeyboardMarkup RatingKeyboard { get; } =
            new (Enumerable.Range(1, 5)
                .Select(star => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        string.Join(" ", Enumerable.Repeat(Emoji.Star, star)),
                        Utils.Join(Strings.Separator, CallbackDataPrefix.Rating, star))
                }));
        
        public static InlineKeyboardMarkup SendFeedbackKeyboard(string subjectName) => 
            new []
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(
                        "Оставить обратную связь",
                        Utils.Join(Strings.Separator, CallbackDataPrefix.Feedback, subjectName))                
                }
            };

        public static InlineKeyboardMarkup GetSubjectCategoryKeyboard(Course course)
            =>  new(Enum.GetValues<SubjectCategory>()
                    .Select(category => new[]
                    {
                        InlineKeyboardButton.WithCallbackData(
                            category.AsString(EnumFormat.Description),
                            Utils.Join(Strings.Separator, CallbackDataPrefix.SubjectCategory, category, course))
                    })
                    .Append(new[]
                    {
                        InlineKeyboardButton.WithCallbackData(
                            Buttons.Back,
                            Utils.Join(Strings.Separator, CallbackDataPrefix.SubjectCategory, Strings.Back))
                    }));
    }
}