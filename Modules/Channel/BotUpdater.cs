//Created by Alexander Fields https://github.com/roku674
using Discord;
using System.IO;
using System.Threading.Tasks;

namespace DiscordBotUpdates.Modules
{
    internal class BotUpdater : DBUTask
    {
        private static ulong _botUpdatesID = 979100384037568582;

        /// <summary>
        /// BotUpdates channel Id
        /// </summary>
        public static ulong botUpdatesID { get => _botUpdatesID; set => _botUpdatesID = value; }

        /// <summary>
        /// Call this to start the Message Updater
        /// </summary>
        /// <returns></returns>
        public async Task MessageBotUpdates(uint id)
        {
            System.Console.WriteLine("MessageBotUpdates Executed!");

            IMessageChannel channel = Program.client.GetChannel(_botUpdatesID) as IMessageChannel;
            await channel.SendMessageAsync("Sucessfully Initiated Message Listener!");

            int taskNum = runningTasks.FindIndex(task => task.id == id);

            for (int i = 0; i < duration; i++)
            {
                if (runningTasks[taskNum].isCancelled)
                {
                    i = duration;
                    break;
                }
                await Task.Delay(1000);

                if (File.Exists(Directory.GetCurrentDirectory() + "/Channel/botUpdates.txt"))
                {
                    dbuString = File.ReadAllText(Directory.GetCurrentDirectory() + "/Channel/botUpdates.txt");

                    if (!string.IsNullOrEmpty(dbuString))
                    {
                        //System.Console.WriteLine(dbuString);
                        await channel.SendMessageAsync(dbuString);
                        File.WriteAllText(Directory.GetCurrentDirectory() + "/Channel/botUpdates.txt", "");
                    }
                }
                else
                {
                    File.Create(Directory.GetCurrentDirectory() + "/Channel/botUpdates.txt");
                    await channel.SendMessageAsync("Created botUpdates.txt ! Recommend ReRunning!");
                    i = duration;
                }
            }
            await channel.SendMessageAsync("No Longer Listening for MessageUpdates!");

            dbuTaskNum--;
        }

        /// <summary>
        /// Call this to start the picture updater
        /// </summary>
        /// <returns></returns>
        public async Task PictureBotUpdates(uint id)
        {
            System.Console.WriteLine("PictureBotUpdates Executed!");

            IMessageChannel channel = Program.client.GetChannel(_botUpdatesID) as IMessageChannel;
            await channel.SendMessageAsync("Sucessfully Initiated Picture Listener!");

            int taskNum = runningTasks.FindIndex(task => task.id == id);

            for (int i = 0; i < duration; i++)
            {
                if (runningTasks[taskNum].isCancelled)
                {
                    i = duration;
                    break;
                }
                string[] paths = Directory.GetFiles(Directory.GetCurrentDirectory() + "/Pictures", "*.png");
                await Task.Delay(1000);

                if (paths.Length > 0)
                {
                    foreach (string path in paths)
                    {
                        System.Console.WriteLine(path);
                        await channel.SendFileAsync(path, Path.GetFileName(path));
                        File.Delete(path);
                    }
                }
            }
            await channel.SendMessageAsync("No Longer Listening for Pictures Updates!");

            dbuTaskNum--;
        }
    }
}