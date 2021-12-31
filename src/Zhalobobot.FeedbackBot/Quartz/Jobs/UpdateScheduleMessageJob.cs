using System.Collections.Generic;
using System.Threading.Tasks;
using Quartz;
using Telegram.Bot.Exceptions;
using Zhalobobot.Bot.Services;
using Zhalobobot.Common.Models.Commons;
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

            var messagesToDelete = new HashSet<(long ChatId, string Data, int MessageId, DayAndMonth WhenDelete)>();
            
            var currentDay = DateHelper.EkbTime.ToDayAndMonth();
            while (iterator.MoveNext())
            {
                if (iterator.Current.WhenDelete <= currentDay)
                    messagesToDelete.Add(iterator.Current);
                else
                {
                    try
                    {
                        await HandleUpdateService.HandleChooseScheduleRange(iterator.Current.ChatId,
                            iterator.Current.Data,
                            iterator.Current.MessageId);
                    }
                    catch (MessageIsNotModifiedException)
                    {
                        // continue
                    }
                }
            }

            foreach (var message in messagesToDelete)
                ScheduleMessageService.RemoveMessageToUpdate(message);
        }
    }
}