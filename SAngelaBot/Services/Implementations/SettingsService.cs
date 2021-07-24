using Discord.WebSocket;
using SAngelaBot.Enums;
using SAngelaBot.Models;
using SAngelaBot.Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SAngelaBot.Services.Implementations
{
    public class SettingsService : ISettingsService
    {
        private readonly IFileHandlingService _fileService;

        public SettingsService(IFileHandlingService fileService)
        {
            _fileService = fileService;
        }

        public async Task Set(IEnumerable<SocketGuild> guilds)
        {
            await Task.Run(async () =>
            {
                await this.SetBasePath();
                await this.CreateGuildFolders(guilds);
                await this.SetBotChannelId(guilds);
                await this.SetRolesChannelId(guilds);
                await this.SetDefaultMsgsDeletionCounter(guilds);
                await this.SetBadWordsList(guilds);
                await this.SetRolesAndReactionsAssignment(guilds);
            });
        }

        private async Task CreateGuildFolders(IEnumerable<SocketGuild> guilds)
        {
            foreach (var guild in guilds)
            {
                var dirPath = StaticSettingsService.BasePath + guild.Id.ToString();
                if (!Directory.Exists(dirPath))
                    Directory.CreateDirectory(dirPath);
            }
        }

        private async Task SetBasePath()
        {
            var executingAssembly = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var drive = Path.GetPathRoot(executingAssembly);

            var middlePath = "home/bot/settings/prod/"; //VPS
            if (!StaticSettingsService.IS_PRODUCTION_MODE && !StaticSettingsService.IS_ON_VPS)
                middlePath = "settings/"; //localComp
            else if (!StaticSettingsService.IS_PRODUCTION_MODE && StaticSettingsService.IS_ON_VPS)
                middlePath = "home/bot/settings/test/";
            StaticSettingsService.BasePath = drive + middlePath;
        }

        private async Task SetBadWordsList(IEnumerable<SocketGuild> guilds)
        {
            foreach (var guild in guilds)
            {
                var guildPath = GetGuildPath(guild);
                var contentFromFile = _fileService.ReadFromFile(StaticSettingsService.BasePath + guildPath + StaticSettingsService.BadWordsFilename);
                if (contentFromFile != null && contentFromFile.Any())
                {
                    StaticSettingsService.BadWords.Add(guild.Id, contentFromFile.ToList());
                }
                else
                    StaticSettingsService.BadWords.Add(guild.Id, new List<string>());
            }


        }

        private async Task SetBotChannelId(IEnumerable<SocketGuild> guilds)
        {
            foreach (var guild in guilds)
            {
                var guildPath = GetGuildPath(guild);
                var contentFromFile = _fileService.ReadFromFile(StaticSettingsService.BasePath + guildPath + StaticSettingsService.BotSpamFilename);
                if (contentFromFile != null && contentFromFile.Any())
                {
                    var result = ulong.TryParse(contentFromFile.First(), out ulong channelId);
                    if (result && !StaticSettingsService.BotSpamChannelId.ContainsKey(guild.Id))
                        StaticSettingsService.BotSpamChannelId.Add(guild.Id, channelId);
                }
            }
        }

        private async Task SetRolesChannelId(IEnumerable<SocketGuild> guilds)
        {
            foreach (var guild in guilds)
            {
                var guildPath = GetGuildPath(guild);
                var contentFromFile = _fileService.ReadFromFile(StaticSettingsService.BasePath + guildPath + StaticSettingsService.RoleAssignmentChannelFilename);
                if (contentFromFile != null && contentFromFile.Any())
                {
                    var result = ulong.TryParse(contentFromFile.First(), out ulong channelId);
                    if (result && !StaticSettingsService.RoleAssignmentChannelId.ContainsKey(guild.Id))
                        StaticSettingsService.RoleAssignmentChannelId.Add(guild.Id, channelId);
                }
            }
        }

        private async Task SetDefaultMsgsDeletionCounter(IEnumerable<SocketGuild> guilds)
        {
            foreach (var guild in guilds)
            {
                var guildPath = GetGuildPath(guild);
                var contentFromFile = _fileService.ReadFromFile(StaticSettingsService.BasePath + guildPath + StaticSettingsService.MsgsDeletionCounterFilename);
                if (contentFromFile != null && contentFromFile.Any())
                {
                    var result = int.TryParse(contentFromFile.First(), out int counter);
                    if (result)
                    {
                        StaticSettingsService.DefaultDeleteMsgsCounter.Add(guild.Id, counter);
                        return;
                    }
                }
            }
        }
        private async Task SetRolesAndReactionsAssignment(IEnumerable<SocketGuild> guilds)
        {
            foreach (var guild in guilds)
            {
                var guildPath = GetGuildPath(guild);
                var contentFromFile = _fileService.ReadFromFile(StaticSettingsService.BasePath + guildPath + StaticSettingsService.RoleAssignmentsFilename);
                if (contentFromFile != null && contentFromFile.Any())
                {
                    var rolesAssignemnts = new List<RolesToReactionAssignmentModel>();
                    foreach (var row in contentFromFile)
                    {
                        var splitted = row.Split("|");
                        if (splitted.Count() >= 3)
                        {
                            var canConvertMsgId = ulong.TryParse(splitted[(int)RolesAssignmentIndexInFile.MESSAGE_ID_INDEX_IN_FILE], out ulong msgId);
                            var canConvertRecId = ulong.TryParse(splitted[(int)RolesAssignmentIndexInFile.REACTION_ID_INDEX_IN_FILE], out ulong recId);
                            var canConvertRoleId = ulong.TryParse(splitted[(int)RolesAssignmentIndexInFile.ROLE_ID_INDEX_IN_FILE], out ulong roleId);

                            if (canConvertRecId && canConvertRoleId && canConvertMsgId)
                            {
                                var assignedRole = new RolesToReactionAssignmentModel
                                {
                                    MessageId = msgId,
                                    ReactionId = recId,
                                    RoleId = roleId
                                };
                                rolesAssignemnts.Add(assignedRole);
                            }
                        }

                    }
                    StaticSettingsService.RolesToReactionsAssignments.Add(guild.Id, rolesAssignemnts);
                }
                else
                    StaticSettingsService.RolesToReactionsAssignments.Add(guild.Id, new List<RolesToReactionAssignmentModel>());
            }
        }


        private string GetGuildPath(SocketGuild guild) => guild.Id.ToString() + "/";

    }
}
