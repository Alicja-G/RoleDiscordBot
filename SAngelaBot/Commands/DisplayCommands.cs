using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SAngelaBot.Models;
using SAngelaBot.Services;
using SAngelaBot.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SAngelaBot.Commands
{
    public class DisplayCommands : ModuleBase<SocketCommandContext>
    {
        private readonly IFileHandlingService _fileService;
        private readonly IMentionBuilderService _mentionBuilderService;
        public DisplayCommands(IFileHandlingService fileService, IMentionBuilderService mentionBuilderService)
        {
            _fileService = fileService;
            _mentionBuilderService = mentionBuilderService;
        }


        [Command("DisplayBadWords")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task DisplayBadWords([Remainder] string args = null)
        {
            var guildPath = Context.Guild.Id.ToString() + "/";
            var resultOfFileReading = _fileService.ReadFromFile(StaticSettingsService.BasePath + guildPath + StaticSettingsService.BadWordsFilename);
            if (!resultOfFileReading.Any())
                await ReplyAsync("No bad words found!");
            else
                await ReplyAsync(string.Join("\n", resultOfFileReading));
        }


        [Command("DisplayCurrentRolesChannel")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task DisplayCurrentRolesChannel([Remainder] string args = null)
        {
            var guildPath = Context.Guild.Id.ToString() + "/";
            var resultOfFileReading = _fileService.ReadFromFile(StaticSettingsService.BasePath + guildPath + StaticSettingsService.RoleAssignmentChannelFilename);
            if (!resultOfFileReading.Any())
            {
                await ReplyAsync("No channel set!");
                return;
            }
            else
            {
                var isIdValid = ulong.TryParse(resultOfFileReading.First(), out ulong channelId);
                if (isIdValid)
                {
                    var channel = Context.Guild.GetChannel(channelId);
                    if (channel != null)
                    {
                        await ReplyAsync("Current channel for roles is: " + _mentionBuilderService.BuildChannelMention(channelId));
                        return;
                    }
                    await ReplyAsync("Invalid channelId in file. Reset file and try again");
                    return;
                }
                else
                    await ReplyAsync("File has incorrect data! Reset file and try setup the correct channel again!");
            }
        }

        [Command("DisplayCurrentBotSpamChannel")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task DisplayCurrentBotSpamChannel([Remainder] string args = null)
        {
            var guildPath = Context.Guild.Id.ToString() + "/";
            var resultOfFileReading = _fileService.ReadFromFile(StaticSettingsService.BasePath + guildPath + StaticSettingsService.BotSpamFilename);
            if (!resultOfFileReading.Any())
            {
                await ReplyAsync("No channel set!");
                return;
            }
            else
            {
                var isIdValid = ulong.TryParse(resultOfFileReading.First(), out ulong channelId);
                if (isIdValid)
                {
                    var channel = Context.Guild.GetChannel(channelId);
                    if (channel != null)
                    {
                        await ReplyAsync("Current channel for bot spam is: " + _mentionBuilderService.BuildChannelMention(channelId));
                        return;
                    }
                    await ReplyAsync("Invalid channelId in file. Reset file and try again");
                    return;
                }
                else
                    await ReplyAsync("File has incorrect data! Reset file and try setup the correct channel again!");
            }
        }

        [Command("DisplayCurrentDeleteMsgsCounter")]
        [RequireUserPermission(GuildPermission.KickMembers)]

        public async Task DisplayCurrentMsgCounter([Remainder] string args = null)
        {
            var guildPath = Context.Guild.Id.ToString() + "/";
            var resultOfFileReading = _fileService.ReadFromFile(StaticSettingsService.BasePath + guildPath + StaticSettingsService.MsgsDeletionCounterFilename);
            if (!resultOfFileReading.Any())
                await ReplyAsync("No counter found!");
            else
            {
                var isIdValid = int.TryParse(resultOfFileReading.First(), out int counter);
                if (isIdValid)
                {
                    await ReplyAsync("Current default counter is: " + counter.ToString());
                }
                else
                    await ReplyAsync("File has incorrect data! Reset file and try setup the correct counter again!");
            }
        }

        [Command("DisplayMsgsInRoleChannel")]
        [RequireUserPermission(GuildPermission.KickMembers)]

        public async Task DisplayMsgsInRoleChannel([Remainder] string args = null)
        {
            if (StaticSettingsService.RoleAssignmentChannelId.ContainsKey(Context.Guild.Id))
            {
                var channelId = StaticSettingsService.RoleAssignmentChannelId[Context.Guild.Id];
                var channel = Context.Guild.Channels.FirstOrDefault(x => x.Id == channelId);
                var msgs = await (channel as SocketTextChannel).GetMessagesAsync().FlattenAsync();
                var msgsOrdered = msgs.OrderBy(d => d.CreatedAt);

                string outputText = string.Empty;
                foreach (var msg in msgsOrdered)
                {
                    outputText += ("Msg: \n" + msg.Content + "\n \n has an Id: " + msg.Id + "\n");
                };
                await ReplyAsync(outputText);
            }
            else
                await ReplyAsync("No roles channel specified");
        }


        [Command("DisplayEmotesInfoFromChannel")]
        [RequireUserPermission(GuildPermission.KickMembers)]

        public async Task DisplayEmotesInfoFromChannel([Remainder] string args = null)
        {
            if (StaticSettingsService.RoleAssignmentChannelId.ContainsKey(Context.Guild.Id))
            {
                var channelId = StaticSettingsService.RoleAssignmentChannelId[Context.Guild.Id];
                var channel = Context.Guild.Channels.FirstOrDefault(x => x.Id == channelId);
                var msgs = await (channel as SocketTextChannel).GetMessagesAsync().FlattenAsync();
                var msgsOrdered = msgs.OrderBy(d => d.CreatedAt);

                string output = string.Empty;
                foreach (var msg in msgs)
                {
                    var reactionsForMessage = msg.Reactions;
                    var reactionsString = string.Empty;
                    foreach (var reaction in reactionsForMessage)
                    {
                        var reactionAsEmote = reaction.Key as Emote;
                        if (reactionAsEmote != null)
                        {
                            var reactionEmoteId = reactionAsEmote.Id;
                            var reactionEmoteName = reactionAsEmote.Name;
                            var emoteInThisGuild = Context.Guild.Emotes.FirstOrDefault(c => c.Id == reactionEmoteId);
                            if (emoteInThisGuild != null)
                                reactionsString += reactionEmoteName + " | " + reactionEmoteId + " \n";
                            else
                                reactionsString += "Emote: " + reactionEmoteName + " is not from this server. \n";
                        }
                        else
                        {
                            var emoji = reaction.Key as Emoji;
                            reactionsString += "Invalid emoji: " + emoji.Name;
                        }

                    }
                    await ReplyAsync("Msg: \n" + msg.Content + "\n Id: " + msg.Id + "\n" + reactionsString + "\n");
                };
            }
            await ReplyAsync("No roles channel specified");
        }


        [Command("DisplayRolesPairedToMsgsAndEmotes")]
        [RequireUserPermission(GuildPermission.KickMembers)]

        public async Task DisplayRolesPairedToMsgsAndEmotes([Remainder] string args = null)
        {
            if (StaticSettingsService.RoleAssignmentChannelId.ContainsKey(Context.Guild.Id))
            {
                if (!StaticSettingsService.RolesToReactionsAssignments.ContainsKey(Context.Guild.Id))
                {
                    await ReplyAsync("No roles assigned yet!");
                    return;
                }
                var assignedRolesForGuild = StaticSettingsService.RolesToReactionsAssignments[Context.Guild.Id];
                if (!assignedRolesForGuild.Any())
                {
                    await ReplyAsync("No roles assigned yet!");
                    return;
                }

                var output = string.Empty;
                var channelId = StaticSettingsService.RoleAssignmentChannelId[Context.Guild.Id];
                var channel = Context.Guild.Channels.FirstOrDefault(x => x.Id == channelId);
                var msgs = await (channel as SocketTextChannel).GetMessagesAsync().FlattenAsync();
                foreach (var role in assignedRolesForGuild)
                {
                    var msgsId = role.MessageId;
                    var msg = msgs.FirstOrDefault(d => d.Id == msgsId);
                    if (msg != null)
                    {
                        var reaction = msg.Reactions.Select(c => c.Key as Emote).FirstOrDefault(d => d.Id == role.ReactionId);
                        if (reaction != null)
                        {
                            var rolez = Context.Guild.GetRole(role.RoleId);
                            if (rolez != null)
                                output += "Role " + rolez.Name + " with ID: " + role.RoleId +
                                    " is assigned to reaction emote " + reaction.Name + " with ID " + reaction.Id + " to msgID "
                                    + role.MessageId + "\n";
                            else
                                output += "Invalid roleId in settings! Remove line: \n" +
                               role.MessageId + "|" + role.ReactionId + "|" + role.RoleId;
                        }
                        else
                            output += "Invalid reactionId in settings! Remove line: \n" +
                           role.MessageId + "|" + role.ReactionId + "|" + role.RoleId;
                    }
                    else
                        output += "Invalid msgsId in settings! Remove line: \n" +
                            role.MessageId + "|" + role.ReactionId + "|" + role.RoleId;
                }

                await ReplyAsync(output);
            }
            else
                await ReplyAsync("No roles channel specified");

        }


        [Command("DisplayRolesNotPairedYet")]
        [RequireUserPermission(GuildPermission.KickMembers)]

        public async Task DisplayRolesNotPairedYet([Remainder] string args = null)
        {
            var assignedRoles = new List<RolesToReactionAssignmentModel>();
            if (StaticSettingsService.RolesToReactionsAssignments.ContainsKey(Context.Guild.Id))
            {
                assignedRoles = StaticSettingsService.RolesToReactionsAssignments[Context.Guild.Id];
            }

            var resultRoles = string.Empty;
            var roleIdsFromFile = assignedRoles.Select(d => d.RoleId);
            var rolesFromGuild = Context.Guild.Roles;
            var rolesNotPaired = rolesFromGuild.Where(d => !roleIdsFromFile.Contains(d.Id));

            var counter = 0;

            foreach (var role in rolesNotPaired)
            {
                counter++;
                if (!role.Permissions.KickMembers && !role.Name.ToLower().Contains("everyone"))
                    resultRoles += "Role " + role.Name + " with ID " + role.Id + "\n";

                if (counter == 5)
                {
                    counter = 0;
                    await ReplyAsync(resultRoles);
                    resultRoles = string.Empty;
                }
            }

            await ReplyAsync(resultRoles);
        }


    }
}
