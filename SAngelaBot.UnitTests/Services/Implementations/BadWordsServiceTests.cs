using FluentAssertions;
using NUnit.Framework;
using SAngelaBot.Services;
using SAngelaBot.Services.Implementations;
using System.Collections.Generic;

namespace SAngelaBot.UnitTests.Services.Implementations
{
    [TestFixture]
    public class BadWordsServiceTests
    {


        [TestCase("badword", (ulong)1111111, false)] //wrong guild
        [TestCase("badword", (ulong)2222222, true)] //correct guild, has bad word
        [TestCase("goodword", (ulong)2222222, false)] //correct guild, no bad word
        public void DoesContainBadWord_GetsDifferentGuildsAndPhrases_ReturnsExpectedValidationResult(string phrase, ulong guildId, bool expectedResult)
        {
            StaticSettingsService.BadWords.Add((ulong)2222222, new List<string> { "bad" });
            // Arrange
            var service = new BadWordsService();

            // Act
            var result = service.DoesContainBadWord(
                phrase,
                guildId);

            StaticSettingsService.BadWords.Remove((ulong)2222222);
            // Assert
            result.Should().Be(expectedResult);
        }
    }
}
