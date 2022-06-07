//Created by Alexander Fields https://github.com/roku674
using Discord;
using System.IO;
using System.Threading;

namespace DiscordBotUpdates.Modules
{
    public static class BotUpdater
    {
        private static string _botUpdatesStr = "";
        private static bool _botUpdatesBool;

        public static string botUpdatesStr { get => _botUpdatesStr; set => _botUpdatesStr = value; }
        public static bool botUpdatesBool { get => _botUpdatesBool; set => _botUpdatesBool = value; }

        public static void Run()
        {
            if (botUpdatesBool)
            {
                Timer timer = new Timer(MessageBotUpdates, null, 0, 1000);
                System.Console.WriteLine("Run");
            }
        }

        private static void MessageBotUpdates(object o)
        {
            ulong id = 979100384037568582;
            var channel = Program.client.GetChannel(id) as IMessageChannel;

            if (File.Exists(Directory.GetCurrentDirectory() + "/botUpdates.txt"))
            {
                _botUpdatesStr = File.ReadAllText(Directory.GetCurrentDirectory() + "/botUpdates.txt");

                if (!string.IsNullOrEmpty(_botUpdatesStr))
                {
                    System.Console.WriteLine(_botUpdatesStr);
                    channel.SendMessageAsync(_botUpdatesStr);
                    File.WriteAllText(Directory.GetCurrentDirectory() + "/botUpdates.txt", "");
                }
            }
            else
            {
                File.Create(Directory.GetCurrentDirectory() + "/botUpdates.txt");
                channel.SendMessageAsync("Created botUpdates.txt !");
            }
        }
    }
}