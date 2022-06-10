//Created by Alexander Fields https://github.com/roku674
using Discord;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace DiscordBotUpdates.Modules
{
    public class BotUpdater
    {
        public class DBUTask
        {
            internal bool isCancelled = false;

            public DBUTask(Task _task, string _purpose, string _owner, uint _id)
            {
                timeStarted = DateTime.Now;

                id = _id;
                owner = _owner;
                purpose = _purpose;
                task = _task;
            }

            public uint id { get; set; }
            public string owner { get; set; }
            public string purpose { get; set; }
            public Task task { get; set; }
            public DateTime timeStarted { get; set; }

            public void Cancel()
            {
                isCancelled = true;
            }
        }

        private static readonly int _duration = 604800;

        private static uint _dbuTaskNum;
        private static List<DBUTask> _runningTasks = new List<DBUTask>();

        /// <summary>
        /// BotUpdates Id
        /// </summary>
        private static ulong botUpdatesID = 979100384037568582;

        private string _botUpdatesStr = "";

        public static int duration => _duration;

        public static uint dbuTaskNum { get => _dbuTaskNum; set => _dbuTaskNum = value; }
        public static List<DBUTask> runningTasks { get => _runningTasks; set => _runningTasks = value; }

        public string botUpdatesStr { get => _botUpdatesStr; set => _botUpdatesStr = value; }

        public static async Task Outprint(string message)
        {
            IMessageChannel channel = Program.client.GetChannel(botUpdatesID) as IMessageChannel;
            await channel.SendMessageAsync(message);
        }

        /// <summary>
        /// Call this to start the Message Updater
        /// </summary>
        /// <returns></returns>
        public async Task MessageBotUpdates(uint id)
        {
            System.Console.WriteLine("MessageBotUpdates Executed!");

            IMessageChannel channel = Program.client.GetChannel(botUpdatesID) as IMessageChannel;
            await channel.SendMessageAsync("Sucessfully Initiated Message Listener!");

            int taskNum = _runningTasks.FindIndex(task => task.id == id);

            for (int i = 0; i < _duration; i++)
            {
                if (_runningTasks[taskNum].isCancelled)
                {
                    i = _duration;
                    break;
                }
                await Task.Delay(1000);

                if (File.Exists(Directory.GetCurrentDirectory() + "/Channel/botUpdates.txt"))
                {
                    botUpdatesStr = File.ReadAllText(Directory.GetCurrentDirectory() + "/Channel/botUpdates.txt");

                    if (!string.IsNullOrEmpty(botUpdatesStr))
                    {
                        //System.Console.WriteLine(botUpdatesStr);
                        await channel.SendMessageAsync(botUpdatesStr);
                        File.WriteAllText(Directory.GetCurrentDirectory() + "/Channel/botUpdates.txt", "");
                    }
                }
                else
                {
                    File.Create(Directory.GetCurrentDirectory() + "/Channel/botUpdates.txt");
                    await channel.SendMessageAsync("Created botUpdates.txt ! Recommend ReRunning!");
                    i = _duration;
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

            IMessageChannel channel = Program.client.GetChannel(botUpdatesID) as IMessageChannel;
            await channel.SendMessageAsync("Sucessfully Initiated Picture Listener!");

            int taskNum = _runningTasks.FindIndex(task => task.id == id);

            for (int i = 0; i < _duration; i++)
            {
                if (_runningTasks[taskNum].isCancelled)
                {
                    i = _duration;
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