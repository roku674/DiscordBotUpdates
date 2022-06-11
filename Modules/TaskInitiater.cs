//Created by Alexander Fields https://github.com/roku674
using Discord;
using System.IO;
using System.Threading.Tasks;

namespace DiscordBotUpdates.Modules
{
    internal class TaskInitiater : DBUTask
    {
        /// </summary>

        /// <summary>
        /// Call this to start the Distress Calls Listener
        /// </summary>
        /// <returns></returns>
        public async Task DistressCallsListener(uint id, ulong channelID)
        {
            System.Console.WriteLine("DistressCalls Executed!");

            IMessageChannel channel = Program.client.GetChannel(channelID) as IMessageChannel;
            await channel.SendMessageAsync("Sucessfully Distress Calls Listener!");

            int taskNum = runningTasks.FindIndex(task => task.id == id);

            for (int i = 0; i < duration; i++)
            {
                if (runningTasks[taskNum].isCancelled)
                {
                    i = duration;
                    break;
                }
                await Task.Delay(1000);

                watcher = new FileSystemWatcher();
                watcher.Path = path;
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                       | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Filter = "*.*";
                watcher.Changed += new FileSystemEventHandler(OnChanged);
                watcher.EnableRaisingEvents = true;
            }
            await channel.SendMessageAsync("No Longer Listening for Distress Signalss!");

            dbuTaskNum--;
        }

        /// <summary>
        /// Call this to start the Message Updater
        /// </summary>
        /// <returns></returns>
        public async Task MessageBotUpdates(uint id, ulong channelID)
        {
            System.Console.WriteLine("MessageBotUpdates Executed!");

            IMessageChannel channel = Program.client.GetChannel(channelID) as IMessageChannel;
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
        public async Task PictureBotUpdates(uint id, ulong channelID)
        {
            System.Console.WriteLine("PictureBotUpdates Executed!");

            IMessageChannel channel = Program.client.GetChannel(channelID) as IMessageChannel;
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

        /// <summary>
        /// Call this to start the Shutdown Listener
        /// </summary>
        /// <returns></returns>
        public async Task ServerShutdownListener(uint id, ulong channelID)
        {
            System.Console.WriteLine("MessageBotUpdates Executed!");

            IMessageChannel channel = Program.client.GetChannel(channelID) as IMessageChannel;
            await channel.SendMessageAsync("Sucessfully Started Server Shutdown Listener!");

            int taskNum = runningTasks.FindIndex(task => task.id == id);

            for (int i = 0; i < duration; i++)
            {
                if (runningTasks[taskNum].isCancelled)
                {
                    i = duration;
                    break;
                }
                await Task.Delay(1000);
            }
            await channel.SendMessageAsync("No Longer Listening for Server Shutdown Mesages!");

            dbuTaskNum--;
        }
    }
}