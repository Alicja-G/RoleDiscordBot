using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using SAngelaBot.Enums;
using SAngelaBot.Services;
using SAngelaBot.Services.Implementations;
using SAngelaBot.Services.Interfaces;
using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SAngelaBot
{
    class Program
    {
        static void Main(string[] args) => new
          Program().RunBotAsync().GetAwaiter().GetResult();

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;
        private IBadWordsService _badWordsService = new BadWordsService();
        private IEmbedMessageBuilder _embededMessageBuilder = new EmbedMessageBuilder();
        private IFileHandlingService _fileServie = new FileHandlingService();


        public async Task RunBotAsync()
        {
            var config = new DiscordSocketConfig { MessageCacheSize = 100 };
            _client = new DiscordSocketClient(config);
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddScoped<IFileHandlingService, FileHandlingService>()
                .AddScoped<IReactionHandlingService, ReactionHandlingService>()
                .AddScoped<ISettingsService, SettingsService>()
                .AddScoped<IMentionBuilderService, MentionBuilderService>()
                .AddScoped<IBadWordsService, BadWordsService>()
                .AddScoped<IEmbedMessageBuilder, EmbedMessageBuilder>()
                .BuildServiceProvider();

            //TODO Register services based on IService and not manually via reflection

            _client.Log += _client_Log;

            await RegisterCommandsAsync();

            await _client.LoginAsync(TokenType.Bot, StaticSettingsService.GetTOKEN());

            await _client.StartAsync();

            await Task.Delay(-1);
        }


        private async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;
            _client.ReactionAdded += HandleReactionAddedAsync;
            _client.ReactionRemoved += HandleReactionRemovedAsync;
            _client.RoleCreated += HandleRoleCreatedAsync;
            _client.RoleDeleted += HandleRoleDeletedAsync;
            _client.Connected += HandleClientConnectedAsync;
            _client.MessageDeleted += HandleMessageDeletedAsync;
            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleMessageDeletedAsync(Cacheable<IMessage, ulong> arg1, ISocketMessageChannel arg2)
        {
            await Task.Run(async () =>
            {
                var channel = arg2 as SocketTextChannel;
                var guildId = channel.Guild.Id;
                if (StaticSettingsService.RoleAssignmentChannelId.ContainsKey(guildId)
                && StaticSettingsService.RolesToReactionsAssignments.ContainsKey(guildId))
                {
                    var channelId = channel.Id;
                    var msgId = arg1.Id;
                    var channelIdFromSettings = StaticSettingsService.RoleAssignmentChannelId[guildId];
                    if (channel.Id == channelIdFromSettings)
                    {
                        var assignmentsToMsgFromSettings = StaticSettingsService.RolesToReactionsAssignments[guildId].Where(d => d.MessageId == msgId);

                        if (assignmentsToMsgFromSettings.Any())
                        {
                            StaticSettingsService.RolesToReactionsAssignments[guildId].RemoveAll(x => x.MessageId == msgId);
                            var filePath = StaticSettingsService.BasePath + GetGuildPath(channel.Guild) + StaticSettingsService.RoleAssignmentChannelFilename;
                            var result = _fileServie.RemoveLineWithContentInIndexPlaceAsync(filePath, msgId.ToString(), (int)RolesAssignmentIndexInFile.MESSAGE_ID_INDEX_IN_FILE);

                            if (StaticSettingsService.BotSpamChannelId.ContainsKey(guildId))
                            {
                                var guild = channel.Guild;
                                var botSpamChannel = guild.GetChannel(StaticSettingsService.BotSpamChannelId[guildId]) as SocketTextChannel;
                                await botSpamChannel.SendMessageAsync("Message was deleted with role reactions assigned. Check the assignments.");
                            }
                        }
                    }
                }
            });
        }

        private async Task HandleClientConnectedAsync()
        {
            await Task.Run(() =>
            {
                var clientConnectedThread = new Thread(new ThreadStart(ClientConnectedThread));
                clientConnectedThread.Start();
            });
        }

        private void ClientConnectedThread()
        {
            Task.Run(async () =>
            {
                var settingsService = new SettingsService(new FileHandlingService());
                var guilds = _client.Guilds;
                await settingsService.Set(guilds);

            });
        }

        private async Task HandleRoleDeletedAsync(SocketRole arg)
        {
            await Task.Run(async () =>
            {
                var guildId = arg.Guild.Id;
                if (StaticSettingsService.BotSpamChannelId.ContainsKey(guildId))
                {
                    var modsChannel = _client.GetChannel(StaticSettingsService.BotSpamChannelId[guildId]) as SocketTextChannel;
                    await modsChannel.SendMessageAsync("Role \n" + arg.Name + "|" + arg.Id + " deleted. Update role file?");
                }
            });
        }

        private async Task HandleRoleCreatedAsync(SocketRole arg)
        {
            await Task.Run(async () =>
            {
                var guildId = arg.Guild.Id;
                if (StaticSettingsService.BotSpamChannelId.ContainsKey(guildId))
                {
                    var modsChannel = _client.GetChannel(StaticSettingsService.BotSpamChannelId[guildId]) as SocketTextChannel;
                    await modsChannel.SendMessageAsync("Role \n" + arg.Name + "|" + arg.Id + " created. Update role file?");
                }
            });
        }

        private async Task HandleReactionRemovedAsync(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            await Task.Run(async () =>
            {
                var channel = arg2 as SocketTextChannel;
                if (StaticSettingsService.RoleAssignmentChannelId.ContainsKey(channel.Guild.Id))
                {
                    var reaction = arg3.Emote as Emote;
                    var reactionId = reaction.Id;
                    var msgId = arg3.MessageId;
                    var channelId = arg3.Channel.Id;

                    if (channelId == StaticSettingsService.RoleAssignmentChannelId[channel.Guild.Id]
                        && StaticSettingsService.RolesToReactionsAssignments.ContainsKey(channel.Guild.Id))
                    {
                        var roleAssignments = StaticSettingsService.RolesToReactionsAssignments[channel.Guild.Id];
                        var roleAssignedToMsg = roleAssignments.Where(d => d.MessageId == msgId);
                        if (roleAssignedToMsg.Any())
                        {
                            var roleWithReaction = roleAssignedToMsg.FirstOrDefault(d => d.ReactionId == reactionId);
                            if (roleWithReaction != null)
                            {
                                var role = channel.Guild.GetRole(roleWithReaction.RoleId);
                                if (role != null)
                                {
                                    var user = (SocketGuildUser)arg3.User;
                                    if (user.Roles.Contains(role))
                                    {
                                        await user.RemoveRoleAsync(role);
                                        if (StaticSettingsService.BotSpamChannelId.ContainsKey(channel.Guild.Id))
                                        {
                                            var botSpamChannel = _client.GetChannel(StaticSettingsService.BotSpamChannelId[channel.Guild.Id]) as SocketTextChannel;
                                            await botSpamChannel.SendMessageAsync("Role " + role.Name + " removed from user " + user.Username);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            });
        }

        private async Task HandleReactionAddedAsync(Cacheable<IUserMessage, ulong> arg1, ISocketMessageChannel arg2, SocketReaction arg3)
        {
            await Task.Run(async () =>
            {
                var userReacted = (SocketGuildUser)arg3.User;
                if (userReacted.IsBot)
                    return;

                var channel = arg2 as SocketTextChannel;

                if (StaticSettingsService.RoleAssignmentChannelId.ContainsKey(channel.Guild.Id))
                {
                    var channelId = arg3.Channel.Id;
                    if (channelId == StaticSettingsService.RoleAssignmentChannelId[channel.Guild.Id])
                    {
                        var reaction = arg3.Emote as Emote;
                        if (reaction == null)
                            return;

                        var reactionId = reaction.Id;
                        var msgId = arg3.MessageId;
                        var roleAssignments = StaticSettingsService.RolesToReactionsAssignments[channel.Guild.Id];
                        var roleAssignedToMsg = roleAssignments.Where(d => d.MessageId == msgId);
                        if (roleAssignedToMsg.Any())
                        {
                            var roleWithReaction = roleAssignedToMsg.FirstOrDefault(d => d.ReactionId == reactionId);
                            if (roleWithReaction != null)
                            {
                                var role = channel.Guild.GetRole(roleWithReaction.RoleId);
                                if (role != null)
                                {
                                    var user = (SocketGuildUser)arg3.User;
                                    await user.AddRoleAsync(role);
                                    if (StaticSettingsService.BotSpamChannelId.ContainsKey(channel.Guild.Id))
                                    {
                                        var botSpamChannel = _client.GetChannel(StaticSettingsService.BotSpamChannelId[channel.Guild.Id]) as SocketTextChannel;
                                        await botSpamChannel.SendMessageAsync("Role " + role.Name + " added to user " + user.Username);
                                    }

                                }
                            }
                        }
                    }
                }
            });

        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            await Task.Run(async () =>
            {
                var message = arg as SocketUserMessage;
                var context = new SocketCommandContext(_client, message);
                var chan = message.Channel as SocketGuildChannel;
                if (chan == null)
                    return;
                var guld = chan.Guild ?? null;
                if (guld == null)
                    return;

                var userGuild = context.User as SocketGuildUser;
                if (message.Author.IsBot) return;


                if (_badWordsService.DoesContainBadWord(message.Content, chan.Guild.Id) &&
                !userGuild.GuildPermissions.KickMembers)
                {
                    await context.Channel.DeleteMessageAsync(message);
                    var user = context.User;
                    await user.SendMessageAsync("Your message was deleted for using inappropriate language. Your message: \n" + message.Content);
                    if (StaticSettingsService.BotSpamChannelId.ContainsKey(context.Guild.Id))
                    {
                        var modsChannel = _client.GetChannel(StaticSettingsService.BotSpamChannelId[context.Guild.Id]) as SocketTextChannel;
                        await modsChannel.SendMessageAsync(embed: _embededMessageBuilder.CreateEmbedForBadWordMsg(message).Build());
                    }
                    return;
                }


                int argPos = 0;
                if (message.HasStringPrefix(StaticSettingsService.GetCommandChar(), ref argPos))
                {
                    await Task.Run(async () =>
                    {
                        await _commands.ExecuteAsync(context, argPos, _services);
                        return;
                    });
                }


            });
        }

        private Task _client_Log(LogMessage arg)
        {
            Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        private string GetGuildPath(SocketGuild guild) => guild.Id.ToString() + "/";
    }
}
