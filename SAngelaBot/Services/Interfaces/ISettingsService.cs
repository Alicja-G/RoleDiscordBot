using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SAngelaBot.Services.Interfaces
{
    public interface ISettingsService
    {
        public Task Set(IEnumerable<SocketGuild> guilds);
    }
}
