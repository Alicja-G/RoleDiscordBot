using Discord;
using Discord.Commands;
using SAngelaBot.Services;
using SAngelaBot.Services.Interfaces;
using System.Threading.Tasks;

namespace SAngelaBot.Commands
{
    public class BadWordsCommands : ModuleBase<SocketCommandContext>
    {
        private readonly IFileHandlingService _fileService;
        public BadWordsCommands(IFileHandlingService fileService)
        {
            _fileService = fileService;
        }

        [Command("AddSlur")]
        [RequireUserPermission(GuildPermission.KickMembers)]

        public async Task AddBadWordAsync([Remainder] string args = null)
        {
            if (args == null)
            {
                await ReplyAsync("No word specified!");
                return;
            }

            var guildPath = Context.Guild.Id.ToString() + "/";

            if (StaticSettingsService.BadWords.ContainsKey(Context.Guild.Id))
                StaticSettingsService.BadWords[Context.Guild.Id].Add(args);
            else
                StaticSettingsService.BadWords.Add(Context.Guild.Id, new System.Collections.Generic.List<string> { args });

            var resultOfFileWritting = await _fileService.WriteToFileAsync(StaticSettingsService.BasePath + guildPath + StaticSettingsService.BadWordsFilename, new string[] { args });
            if (resultOfFileWritting.IsSuccessful)
                await ReplyAsync("Bad word added.");
            else
                await ReplyAsync("Bad word not added. More info: " + resultOfFileWritting.Message);
        }

        [Command("RemoveSlur")]
        [RequireUserPermission(GuildPermission.KickMembers)]

        public async Task RemoveSlurAsync([Remainder] string args = null)
        {
            if (args == null)
            {
                await ReplyAsync("No word specified!");
                return;
            }
            var guildPath = Context.Guild.Id.ToString() + "/";

            if (StaticSettingsService.BadWords.ContainsKey(Context.Guild.Id))
                StaticSettingsService.BadWords[Context.Guild.Id].Remove(args);

            var resultOfFileWritting = await _fileService.RemoveLineFromFileAsync(StaticSettingsService.BasePath + guildPath + StaticSettingsService.BadWordsFilename, args.ToString());
            if (resultOfFileWritting.IsSuccessful)
                await ReplyAsync("Slur removed.");
            else
                await ReplyAsync("Slur not removed. More info: " + resultOfFileWritting.Message);
        }


        [Command("ResetSlurs")]
        [RequireUserPermission(GuildPermission.KickMembers)]

        public async Task ResetSlurs([Remainder] string args = null)
        {
            if (StaticSettingsService.BadWords.ContainsKey(Context.Guild.Id))
            {

                StaticSettingsService.BadWords.Remove(Context.Guild.Id);
                await _fileService.ClearFileAsync(StaticSettingsService.BasePath + StaticSettingsService.GuildPath(Context.Guild.Id) + StaticSettingsService.BadWordsFilename);
                await ReplyAsync("Slurs cleared");
            }
            else
                await ReplyAsync("No slurs to clear!");
        }

    }
}
