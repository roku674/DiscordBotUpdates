//Created by Alexander Fields https://github.com/roku674
using Discord.Commands;
using System.Threading.Tasks;

namespace DiscordBotUpdates.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        [Command("Ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong");
        }

        [Command("run BotUpdater")]
        public async Task BotUpdater()
        {
            Modules.BotUpdater.botUpdatesBool = true;
            Modules.BotUpdater.Run();
            await ReplyAsync("By Your Command! Listening for messages indefinitely");
        }
    }
}