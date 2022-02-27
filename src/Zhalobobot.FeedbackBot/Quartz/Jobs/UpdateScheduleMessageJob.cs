using System.Collections.Generic;
using System.Threading.Tasks;
using Quartz;
using Telegram.Bot.Exceptions;
using Zhalobobot.Bot.Services;
using Zhalobobot.Common.Models.Helpers;

namespace Zhalobobot.Bot.Quartz.Jobs
{
    [DisallowConcurrentExecution]
    public class UpdateScheduleMessageJob : IJob
    {
        private IScheduleMessageService ScheduleMessageService { get; }
        private HandleUpdateService HandleUpdateService { get; }

        public UpdateScheduleMessageJob(IScheduleMessageService scheduleMessageService, HandleUpdateService handleUpdateService)
        {
            ScheduleMessageService = scheduleMessageService;
            HandleUpdateService = handleUpdateService;
        }
        
        public async Task Execute(IJobExecutionContext context)
        {
            var iterator = ScheduleMessageService.GetAll();

            var chatsToDelete = new HashSet<long>();
            
            var currentDay = DateHelper.EkbTime.ToDateOnly();
            while (iterator.MoveNext())
            {
                if (iterator.Current.Value.WhenDelete <= currentDay)
                    chatsToDelete.Add(iterator.Current.Key);
                else
                {
                    try
                    {
                        await HandleUpdateService.HandleChooseScheduleRange(iterator.Current.Key,
                            iterator.Current.Value.Data,
                            iterator.Current.Value.MessageId);
                    }
                    catch (MessageIsNotModifiedException)
                    {
                        // continue
                    }
                }
            }

            foreach (var chatId in chatsToDelete)
                ScheduleMessageService.RemoveMessageToUpdate(chatId);
        }
    }
}