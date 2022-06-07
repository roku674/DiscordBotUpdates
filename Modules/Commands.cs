//Created by Alexander Fields https://github.com/roku674
using Discord.Commands;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace BotUpdates.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private int duration = 86400;

        [Command("Ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong");
        }

        [Command("Destroy NWO")]
        public async Task DestroyNWO()
        {
            await ReplyAsync("By Your Command!");
        }

        [Command("run BotUpdater")]
        public async Task BotUpdater()
        {
            await ReplyAsync("By Your Command!");

            for (int i = 0; i < duration; i++)
            {
                await Task.Delay(1000);

                if (File.Exists(Directory.GetCurrentDirectory() + "/botUpdates.txt"))
                {
                    Program.botUpdates = File.ReadAllText(Directory.GetCurrentDirectory() + "/botUpdates.txt");

                    if (!string.IsNullOrEmpty(Program.botUpdates))
                    {
                        System.Console.WriteLine(Program.botUpdates);
                        await ReplyAsync(Program.botUpdates);
                        File.WriteAllText(Directory.GetCurrentDirectory() + "/botUpdates.txt", "");
                    }
                }
                else
                {
                    File.Create(Directory.GetCurrentDirectory() + "/botUpdates.txt");
                    await ReplyAsync("Created botUpdates.txt !");
                    i = duration;
                }
            }
            await ReplyAsync("No Longer Checking Bot Updates!");
        }
    }
}