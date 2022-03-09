using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Zhalobobot.Api.Server.Repositories.Common;
using Zhalobobot.Common.Models.Reply;

namespace Zhalobobot.Api.Server.Repositories.Feedback
{
    public class ReplyRepository : GoogleSheetsRepositoryBase, IReplyRepository
    {
        private IConfiguration Configuration { get; }
        private ILogger<FeedbackRepository> Logger { get; }
        private string RepliesRange { get; }

        public ReplyRepository(IConfiguration configuration, ILogger<FeedbackRepository> logger)
            : base(configuration, configuration["FeedbackSpreadSheetId"])
        {
            Configuration = configuration;
            Logger = logger;
            RepliesRange = configuration["RepliesRange"];
        }

        public async Task Add(Reply reply)
        {
            var objectList = new List<object>
            {
                reply.UserId,
                reply.Username,
                reply.ChatId,
                reply.MessageId,
                reply.Message,
                reply.ChildChatId,
                reply.ChildMessageId
            };
            
            await AppendRequest(RepliesRange, objectList).ExecuteAsync();
        }

        public async Task<Reply[]> GetAll()
        {
            var values = await GetRequest(RepliesRange).ExecuteAsync();

            if (values.Values is null)
            {
                return new Reply[0];
            }

            return values.Values.Select(reply => new Reply(
                    ParsingHelper.ParseLong(reply[0]),
                    reply[1] as string ?? string.Empty,
                    ParsingHelper.ParseLong(reply[2]),
                    ParsingHelper.ParseInt(reply[3]),
                    reply[4] as string ?? string.Empty,
                    ParsingHelper.ParseLong(reply[5]),
                    ParsingHelper.ParseInt(reply[6])))
                .ToArray();
        }
    }
}