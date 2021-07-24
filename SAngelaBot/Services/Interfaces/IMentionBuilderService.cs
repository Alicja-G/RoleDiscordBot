namespace SAngelaBot.Services.Interfaces
{
    public interface IMentionBuilderService
    {
        public string BuildChannelMention(ulong channelId);
        public string BuildRoleMention(ulong roleId);
    }
}
