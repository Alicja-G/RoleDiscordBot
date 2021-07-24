using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SAngelaBot.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SAngelaBot.Commands
{
    public class DeleteMsgsCommands : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// Delete msgs from channel. You can provide channel name or id 
        /// like deletemsgs #channel | counter (default 20) 
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("deletemsgs")]
        [RequireUserPermission(GuildPermission.KickMembers)]

        public async Task Msgs([Remainder] string args = null)
        {
            var msg = Context.Message;
            var mentionedChannels = msg.MentionedChannels;
            ulong channelId;
            int additionalCounter = 0;
            if (mentionedChannels.Any() && mentionedChannels.Count > 1)
            {
                await ReplyAsync("For some reason I'm only allowed to delete messages from only one channel at the time. Ask The Developer for the reason (possible reason being *just because*)");
                return;
            }
            else if (mentionedChannels.Any())
            {
                channelId = mentionedChannels.First().Id;
                additionalCounter = 1;
            }
            else
                channelId = Context.Channel.Id;

            var settings = new string[] { };
            if (args != null && args.Contains("|"))
            {
                settings = args.Split("|");
            }
            int counter = 0;
            if (settings.Any())
            {
                var counterFromArgumentResult = int.TryParse(settings[1], out int counterFromContent);
                if (counterFromArgumentResult)
                    counter = counterFromContent;
                else
                {
                    if (StaticSettingsService.DefaultDeleteMsgsCounter.ContainsKey(Context.Guild.Id) && StaticSettingsService.DefaultDeleteMsgsCounter[Context.Guild.Id] != 0)
                        counter = StaticSettingsService.DefaultDeleteMsgsCounter[Context.Guild.Id];
                    else
                        counter = 20;
                }
            }
            else
            {
                if (StaticSettingsService.DefaultDeleteMsgsCounter.ContainsKey(Context.Guild.Id) && StaticSettingsService.DefaultDeleteMsgsCounter[Context.Guild.Id] != 0)
                    counter = StaticSettingsService.DefaultDeleteMsgsCounter[Context.Guild.Id];
                else
                    counter = 20;
            }

            var channel = Context.Guild.GetChannel(channelId);
            var msgs = await (channel as ISocketMessageChannel).GetMessagesAsync(counter + additionalCounter).FlattenAsync();
            await (channel as SocketTextChannel).DeleteMessagesAsync(msgs);
        }


    }
}
