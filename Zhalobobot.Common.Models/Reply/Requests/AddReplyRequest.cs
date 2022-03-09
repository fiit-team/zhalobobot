using System;

namespace Zhalobobot.Common.Models.Reply.Requests
{
    public class AddReplyRequest
    {
        public AddReplyRequest(Reply reply)
        {
            Reply = reply ?? throw new ArgumentNullException(nameof(reply));
        }
        
        public Reply Reply { get; }
    }
}