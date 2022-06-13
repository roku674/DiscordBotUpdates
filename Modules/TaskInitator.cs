//Created by Alexander Fields https://github.com/roku674
using Discord;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotUpdates.Modules
{
    internal class TaskInitator : DBUTask
    {
        private string[] enemies =
        {
            "Altair","Awmalzo","B-radk.","Dad","Deegs", "DOG-WHISPERER",
            "Flint", "Meshuggah","McGee","Pluto","Presto", "Revelation",
            "RepealThe2ndA","Scar-Face"
        };

        private string[] allies =
        {
            "Allie", "Anxiety.jar", "Bodhi", "CaptArcher",
            "Depression.Wav",  "Devila", "Hokujinn",
            "Leaderkiller","Probation", "Taterchip", "WW3"
        };

        private FileSystemWatcher watcher;

        public static bool distress { get; set; }
        public static bool serverReset { get; set; }

        /// </summary>
        /// <summary>
        /// Call this to start the Distress Calls Listener
        /// </summary>
        /// <returns></returns>
        public async Task ChatLogListener(uint id, ulong channelID)
        {
            System.Console.WriteLine("ChatLog Listener Executed!");

            watcher = new FileSystemWatcher();
            watcher.Path = "C:/Users/ALEX/StarportGE/ChatLogs";
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.*";
            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.EnableRaisingEvents = true;

            IMessageChannel channel = Program.client.GetChannel(channelID) as IMessageChannel;
            await channel.SendMessageAsync("Sucessfully ran ChatLog Listener!");

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
            watcher = null;
            await channel.SendMessageAsync("No longer listening to chat logs!");
            runningTasks.RemoveAt(taskNum);
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
            await Outprint("Sucessfully Initiated Message Listener!", channelID);

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
            runningTasks.RemoveAt(taskNum);
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
            runningTasks.RemoveAt(taskNum);
            dbuTaskNum--;
        }

        public async Task SetDistress(bool v)
        {
            distress = v;
            await Task.Delay(0);
        }

        public async Task SetServerReset(bool v)
        {
            serverReset = v;
            await Task.Delay(0);
        }

        private async void OnChanged(object sender, FileSystemEventArgs e)
        {
            string filePath = e.FullPath;
            string[] fileStrArr = File.ReadAllLines(filePath);
            string lastLine = fileStrArr[fileStrArr.Length - 1];

            if (distress)
            {
                ///Console.WriteLine("Distress");
                if (lastLine.Contains("***") && lastLine.Contains("landed"))
                {
                    await Outprint(lastLine, ChannelID.distressCallsID);
                }
            }
            if (serverReset)
            {
                if (lastLine.Contains("Server Alert!"))
                {
                    await Outprint(lastLine, ChannelID.slaversID);
                }
            }

            if (enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("warped into"))
            {
                await Outprint(lastLine, ChannelID.distressCallsID);
                await Say(lastLine, ChannelID.slaversOnlyVoiceID);
            }
            else if (enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("warped out"))
            {
                await Outprint(lastLine, ChannelID.distressCallsID);
                await Say(lastLine + " because he's a bitch nigga", ChannelID.slaversOnlyVoiceID);
            }

            if (lastLine.Contains("tons of unidentified compounds"))
            {
                if (lastLine.Contains("contains 0 tons of unidentified compounds") && !lastLine.Contains("System"))
                {
                    await Outprint("Undomed: " + lastLine, ChannelID.buildingID);
                }
                else if (lastLine.Contains("contains") && !lastLine.Contains("System"))
                {
                    await Outprint(lastLine, ChannelID.nuetrinoID);
                }
            }

            if (enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("shot down"))
            {
                List<string> alliesList = allies.ToList<string>();
                List<string> enemiesList = enemies.ToList<string>();

                string ally = alliesList.Find(s => lastLine.Contains(s));
                string enemy = enemiesList.Find(s => lastLine.Contains(s));

                if (lastLine.Contains("shot down " + enemy))
                {
                    if (lastLine.Contains("Defenses"))
                    {
                        await Outprint("Nice Job! " + ally + "'s defenses clapped" + enemy, ChannelID.slaversID);
                    }
                    else
                    {
                        await Outprint("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass", ChannelID.slaversID);
                    }

                    await Outprint(lastLine, ChannelID.slaversID);
                    await Say(enemy + " Has Been Slain!", ChannelID.slaversOnlyVoiceID);
                }
                else if (lastLine.Contains("shot down " + ally))
                {
                    await Outprint(lastLine + " Help " + ally + " Nigga. Damn!", ChannelID.slaversID);
                    await Say(ally + " Has Been Slain!", ChannelID.slaversOnlyVoiceID);
                }
            }
        }
    }
}