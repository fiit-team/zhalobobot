using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Zhalobobot.Bot.Models
{
    public class FeedbackInfo
    {
        public FeedbackType Type { get; set; }
        public string Message { get; set; }
        public string Subject { get; set; }
    }
}
