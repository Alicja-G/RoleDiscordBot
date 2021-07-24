using Discord;

namespace SAngelaBot.Services.Interfaces
{
    public interface IEmbedMessageBuilder
    {
        EmbedBuilder CreateEmbedForRecievedMsg(IMessage msg);
        EmbedBuilder CreateEmbedForBadWordMsg(IMessage msg);
    }
}
