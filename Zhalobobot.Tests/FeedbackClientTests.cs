using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Zhalobobot.Common.Clients.Feedback;
using Zhalobobot.Common.Models.Feedback;

namespace Zhalobobot.Tests
{
    [TestFixture]
    public class FeedbackClientTests
    {
        private IFeedbackClient client = null!;
        
        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            client = new FeedbackClient();
        }

        [Test]
        public async Task Feedback_add_should_complete_successful()
        {
            var feedback = Feedback.General with {Subject = new Subject("Subject"), Message = "Message"};
            
            var result = await client.AddFeedback(feedback);

            result.IsSuccessful.Should().BeTrue();
        }
    }
}