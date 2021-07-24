using SAngelaBot.Models;
using System.Collections.Generic;

namespace SAngelaBot.Services
{
    public static class StaticSettingsService
    {

        public static bool IS_PRODUCTION_MODE = true;
        public static bool IS_ON_VPS = true;

        public static string GetCommandChar() => "_";
        public static string GetTOKEN() => ""; //TODO: get from file

        public static Dictionary<ulong, ulong> BotSpamChannelId = new Dictionary<ulong, ulong>();
        public static Dictionary<ulong, ulong> RoleAssignmentChannelId = new Dictionary<ulong, ulong>();
        public static Dictionary<ulong, List<RolesToReactionAssignmentModel>> RolesToReactionsAssignments = new Dictionary<ulong, List<RolesToReactionAssignmentModel>>();
        public static Dictionary<ulong, List<string>> BadWords = new Dictionary<ulong, List<string>>();
        public static Dictionary<ulong, int> DefaultDeleteMsgsCounter = new Dictionary<ulong, int>();

        public static string BasePath;

        public static string BotSpamFilename = "botcommandschannelangella.txt";
        public static string RoleAssignmentChannelFilename = "roleassignmentchannel.txt";
        public static string RoleAssignmentsFilename = "roleassignments.txt";
        public static string MsgsDeletionCounterFilename = "msgsDeletioncounter.txt";
        public static string BadWordsFilename = "badwords.txt";

        public static string GuildPath(ulong guildId) => guildId.ToString() + "/";
    }

}
