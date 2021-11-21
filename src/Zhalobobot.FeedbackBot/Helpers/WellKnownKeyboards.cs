using System;
using System.Linq;
using EnumsNET;
using Telegram.Bot.Types.ReplyMarkups;
using Zhalobobot.Bot.Models;
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
            new (Enum.GetValues<Course>()
                .Select(course => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        course.AsString(EnumFormat.Description),
                        $"{CallbackDataPrefix.Course}-{course}")
                }));
        
        public static InlineKeyboardMarkup SubjectCategoryKeyboard { get; } =
            new (Enum.GetValues<SubjectCategory>()
                .Select(category => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        category.AsString(EnumFormat.Description),
                        $"{CallbackDataPrefix.SubjectCategory}-{category}")
                }));

        public static InlineKeyboardMarkup RatingKeyboard { get; } =
            new (Enumerable.Range(1, 5)
                .Select(star => new[]
                {
                    InlineKeyboardButton.WithCallbackData(
                        string.Join(" ", Enumerable.Repeat(Emoji.Star, star)),
                        $"{CallbackDataPrefix.Rating}-{star}")
                }));
        
        public static InlineKeyboardMarkup SendFeedbackKeyboard(string subjectName) => 
            new []
            {
                new []
                {
                    InlineKeyboardButton.WithCallbackData(
                        "Оставить обратную связь",
                        $"{CallbackDataPrefix.Feedback}-{subjectName}")                
                }
            };
    }
}