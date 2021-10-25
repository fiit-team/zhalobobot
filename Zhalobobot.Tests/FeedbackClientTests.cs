using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Zhalobobot.Common.Clients.Core;
using Zhalobobot.Common.Models.Feedback;

namespace Zhalobobot.Tests
{
    [TestFixture]
    public class FeedbackClientTests
    {
        private readonly IZhalobobotApiClient client;

        public FeedbackClientTests(IZhalobobotApiClient client)
        {
            this.client = client;
        }

        [Test]
        public async Task Feedback_add_should_complete_successful()
        {
            var feedback = Feedback.General with {Subject = new Subject("Subject"), Message = "Message"};
            
            var result = await client.Feedback.AddFeedback(feedback);

            result.IsSuccessful.Should().BeTrue();
        }
    }
}