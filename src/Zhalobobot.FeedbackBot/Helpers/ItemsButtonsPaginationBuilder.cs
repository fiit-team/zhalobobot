using System;
using System.Collections.Generic;
using System.Linq;
using Telegram.Bot.Types.ReplyMarkups;
using Zhalobobot.Bot.Models;

namespace Zhalobobot.Bot.Helpers;

internal static class ItemsButtonsPaginationBuilder
{
    public static InlineKeyboardMarkup BuildWithSubmitButton(
        string[] items, 
        int currentPage, 
        int itemsPerPage,
        string itemButtonCallbackPrefix,
        string paginationButtonCallbackPrefix,
        string submitButtonCallbackPrefix)
    {
        if (itemsPerPage <= 0 || items.Length == 0)
            return InlineKeyboardMarkup.Empty();
        
        var pagesCount = (int)Math.Ceiling((double)items.Length / itemsPerPage);
        if (currentPage >= pagesCount)
            return InlineKeyboardMarkup.Empty();

        var buttons = BuildItemsButtons(items, currentPage, itemsPerPage, itemButtonCallbackPrefix)
            .Concat(BuildPaginationButtons(currentPage, pagesCount, itemsPerPage, paginationButtonCallbackPrefix))
            .Append(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData(" ", CallbackDataPrefix.Nothing) })
            .Append(new List<InlineKeyboardButton> { InlineKeyboardButton.WithCallbackData("Подтвердить выбор", string.Join(Strings.Separator, submitButtonCallbackPrefix, "")) });
        
        return new InlineKeyboardMarkup(buttons);
    }
    
    private static IEnumerable<List<InlineKeyboardButton>> BuildItemsButtons(IEnumerable<string> items, int currentPage, int itemsPerPage, string itemButtonCallbackPrefix)
    {
        var itemsToShow = items
            .Skip(itemsPerPage * currentPage)
            .Take(itemsPerPage)
            .Select((i, ind) => (i, ind));

        foreach (var (itemToShow, ind) in itemsToShow)
        {
            yield return new List<InlineKeyboardButton>
            {
                InlineKeyboardButton.WithCallbackData(
                    itemToShow,
                    string.Join(Strings.Separator, itemButtonCallbackPrefix, itemsPerPage, currentPage, ind))
            };
        }
    }
    
    private static IEnumerable<List<InlineKeyboardButton>> BuildPaginationButtons(int currentPage, int pagesCount, int itemsPerPage, string paginationButtonCallbackPrefix)
    {
        if (pagesCount <= 1)
            yield break;
        
        yield return new List<InlineKeyboardButton>(pagesCount)
        {
            InlineKeyboardButton.WithCallbackData("<<", string.Join(Strings.Separator, paginationButtonCallbackPrefix, itemsPerPage, (pagesCount + currentPage - 1) % pagesCount)),
            InlineKeyboardButton.WithCallbackData($"{currentPage + 1} из {pagesCount}", CallbackDataPrefix.Nothing),
            InlineKeyboardButton.WithCallbackData(">>", string.Join(Strings.Separator, paginationButtonCallbackPrefix, itemsPerPage, (currentPage + 1) % pagesCount))
        };
    }
}