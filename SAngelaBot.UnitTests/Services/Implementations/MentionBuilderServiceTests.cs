using FluentAssertions;
using NUnit.Framework;
using SAngelaBot.Services.Implementations;

namespace SAngelaBot.UnitTests.Services.Implementations
{
    [TestFixture]
    public class MentionBuilderServiceTests
    {

        private MentionBuilderService CreateService()
        {
            return new MentionBuilderService();
        }

        [Test]
        public void BuildChannelMention_GetsChannelId_ReturnsExpectedMentionFormat()
        {
            // Arrange
            var service = this.CreateService();
            ulong channelId = 111111111;

            // Act
            var result = service.BuildChannelMention(
                channelId);

            // Assert
            result.Should().Be("<#111111111>");
        }

        [Test]
        public void BuildRoleMention_GetsRoleId_ReturnsExpectedMentionFormat()
        {
            // Arrange
            var service = this.CreateService();
            ulong roleId = 111111111;

            // Act
            var result = service.BuildRoleMention(
                roleId);

            // Assert
            result.Should().Be("<@&111111111>");
        }
    }
}
