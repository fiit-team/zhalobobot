using System.Collections.Generic;
using Telegram.Bot.Types;
using Zhalobobot.Common.Models.Feedback;

namespace Zhalobobot.Bot.Models
{
    public class Conversation
    {
        public Feedback Feedback { get; set; }
        public IList<Message> Messages { get; set; } = new List<Message>();
        public PollInfo LastPollInfo { get; set; }
    }
}