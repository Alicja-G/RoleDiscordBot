using SAngelaBot.Services.Interfaces;

namespace SAngelaBot.Services.Implementations
{
    public class MentionBuilderService : IMentionBuilderService
    {
        public string BuildChannelMention(ulong channelId)
        {
            return "<#" + channelId + ">";
        }

        public string BuildRoleMention(ulong roleId)
        {
            return "<@&" + roleId + ">";
        }
    }
}