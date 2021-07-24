using Discord;
using Discord.Commands;
using Discord.WebSocket;
using SAngelaBot.Enums;
using SAngelaBot.Models;
using SAngelaBot.Services;
using SAngelaBot.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace SAngelaBot.Commands
{
    public class RolesCommands : ModuleBase<SocketCommandContext>
    {
        private readonly IFileHandlingService _fileService;
        public RolesCommands(IFileHandlingService fileService)
        {
            _fileService = fileService;
        }

        /// <summary>
        /// ids or emotes or mixed |
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("AssignRole")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task AssignRole([Remainder] string args = null)
        {
            if (args == null || !args.Contains("|"))
            {
                await ReplyAsync("Invalid content. Try msgId|@role|Emote");
                return;
            }

            var msg = Context.Message;
            ulong msgId;
            ulong roleId;
            ulong mentionedId;
            var splitted = args.Split("|");

            var canParseMsgId = ulong.TryParse(splitted[(int)RolesAssignmentIndexInFile.MESSAGE_ID_INDEX_IN_FILE], out msgId);
            if (!canParseMsgId)
            {
                await ReplyAsync("Invalid content. Invalid msgId");
                return;
            }

            var rolesMentioned = msg.MentionedRoles;
            if (rolesMentioned.Any() && rolesMentioned.Count == 1)
            {
                var roleMentioned = rolesMentioned.First();
                roleId = roleMentioned.Id;
            }
            else
            {
                await ReplyAsync("Invalid content. Invalid roleId");
                return;
            }

            var emteName = string.Empty;
            var emotePart = splitted[(int)RolesAssignmentIndexInFile.REACTION_ID_INDEX_IN_FILE];
            if (emotePart.Contains("<") && emotePart.Contains(">"))
            {
                var indexFrom = emotePart.IndexOf("<");
                var indexTo = emotePart.IndexOf(">");
                var len = indexTo - indexFrom;
                var emoteString = emotePart.Substring(indexFrom, len);
                var id = emoteString.Where(d => char.IsDigit(d));
                emteName = new string(emoteString.Where(d => char.IsLetter(d)).ToArray());
                mentionedId = ulong.Parse(string.Join(string.Empty, id));
            }
            else
            {
                var canRecIdBeParsed = ulong.TryParse(emotePart, out mentionedId);
                if (!canRecIdBeParsed)
                {
                    await ReplyAsync("Invalid content. Invalid emoteId");
                    return;
                }
            }

            if (StaticSettingsService.RolesToReactionsAssignments.ContainsKey(Context.Guild.Id))
            {
                var assignmentsForGuild = StaticSettingsService.RolesToReactionsAssignments[Context.Guild.Id];
                if (assignmentsForGuild.Any(c => c.RoleId == roleId))
                {
                    await ReplyAsync("Role already assingned!");
                    return;
                }

                var rolesAssignedForMsg = assignmentsForGuild.Where(d => d.MessageId ==
                msgId);
                if (rolesAssignedForMsg.Any(c => c.ReactionId == mentionedId))
                {
                    await ReplyAsync("Reaction already used in this msg!");
                    return;
                }

                var emote = Context.Guild.Emotes.FirstOrDefault(d => d.Id == mentionedId);
                if (emote == null)
                {
                    await ReplyAsync("Emote not found in this server! Try again.");
                    return;
                }

                var role = Context.Guild.Roles.FirstOrDefault(d => d.Id == roleId);
                if (role == null)
                {
                    await ReplyAsync("Role not found in this server! Try again!");
                    return;
                }

            }

            var assignmentToAdd = new Models.RolesToReactionAssignmentModel
            {
                ReactionId = mentionedId,
                MessageId = msgId,
                RoleId = roleId
            };
            var assignment = assignmentToAdd.ToString();
            if (!StaticSettingsService.RolesToReactionsAssignments.ContainsKey(Context.Guild.Id))
            {
                StaticSettingsService.RolesToReactionsAssignments.TryAdd(Context.Guild.Id, new System.Collections.Generic.List<RolesToReactionAssignmentModel> { assignmentToAdd });
            }
            else
            {
                StaticSettingsService.RolesToReactionsAssignments[Context.Guild.Id].Add(assignmentToAdd);
            }

            if (StaticSettingsService.RoleAssignmentChannelId.ContainsKey(Context.Guild.Id))
            {
                var roleChannel = Context.Guild.GetChannel(StaticSettingsService.RoleAssignmentChannelId[Context.Guild.Id]) as ISocketMessageChannel;
                var messageToReact = roleChannel.GetMessageAsync(msgId);
                var messageToReactResult = messageToReact.Result;
                var emotesFromSErver = Context.Guild.Emotes;
                IEmote emoteX = emotesFromSErver.FirstOrDefault(c => c.Id == mentionedId);
                IEmote emote = emotesFromSErver.FirstOrDefault(d => d.Name == emteName);
                if (emoteX != null)
                {
                    await messageToReactResult.AddReactionAsync(emoteX);
                }
            }


            await _fileService.WriteToFileAsync(StaticSettingsService.BasePath + StaticSettingsService.GuildPath(Context.Guild.Id) + StaticSettingsService.RoleAssignmentsFilename, new string[] { assignment });

            await ReplyAsync("Role assigned");
        }

        /// <summary>
        /// roleId|roleId|roleID
        /// @role|@role|@role  ///multiple  roles!
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        [Command("DeleteRoleAssignment")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task DeleteRoleFromReaction([Remainder] string args = null)
        {
            var msg = Context.Message;
            var roleMentioned = msg.MentionedRoles;
            if (roleMentioned.Any())
            {
                if (StaticSettingsService.RolesToReactionsAssignments.ContainsKey(Context.Guild.Id))
                {
                    foreach (var role in roleMentioned)
                    {
                        var assignedRole = StaticSettingsService.RolesToReactionsAssignments[Context.Guild.Id].FirstOrDefault(d => d.RoleId == role.Id);
                        if (assignedRole != null)
                        {
                            StaticSettingsService.RolesToReactionsAssignments[Context.Guild.Id].Remove(assignedRole);
                            await _fileService.RemoveLineFromFileAsync(StaticSettingsService.BasePath + StaticSettingsService.GuildPath(Context.Guild.Id) + StaticSettingsService.RoleAssignmentsFilename, assignedRole.ToString());

                            var roleChannel = Context.Guild.GetChannel(StaticSettingsService.RoleAssignmentChannelId[Context.Guild.Id]) as ISocketMessageChannel;
                            var messageToReact = roleChannel.GetMessageAsync(assignedRole.MessageId);
                            var messageToReactResult = messageToReact.Result;
                            var emotesFromSErver = Context.Guild.Emotes;
                            var user = Context.Guild.GetUser(759537090814935072);
                            IEmote emoteX = emotesFromSErver.FirstOrDefault(c => c.Id == assignedRole.ReactionId);
                            if (emoteX != null)
                            {
                                await messageToReactResult.RemoveReactionAsync(emoteX, user);
                            }

                        }


                    }
                    await ReplyAsync("Roles removed.");
                    return;
                }

                await ReplyAsync("No roles set yet! Can't remove.");
                return;
            }
            await ReplyAsync("No roles mentioned!");

        }

        [Command("DeleteAssignedMessage")]
        [RequireUserPermission(GuildPermission.KickMembers)]
        public async Task DeleteAssignedMessage([Remainder] string args = null)
        {
            var content = args;
            if (content == null)
            {
                await ReplyAsync("No msg id given!");
                return;
            }
            var guildId = Context.Guild.Id;

            if (StaticSettingsService.RolesToReactionsAssignments.ContainsKey(guildId))
            {
                ulong msgId;
                var resultOfparsing = ulong.TryParse(content, out msgId);

                if (!resultOfparsing)
                {
                    await ReplyAsync("Invalid argument given!");
                    return;
                }

                var assignmentsToMsgFromSettings = StaticSettingsService.RolesToReactionsAssignments[guildId].Where(d => d.MessageId == msgId);

                if (assignmentsToMsgFromSettings.Any())
                {
                    StaticSettingsService.RolesToReactionsAssignments[guildId].RemoveAll(x => x.MessageId == msgId);
                    var filePath = StaticSettingsService.BasePath + StaticSettingsService.GuildPath(guildId) + StaticSettingsService.RoleAssignmentChannelFilename;
                    var result = _fileService.RemoveLineWithContentInIndexPlaceAsync(filePath, msgId.ToString(), (int)RolesAssignmentIndexInFile.MESSAGE_ID_INDEX_IN_FILE);
                    await ReplyAsync("Message removed.");
                }
            }
            else
                await ReplyAsync("No roles set yet! Can't remove.");
        }



    }
}



