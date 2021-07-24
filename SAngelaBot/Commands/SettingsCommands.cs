using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SAngelaBot.Services;
using SAngelaBot.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace SAngelaBot.Commands
{
    public class SettingsCommands : ModuleBase<SocketCommandContext>
    {
        private readonly IFileHandlingService _fileService;
        public SettingsCommands(IFileHandlingService fileService)
        {
            _fileService = fileService;
        }

        [Command("SetBotCommandsChannel")]
        [RequireUserPermission(GuildPermission.BanMembers)]

        public async Task SetBotCommandsChannel([Remainder] string args = null)
        {
            var msg = Context.Message;
            var mentionedChannels = msg.MentionedChannels;
            if (!mentionedChannels.Any())
                await ReplyAsync("You haven't mentioned any channels!");
            else if (mentionedChannels.Count > 1)
                await ReplyAsync("More channels mentioned. Decide next time.");
            else
            {
                var channel = mentionedChannels.First();
                if (!StaticSettingsService.BotSpamChannelId.ContainsKey(Context.Guild.Id))
                {
                    StaticSettingsService.BotSpamChannelId.Add(Context.Guild.Id, channel.Id);
                }
                else
                    StaticSettingsService.BotSpamChannelId[Context.Guild.Id] = channel.Id;
                var resultOfFileWritting = await _fileService.RefreshFileAsync(StaticSettingsService.BasePath + GetGuildPath(Context.Guild) + StaticSettingsService.BotSpamFilename, new string[] { channel.Id.ToString() });
                if (resultOfFileWritting.IsSuccessful)
                    await ReplyAsync("Channel set.");
                else
                    await ReplyAsync("Channel not set. More info: " + resultOfFileWritting.Message);
            }
        }

        [Command("SetRolesChannel")]
        [RequireUserPermission(GuildPermission.BanMembers)]

        public async Task SetRolesChannel([Remainder] string args = null)
        {
            var msg = Context.Message;
            var mentionedChannels = msg.MentionedChannels;
            if (!mentionedChannels.Any())
                await ReplyAsync("You haven't mentioned any channels!");
            else if (mentionedChannels.Count > 1)
                await ReplyAsync("More channels mentioned. Decide next time.");
            else
            {
                var channel = mentionedChannels.First();
                if (!StaticSettingsService.RoleAssignmentChannelId.ContainsKey(Context.Guild.Id))
                {
                    StaticSettingsService.RoleAssignmentChannelId.Add(Context.Guild.Id, channel.Id);
                }
                else
                    StaticSettingsService.RoleAssignmentChannelId[Context.Guild.Id] = channel.Id;
                var resultOfFileWritting = await _fileService.RefreshFileAsync(StaticSettingsService.BasePath + GetGuildPath(Context.Guild) + StaticSettingsService.RoleAssignmentChannelFilename, new string[] { channel.Id.ToString() });
                if (resultOfFileWritting.IsSuccessful)
                    await ReplyAsync("Channel set.");
                else
                    await ReplyAsync("Channel not set. More info: " + resultOfFileWritting.Message);
            }
        }


        [Command("SetDefaultMsgsDeletionCounter")]
        [RequireUserPermission(GuildPermission.BanMembers)]

        public async Task SetDefaultMsgsDeletionCounter([Remainder] string args = null)
        {
            var result = Int32.TryParse(args, out int counter);
            if (!result || counter < 1)
                await ReplyAsync("Invalid given value. Try an integer greater than 1.");
            else
            {
                if (!StaticSettingsService.DefaultDeleteMsgsCounter.ContainsKey(Context.Guild.Id))
                    StaticSettingsService.DefaultDeleteMsgsCounter.Add(Context.Guild.Id, counter);
                else
                    StaticSettingsService.DefaultDeleteMsgsCounter[Context.Guild.Id] = counter;
                var resultOfFileWritting = await _fileService.RefreshFileAsync(StaticSettingsService.BasePath + GetGuildPath(Context.Guild) + StaticSettingsService.MsgsDeletionCounterFilename, new string[] { counter.ToString() });
                if (resultOfFileWritting.IsSuccessful)
                    await ReplyAsync("Counter not set.");
                else
                    await ReplyAsync("Counter not set. More info: " + resultOfFileWritting.Message);
            }
        }

        private string GetGuildPath(SocketGuild guild) => guild.Id.ToString() + "/";


    }
}
