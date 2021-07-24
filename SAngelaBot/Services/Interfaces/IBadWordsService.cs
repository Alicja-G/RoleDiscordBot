namespace SAngelaBot.Services.Interfaces
{
    public interface IBadWordsService
    {
        bool DoesContainBadWord(string msg, ulong guildId);

    }
}
