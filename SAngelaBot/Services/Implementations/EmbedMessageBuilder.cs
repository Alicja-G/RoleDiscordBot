using Discord;
using SAngelaBot.Services.Interfaces;

namespace SAngelaBot.Services.Implementations
{
    public class EmbedMessageBuilder : IEmbedMessageBuilder
    {
        public EmbedBuilder CreateEmbedForBadWordMsg(IMessage msg)
        {
            EmbedBuilder embed = CreateEmbed(msg, "message deleted bc forbidden words");
            embed.WithColor(80, 200, 150);
            return embed;
        }

        public EmbedBuilder CreateEmbedForRecievedMsg(IMessage msg)
        {
            EmbedBuilder embed = CreateEmbed(msg, "message recieved");
            embed.WithColor(40, 200, 150);
            return embed;
        }

        private EmbedBuilder CreateEmbed(IMessage msg, string fieldName)
        {
            EmbedBuilder embed = new EmbedBuilder();

            embed.AddField(fieldName, msg.ToString());
            embed.Author = new EmbedAuthorBuilder
            {
                Name = msg.Author.Username,
                IconUrl = msg.Author.GetAvatarUrl()
            };
            embed.Footer = new EmbedFooterBuilder
            {
                Text = msg.Channel.Name
            };
            embed.Timestamp = msg.Timestamp;
            return embed;
        }
    }
}
