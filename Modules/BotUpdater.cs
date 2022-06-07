//Created by Alexander Fields https://github.com/roku674
using Discord;
using System.IO;
using System.Threading.Tasks;

namespace DiscordBotUpdates.Modules
{
    public class BotUpdater
    {
        /// <summary>
        /// BotUpdates Id
        /// </summary>
        private ulong botUpdatesID = 979100384037568582;

        private static readonly int _duration = 604800;
        private string _botUpdatesStr = "";

        public static int duration => _duration;

        public string botUpdatesStr { get => _botUpdatesStr; set => _botUpdatesStr = value; }

        public async Task MessageBotUpdates()
        {
            System.Console.WriteLine("MessageBotUpdates Executed!");

            var channel = Program.client.GetChannel(botUpdatesID) as IMessageChannel;

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
                    await channel.SendMessageAsync("Created botUpdates.txt ! Recommend ReRunning!");
                    i = _duration;
                }
            }
        }

        public async Task PictureBotUpdates()
        {
            System.Console.WriteLine("PictureBotUpdates Executed!");

            var channel = Program.client.GetChannel(botUpdatesID) as IMessageChannel;

            for (int i = 0; i < _duration; i++)
            {
                string[] paths = Directory.GetFiles(Directory.GetCurrentDirectory() + "/pictures", "*.png");
                await Task.Delay(1000);

                if (paths.Length > 0)
                {
                    foreach (string path in paths)
                    {
                        System.Console.WriteLine(path);
                        await channel.SendFileAsync(path);
                        File.Delete(path);
                    }
                }
            }
        }
    }
}