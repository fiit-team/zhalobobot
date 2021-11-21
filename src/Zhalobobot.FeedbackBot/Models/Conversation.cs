using System.Collections.Generic;
using Zhalobobot.Common.Models.Feedback;

namespace Zhalobobot.Bot.Models
{
    public class Conversation
    {
        public Feedback Feedback { get; set; }
        public IList<string> Messages { get; set; } = new List<string>();
        public PollInfo LastPollInfo { get; set; }
    }
}