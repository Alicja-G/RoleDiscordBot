using SAngelaBot.Services.Interfaces;
using System.Linq;

namespace SAngelaBot.Services.Implementations
{
    public class BadWordsService : IBadWordsService
    {

        public bool DoesContainBadWord(string msg, ulong guildId)
        {
            if (StaticSettingsService.BadWords.ContainsKey(guildId))
            {
                var words = msg.Split(' ');
                foreach (var word in words)
                {
                    if (StaticSettingsService.BadWords[guildId].Any(w => word.Contains(w)))
                        return true;
                }
            }
            return false;
        }
    }
}
