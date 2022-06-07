//Created by Alexander Fields https://github.com/roku674
using Discord;
using System.IO;
using System.Threading.Tasks;

namespace DiscordBotUpdates.Modules
{
    public class BotUpdater
    {
        private static readonly int _duration = 604800;
        private string _botUpdatesStr = "";

        public static int duration => _duration;

        public string botUpdatesStr { get => _botUpdatesStr; set => _botUpdatesStr = value; }

        public async Task MessageBotUpdates()
        {
            System.Console.WriteLine("MessageBotUpdates Executed!");
            ulong id = 979100384037568582;
            var channel = Program.client.GetChannel(id) as IMessageChannel;

            for (int i = 0; i < _duration; i++)
            {
                await Task.Delay(1000);

                if (File.Exists(Directory.GetCurrentDirectory() + "/botUpdates.txt"))
                {
                    botUpdatesStr = File.ReadAllText(Directory.GetCurrentDirectory() + "/botUpdates.txt");

                    if (!string.IsNullOrEmpty(botUpdatesStr))
                    {
                        //System.Console.WriteLine(botUpdatesStr);
                        await channel.SendMessageAsync(botUpdatesStr);
                        File.WriteAllText(Directory.GetCurrentDirectory() + "/botUpdates.txt", "");
                    }
                }
                else
                {
                    File.Create(Directory.GetCurrentDirectory() + "/botUpdates.txt");
                    await channel.SendMessageAsync("Created botUpdates.txt !");
                    i = _duration;
                }
            }
        }
    }
}