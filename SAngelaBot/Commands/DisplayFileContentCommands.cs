using Discord;
using Discord.Commands;
using SAngelaBot.Services;
using SAngelaBot.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace SAngelaBot.Commands
{
    public class DisplayFileContentCommands : ModuleBase<SocketCommandContext>
    {
        private readonly IFileHandlingService _fileService;
        public DisplayFileContentCommands(IFileHandlingService fileService)
        {
            _fileService = fileService;
        }

        [Command("DisplayRolesFile")]
        [RequireUserPermission(GuildPermission.KickMembers)]

        public async Task DisplayRolesNotPairedYet([Remainder] string args = null)
        {
            var guildPath = Context.Guild.Id.ToString() + "/";
            var fileContent = _fileService.ReadFromFile(StaticSettingsService.BasePath + guildPath + StaticSettingsService.RoleAssignmentChannelFilename);
            if (!fileContent.Any())
            {
                await ReplyAsync("File empty!");
            }
            else
            {
                string rowsFromFile = string.Join('\n', fileContent);
                await ReplyAsync(rowsFromFile);
            }
        }


    }
}
