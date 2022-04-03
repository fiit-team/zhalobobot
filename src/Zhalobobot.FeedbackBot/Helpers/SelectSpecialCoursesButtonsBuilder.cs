using Telegram.Bot.Types.ReplyMarkups;
using Zhalobobot.Bot.Models;

namespace Zhalobobot.Bot.Helpers;

internal static class SelectSpecialCoursesButtonsBuilder
{
    public static InlineKeyboardMarkup Build(string[] items, int itemsPerPage, int currentPage)
        => ItemsButtonsPaginationBuilder.BuildWithSubmitButton(
            items,
            currentPage,
            itemsPerPage,
            CallbackDataPrefix.PaginationItemButton,
            CallbackDataPrefix.PaginationButton,
            CallbackDataPrefix.SubmitSpecialCourses);
}