﻿//Created by Alexander Fields https://github.com/roku674

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace DiscordBotUpdates.Modules
{
    internal class TaskInitator : DBUTask
    {
        private string[] allies =
        {
            "Allie", "Anxiety.jar", "Bodhi", "CaptArcher",
            "Depression.Wav",  "Devila", "Hokujinn",
            "Leaderkiller","Probation", "Taterchip", "WW3"
        };

        private string[] enemies =
                {
            "Altair","Awmalzo","B-radk.","Dad","Darkside", "Demon", "Deegs",
            "DOG-WHISPERER", "Flint", "Meshuggah","McGee","Pluto","Presto",
            "Revelation", "RepealThe2ndA","Scar-Face"
        };

        private Discord.IMessageChannel pictureChannel;

        public static bool building { get; set; }
        public static bool distress { get; set; }
        public static bool kombat { get; set; }
        public static bool alerts { get; set; }

        /// </summary>
        /// <summary>
        /// Call this to start the Distress Calls Listener
        /// </summary>
        /// <returns></returns>
        public async Task ChatLogListener(uint id, ulong channelID, string owner)
        {
            System.Console.WriteLine("ChatLog Listener Executed!");

            FileSystemWatcher watcher = new FileSystemWatcher();

            if (owner.Equals("Client"))
            {
                if (Directory.Exists("C:/Users/ZANDER/StarportGE/ChatLogs"))
                {
                    watcher.Path = "C:/Users/ZANDER/StarportGE/ChatLogs";
                }
                else if (Directory.Exists("C:/Users/ALEX/StarportGE/ChatLogs"))
                {
                    watcher.Path = "C:/Users/ALEX/StarportGE/ChatLogs";
                }
            }
            else
            {
                if (Directory.Exists("H:/My Drive/" + owner + "/StarportGE/ChatLogs"))
                {
                    watcher.Path = "H:/My Drive/Probation/StarportGE/ChatLogs";
                }
            }

            if (!watcher.Path.Equals(" ") && !string.IsNullOrEmpty(watcher.Path))
            {
                System.Console.WriteLine("Found Chatlogs for " + owner);
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Filter = "*.*";

                watcher.Changed += new FileSystemEventHandler(OnChatChanged);

                watcher.EnableRaisingEvents = true;

                Discord.IMessageChannel channel = Program.client.GetChannel(channelID) as Discord.IMessageChannel;
                //await channel.SendMessageAsync("Sucessfully ran ChatLog Listener!");
                System.Console.WriteLine("Sucessfully ran ChatLog Listener!");

                await Task.Delay(2000);

                int taskNum = runningTasks.FindIndex(task => task.id == id);
                DBUTaskObj task = runningTasks.ElementAt(taskNum);

                for (int i = 0; i < duration; i++)
                {
                    if (runningTasks[taskNum].isCancelled)
                    {
                        i = duration;
                        break;
                    }
                    await Task.Delay(1000);

                    if (i == 1)
                    {
                        System.Console.WriteLine("ChatLogListener Updater: First pass completed!For " + owner);
                    }

                    task.ticker++;
                }
                watcher = null;
                await channel.SendMessageAsync("No longer listening to chat logs!");
                runningTasks.RemoveAt(taskNum);
                dbuTaskNum--;
            }
            else
            {
                int taskNum = runningTasks.FindIndex(task => task.id == id);
                DBUTaskObj task = runningTasks.ElementAt(taskNum);
                System.Console.WriteLine("Watcher Path Not Found for " + owner + "! Thread was not started!");
                runningTasks.RemoveAt(taskNum);
                dbuTaskNum--;
            }
        }

        /// <summary>
        /// Call this to start the picture updater
        /// </summary>
        /// <returns></returns>
        public async Task PictureUpdater(uint id, ulong channelID)
        {
            pictureChannel = Program.client.GetChannel(channelID) as Discord.IMessageChannel;
            //await channel.SendMessageAsync("Sucessfully Initiated Picture Listener!");
            System.Console.WriteLine("Sucessfully Initiated Picture Listener!");
            await Task.Delay(2000);

            FileSystemWatcher pictureWatcher = new FileSystemWatcher();
            pictureWatcher.Path = "H:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Pictures";
            pictureWatcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                   | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            pictureWatcher.Filter = "*.*";
            pictureWatcher.Changed += new FileSystemEventHandler(OnPictureChanged);
            pictureWatcher.EnableRaisingEvents = true;

            int taskNum = runningTasks.FindIndex(task => task.id == id);

            while (taskNum == -1)
            {
                taskNum = runningTasks.FindIndex(task => task.id == id);
            }

            DBUTaskObj task = runningTasks.ElementAt(taskNum);

            for (int i = 0; i < duration; i++)
            {
                if (runningTasks[taskNum].isCancelled)
                {
                    i = duration;
                    break;
                }
                await Task.Delay(1000);

                if (i == 1)
                {
                    System.Console.WriteLine("Picture Updater: First pass completed!");
                }

                task.ticker++;
            }
            await pictureChannel.SendMessageAsync("No Longer Listening for Pictures Updates!");
            runningTasks.RemoveAt(taskNum);
            dbuTaskNum--;
        }

        /// <summary>
        /// Call this to start the Message Updater
        /// </summary>
        /// <returns></returns>
        public async Task TextUpdater(uint id, ulong channelID, string type)
        {
            //await Outprint("Sucessfully Initiated " + type + " Listener!", ChannelID.botCommandsID);
            await Task.Delay(500);
            int taskNum = runningTasks.FindIndex(task => task.id == id);

            while (taskNum == -1)
            {
                taskNum = runningTasks.FindIndex(task => task.id == id);
            }

            DBUTaskObj task = runningTasks.ElementAt(taskNum);
            System.Console.WriteLine("Sucessfully Initiated " + type + " Listener!", ChannelID.botCommandsID);

            for (int i = 0; i < duration; i++)
            {
                if (runningTasks[taskNum].isCancelled)
                {
                    i = duration;
                    break;
                }

                await Task.Delay(1000);

                string filePath = "H:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Channel/" + type + ".txt";
                string fileAsText = "";

                if (File.Exists(filePath))
                {
                    fileAsText = await File.ReadAllTextAsync(filePath, default);

                    if (!string.IsNullOrEmpty(fileAsText) && fileAsText != " ")
                    {
                        Discord.IMessageChannel channel = Program.client.GetChannel(channelID) as Discord.IMessageChannel;
                        await channel.SendMessageAsync(fileAsText); //let it rain

                        await File.WriteAllTextAsync(filePath, " "); //now clear it out
                    }
                }
                else
                {
                    File.Create(filePath).Close();

                    await Outprint("Created " + type + ".txt ! Recommend Rerunning!", ChannelID.botCommandsID);
                }

                if (i == 1)
                {
                    System.Console.WriteLine(type + " Updater: First pass completed!");
                }

                task.ticker++;
            }
            await Outprint("No Longer Listening for " + type + "!", ChannelID.botCommandsID);
            runningTasks.RemoveAt(taskNum);
            dbuTaskNum--;
        }

        internal async Task SetAll(bool v)
        {
            building = v;
            distress = v;
            kombat = v;
            alerts = v;
            await Task.Delay(0);
        }

        internal async Task SetBuilding(bool v)
        {
            building = v;
            await Task.Delay(0);
        }

        internal async Task SetDistress(bool v)
        {
            distress = v;
            await Task.Delay(0);
        }

        internal async Task SetKombat(bool v)
        {
            kombat = v;
            await Task.Delay(0);
        }

        internal async Task SetAlerts(bool v)
        {
            alerts = v;
            await Task.Delay(0);
        }

        /// <summary>
        /// If the file can be opened for exclusive access it means that the file
        /// is no longer locked by another process.
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private static bool IsFileReady(string filename)
        {
            try
            {
                using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                    return inputStream.Length > 0;
            }
            catch (System.Exception)
            {
                return false;
            }
        }

        private async void OnChatChanged(object sender, FileSystemEventArgs fileSysEvent)
        {
            string filePath = fileSysEvent.FullPath;
            string[] fileStrArr = new string[0];

            bool bot = true;
            if (filePath.Contains("Users"))
            {
                bot = false;
                //System.Console.WriteLine("On Changed Client Not Bot");
            }

            fileStrArr = await File.ReadAllLinesAsync(filePath);

            if (fileStrArr.Length > 0)
            {
                string lastLine = fileStrArr[fileStrArr.Length - 1];

                if (kombat)
                {
                    //warped in
                    if (enemies.Any(s => lastLine.Contains(s)) && (lastLine.Contains("warped into") || lastLine.Contains("entered the system")))
                    {
                        List<string> enemiesList = enemies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await Outprint(Path.GetFileName(filePath) + ": " + lastLine, ChannelID.distressCallsID);
                        await Say(enemy + " spotted! By " + Path.GetFileName(filePath), ChannelID.slaversOnlyVoiceID);
                    }
                    else if (enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("warped out"))
                    {
                        List<string> enemiesList = allies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await Outprint(lastLine, ChannelID.distressCallsID);
                        await Say(enemy + " ran away cause he's a bitch nigga", ChannelID.slaversOnlyVoiceID);
                    }
                    else if (enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("landed on a planet"))
                    {
                        List<string> enemiesList = allies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await Outprint(lastLine, ChannelID.slaversID);
                        await Say(enemy + " landed!", ChannelID.slaversOnlyVoiceID);
                    }
                    else if (enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("docked"))
                    {
                        List<string> enemiesList = allies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await Outprint(lastLine, ChannelID.slaversID);
                        await Say(enemy + " Re-Shielded!", ChannelID.slaversOnlyVoiceID);
                    }

                    //shot downs
                    if (enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("shot down") && !bot)
                    {
                        List<string> alliesList = allies.ToList<string>();
                        List<string> enemiesList = enemies.ToList<string>();

                        string ally = alliesList.Find(s => lastLine.Contains(s));
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        if (lastLine.Contains("shot down " + enemy))
                        {
                            if (lastLine.Contains("Defenses") && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await Outprint("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + lastLine, ChannelID.slaversID);
                            }
                            else if (!string.IsNullOrEmpty(ally) && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await Outprint("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + lastLine, ChannelID.slaversID);
                            }
                            else
                            {
                                await Outprint(lastLine, ChannelID.slaversID);
                            }

                            await Say(enemy + " Has Been Slain!", ChannelID.slaversOnlyVoiceID);
                        }
                        else if (lastLine.Contains("shot down " + ally) && !ally.Equals(" "))
                        {
                            await Outprint(lastLine + " Help " + ally + " Nigga. Damn!", ChannelID.slaversID);
                            await Say(ally + " Has Been Slain!", ChannelID.slaversOnlyVoiceID);
                        }
                    }

                    //invasions
                    if (lastLine.Contains("was invaded"))
                    {
                        await Outprint(lastLine, ChannelID.recapListID);
                        await Say("We've Lost a Command Post!", ChannelID.slaversOnlyVoiceID);
                    }
                    else if (lastLine.Contains("captured"))
                    {
                        List<string> alliesList = allies.ToList<string>();
                        string ally = alliesList.Find(s => lastLine.Contains(s));

                        await Outprint(ally + " captured a command post!" + '\n' + lastLine, ChannelID.slaversID);
                        await Say("We've Captured a Command Post!", ChannelID.slaversOnlyVoiceID);
                    }
                }

                if (distress)
                {
                    ///Console.WriteLine("Distress");
                    if (lastLine.Contains("***") && lastLine.Contains("landed"))
                    {
                        await Outprint(lastLine, ChannelID.distressCallsID);
                    }
                }
                if (alerts && !bot)
                {
                    if (lastLine.Contains("Server Alert!"))
                    {
                        await Outprint("@everyone " + lastLine, ChannelID.slaversID);
                    }
                    else if (lastLine.Contains("U.N. Hotline"))
                    {
                        List<string> alliesList = allies.ToList<string>();
                        string ally = alliesList.Find(s => lastLine.Contains(s));

                        await Outprint(lastLine, ChannelID.slaversID);
                        await Say(ally + " I see you!", ChannelID.slaversOnlyVoiceID);
                    }
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

                if (building)
                {
                    //col died
                    /*
                    if (lastLine.Contains("was finally abandoned"))
                    {
                        System.TimeSpan days3 = new System.TimeSpan(72, 0, 0, 0);
                        System.TimeSpan days3thirtyMin = new System.TimeSpan(72, 0, 30, 0);

                        System.DateTime start = new System.DateTime() + days3;

                        System.DateTime end = System.DateTime.Now + days3thirtyMin;

                        //await CreateCalendarEvent(start, end, lastLine, ChannelID.buildingID);

                        await Outprint(lastLine +
                            '\n' + "Add redome time to Discord Calendar!" +
                            '\n' + end.ToString(), ChannelID.buildingID);
                    }
                    */
                    //aa
                    if (lastLine.Contains("Advanced Architecture lvl 4") || lastLine.Contains("Advanced Architecture lvl 5"))
                    {
                        await Outprint(lastLine, ChannelID.buildingID);
                    }
                    else if (lastLine.Contains("Advanced Architecture lvl 2") && (lastLine.Contains(".Arc") || lastLine.Contains(".arc")))
                    {
                        await Outprint(lastLine, ChannelID.buildingID);
                    }

                    //Domed new colony && dd
                    if (lastLine.Contains("founding"))
                    {
                        await Outprint(Path.GetFileName(filePath) + " Colonized a New World!", ChannelID.buildingID);
                    }
                    else if (lastLine.Contains("adding another dome"))
                    {
                        await Outprint(Path.GetFileName(filePath) + " Created a New Double Dome!", ChannelID.buildingID);
                        await Outprint("https://tenor.com/view/cat-shooting-mouth-open-gif-15017033", ChannelID.buildingID);
                        //await CelebrateUser("Slavers", "I'd like to see them try and take this!", ChannelID.slaversID);
                    }
                }
            }
        }

        private async void OnPictureChanged(object sender, FileSystemEventArgs fileSysEvent)
        {
            string path = fileSysEvent.FullPath;

            System.Console.WriteLine(Path.GetFileName(path));
            await pictureChannel.SendFileAsync(path, Path.GetFileName(path));
            File.Delete(path);
        }
    }
}