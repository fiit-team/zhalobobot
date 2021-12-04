using System;

namespace Zhalobobot.Common.Models.Feedback.Requests
{
    public class AddFeedbackRequest
    {
        public AddFeedbackRequest(Feedback feedback)
        {
            Feedback = feedback ?? throw new ArgumentNullException(nameof(feedback));
        }
        
        public Feedback Feedback { get; }
    }
}