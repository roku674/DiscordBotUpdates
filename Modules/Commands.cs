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
        public async Task BotUpdaterCommand()
        {
            BotUpdater botUpdater = new BotUpdater();
            _ = Task.Run(() => botUpdater.MessageBotUpdates());
            await ReplyAsync("By Your Command! Listening for messages for " + BotUpdater.duration + " seconds!");
        }
    }
}