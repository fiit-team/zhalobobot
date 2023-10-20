// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Quartz;
// using Zhalobobot.Bot.Services;
// using Zhalobobot.Common.Models.Helpers;
// using Zhalobobot.TelegramMessageQueue;
// using Zhalobobot.TelegramMessageQueue.Core;
//
// namespace Zhalobobot.Bot.Quartz.Jobs
// {
//     [DisallowConcurrentExecution]
//     public class UpdateScheduleMessageJob : IJob
//     {
//         private MessageSender MessageSender { get; }
//         private IScheduleMessageService ScheduleMessageService { get; }
//         private HandleUpdateService HandleUpdateService { get; }
//
//         public UpdateScheduleMessageJob(MessageSender messageSender, IScheduleMessageService scheduleMessageService, HandleUpdateService handleUpdateService)
//         {
//             MessageSender = messageSender;
//             ScheduleMessageService = scheduleMessageService;
//             HandleUpdateService = handleUpdateService;
//         }
//         
//         public async Task Execute(IJobExecutionContext context)
//         {
//             var iterator = ScheduleMessageService.GetAll();
//
//             var chatsToDelete = new HashSet<long>();
//             
//             var currentDay = DateHelper.EkbTime.ToDateOnly();
//             while (iterator.MoveNext())
//             {
//                 if (iterator.Current.Value.WhenDelete <= currentDay)
//                     chatsToDelete.Add(iterator.Current.Key);
//                 else
//                 {
//                     MessageSender.SendToUser(
//                         () => HandleUpdateService.HandleChooseScheduleRange(
//                             iterator.Current.Key,
//                             iterator.Current.Value.Data,
//                             iterator.Current.Value.MessageId),
//                         MessagePriority.Low);
//                 }
//             }
//
//             foreach (var chatId in chatsToDelete)
//                 ScheduleMessageService.RemoveMessageToUpdate(chatId);
//         }
//     }
// }