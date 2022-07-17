//Created by Alexander Fields https://github.com/roku674

using StarportObjects;
using DiscordBotUpdates.Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotUpdates.Modules
{
    internal class TaskInitator : DBUTask
    {
        private Discord.IMessageChannel pictureChannel;

        public static string lastLand { get; set; }
        public static bool building { get; set; }
        public static bool distress { get; set; }
        public static bool kombat { get; set; }
        public static bool alerts { get; set; }
        public static uint planetsLost { get; set; }
        public static uint planetsKaptured { get; set; }
        public static uint alliesSlain { get; set; }
        public static uint enemiesSlain { get; set; }
        public static uint colsAbandoned { get; set; }
        public static uint colsBuilt { get; set; }
        public static uint landings { get; set; }

        /// </summary>
        /// <summary>
        /// Call this to start the Distress Calls Listener
        /// </summary>
        /// <returns></returns>
        public async Task ChatLogListenerAsync(uint id, ulong channelID, string owner)
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
                if (Directory.Exists("G:/My Drive/" + owner + "/StarportGE/ChatLogs"))
                {
                    watcher.Path = "G:/My Drive/Probation/StarportGE/ChatLogs";
                }
            }

            if (!watcher.Path.Equals(" ") && !string.IsNullOrEmpty(watcher.Path))
            {
                System.Console.WriteLine("Found Chatlogs for " + owner);
                watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
                                     | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                watcher.Filter = "*.*";

                watcher.Changed += new FileSystemEventHandler(OnChatChangedAsync);

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
                        System.Console.WriteLine("ChatLogListener Updater: First pass completed For " + owner + "!");
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
        public async Task PictureUpdaterAsync(uint id, ulong channelID)
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
            pictureWatcher.Changed += new FileSystemEventHandler(OnPictureChangedAsync);
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
        public async Task TextUpdaterAsync(uint id, ulong channelID, string type)
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

                string filePath = "";
                if (Directory.Exists("H:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Channel/" + type + ".txt"))
                {
                    filePath = "H:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Channel/" + type + ".txt";
                }
                else if (Directory.Exists("G:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Channel/" + type + ".txt"))
                {
                    filePath = "G:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Channel/" + type + ".txt";
                }
                if (!string.IsNullOrEmpty(filePath) && !filePath.Equals(""))
                {
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

                        await OutprintAsync("Created " + type + ".txt ! Recommend Rerunning!", ChannelID.botCommandsID);
                    }
                }

                if (i == 1)
                {
                    System.Console.WriteLine(type + " Updater: First pass completed!");
                }

                System.TimeSpan timeSpan = System.DateTime.Now - System.DateTime.Today.AddDays(1).AddSeconds(-1);

                //System.Console.WriteLine(timeSpan.Hours + ":" + timeSpan.Minutes + ":" + timeSpan.Seconds);

                if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 0)
                {
                    if (((int)planetsKaptured - (int)planetsLost) >= 20)
                    {
                        await OutprintAsync("https://tenor.com/view/cat-shooting-mouth-open-gif-15017033", ChannelID.slaversID);
                    }
                    if (colsBuilt > 10)
                    {
                        await OutprintAsync("https://tenor.com/view/construct-construction-nail-and-hammer-build-worker-gif-13899535", ChannelID.slaversID);
                    }
                    if (enemiesSlain > 10)
                    {
                        await OutprintAsync("https://tenor.com/view/starwars-the-force-awakens-xwing-air-combat-gif-4813295", ChannelID.slaversID);
                    }

                    await OutprintAsync(
                        "@everyone Daily Report: " + '\n'
                        + "We Lawst: " + planetsLost + '\n'
                        + "We Kaptured: " + planetsKaptured + '\n'
                        + "Allies Slain: " + alliesSlain + '\n'
                        + "Enemies Slain: " + enemiesSlain + '\n'
                        + "Landings: " + landings + '\n'
                        + "Colonies Abanonded: " + colsAbandoned + " (They just went out for milk and cigarettes)" + '\n'
                        + "Colonies Built: " + colsBuilt, ChannelID.slaversID);

                    planetsKaptured = 0;
                    planetsLost = 0;
                    alliesSlain = 0;
                    enemiesSlain = 0;
                    landings = 0;
                    colsAbandoned = 0;
                    colsBuilt = 0;
                }

                task.ticker++;
            }
            await OutprintAsync("No Longer Listening for " + type + "!", ChannelID.botCommandsID);
            runningTasks.RemoveAt(taskNum);
            dbuTaskNum--;
        }

        internal async Task SetAllAsync(bool v)
        {
            building = v;
            distress = v;
            kombat = v;
            alerts = v;
            await Task.Delay(0);
        }

        internal async Task SetBuildingAsync(bool v)
        {
            building = v;
            await Task.Delay(0);
        }

        internal async Task SetDistressAsync(bool v)
        {
            distress = v;
            await Task.Delay(0);
        }

        internal async Task SetKombatAsync(bool v)
        {
            kombat = v;
            await Task.Delay(0);
        }

        internal async Task SetAlertsAsync(bool v)
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

        private async void OnChatChangedAsync(object sender, FileSystemEventArgs fileSysEvent)
        {
            string filePath = fileSysEvent.FullPath;
            string[] fileStrArr = new string[0];

            string[] split = Path.GetFileName(filePath).Split(" ");
            string chatLogOwner = split[0];

            bool bot = true;
            if (filePath.Contains("Users"))
            {
                bot = false;
                //System.Console.WriteLine("On Changed Client Not Bot");
            }
            try
            {
                fileStrArr = await File.ReadAllLinesAsync(filePath);
            }
            catch (System.Exception ex)
            {
                await OutprintAsync(ex.ToString(), ChannelID.botUpdatesID);
            }

            if (fileStrArr.Length > 0)
            {
                string lastLine = fileStrArr[fileStrArr.Length - 1];
                string secondToLastLine = fileStrArr[fileStrArr.Length - 2];

                if (lastLine.Contains("Landed on ") && lastLine.Contains("world"))
                {
                    lastLand = Algorithms.StringManipulation.GetBetween(lastLine, "on", ",");
                    lastLand = lastLand.Replace("world ", "");
                }
                else if (lastLine.Contains("Taking you directly"))
                {
                    lastLand = Algorithms.StringManipulation.GetBetween(secondToLastLine, "on", ",");
                    lastLand = lastLand.Replace("world ", "");
                }

                if (kombat)
                {
                    //warped in
                    if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && (lastLine.Contains("warped into") || lastLine.Contains("entered the system")))
                    {
                        List<string> enemiesList = Diplomacy.enemies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync("@everyone " + chatLogOwner + ": " + lastLine, ChannelID.distressCallsID);
                        await SayAsync(enemy + " spotted! By " + chatLogOwner, ChannelID.voiceSlaversOnlyID);
                    }
                    else if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("warped out"))
                    {
                        List<string> enemiesList = Diplomacy.allies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync("@everyone " + lastLine, ChannelID.distressCallsID);
                        await SayAsync(enemy + " ran away cause he's a bitch nigga", ChannelID.voiceSlaversOnlyID);
                    }
                    else if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("landed on a planet"))
                    {
                        List<string> enemiesList = Diplomacy.allies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync(lastLine, ChannelID.distressCallsID);
                        await SayAsync(enemy + " landed!", ChannelID.voiceSlaversOnlyID);
                    }
                    else if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("docked"))
                    {
                        List<string> enemiesList = Diplomacy.allies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync(lastLine, ChannelID.slaversID);
                        await SayAsync(enemy + " Re-Shielded!", ChannelID.voiceSlaversOnlyID);
                    }

                    //shot downs
                    if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("shot down") && !bot && !lastLine.Contains("shouts"))
                    {
                        List<string> alliesList = Diplomacy.allies.ToList<string>();
                        List<string> enemiesList = Diplomacy.enemies.ToList<string>();

                        string ally = alliesList.Find(s => lastLine.Contains(s));
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        if (ally == null)
                        {
                            ally = " ";
                        }
                        if (enemy == null)
                        {
                            enemy = " ";
                        }

                        if (lastLine.Contains("shot down " + enemy))
                        {
                            enemiesSlain++;
                            if (lastLine.Contains("Defenses") && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + lastLine, ChannelID.slaversID);
                                //await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + lastLine, ChannelID.alliedChatID);
                                enemiesSlain++;
                            }
                            else if (!string.IsNullOrEmpty(ally) && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + lastLine, ChannelID.slaversID);
                                //await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + lastLine, ChannelID.alliedChatID);
                            }
                            else
                            {
                                await OutprintAsync(lastLine, ChannelID.slaversID);
                                //await OutprintAsync(lastLine, ChannelID.alliedChatID);
                            }

                            await SayAsync(enemy + " Has Been Slain!", ChannelID.voiceSlaversOnlyID);
                        }
                        else if (lastLine.Contains("For blowing up"))
                        {
                            enemiesSlain++;
                            if (secondToLastLine.Contains("Defenses") && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + secondToLastLine, ChannelID.slaversID);
                                //await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + secondToLastLine, ChannelID.alliedChatID);
                                enemiesSlain++;
                            }
                            else if (!string.IsNullOrEmpty(ally) && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + secondToLastLine, ChannelID.slaversID);
                                //await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + secondToLastLine, ChannelID.alliedChatID);
                            }
                            else
                            {
                                await OutprintAsync(secondToLastLine, ChannelID.slaversID);
                                //await OutprintAsync(secondToLastLine, ChannelID.alliedChatID);
                            }

                            await SayAsync(enemy + " Has Been Slain!", ChannelID.voiceSlaversOnlyID);
                        }
                        else if (lastLine.Contains("shot down " + ally) && !ally.Equals(" "))
                        {
                            alliesSlain++;
                            await OutprintAsync(lastLine + " Help " + ally + " Nigga. Damn!", ChannelID.slaversID);
                            //await OutprintAsync(lastLine + " Help " + ally + " Nigga. Damn!", ChannelID.alliedChatID);
                            await SayAsync(ally + " Has Been Slain!", ChannelID.voiceSlaversOnlyID);
                        }
                    }

                    //invasions
                    if (lastLine.Contains("was invaded and taken"))
                    {
                        await OutprintAsync(AtUser(lastLine) + lastLine, ChannelID.recapListID);
                        await SayAsync("We've Lost a Command Post!", ChannelID.voiceSlaversOnlyID);

                        if (lastLine.Contains("experience"))
                        {
                            string experience = Algorithms.StringManipulation.GetBetween(lastLine, "lost", "experience");
                            experience = experience.Replace(",", "");
                            experience = experience.Trim();
                            uint exp = 50001;

                            try
                            {
                                exp = uint.Parse(experience);
                            }
                            catch (System.Exception e)
                            {
                                await OutprintAsync(experience + '\n' + e.ToString(), ChannelID.botUpdatesID);
                            }

                            if (exp > 50000)
                            {
                                planetsLost++;
                            }
                        }
                    }
                    else if (lastLine.Contains("captured the colony"))
                    {
                        await OutprintAsync(lastLine, ChannelID.slaversID);
                        //await OutprintAsync(lastLine, ChannelID.alliedChatID);
                        await SayAsync("We've Captured a Command Post!", ChannelID.voiceSlaversOnlyID);
                        planetsKaptured++;
                    }
                    else if (lastLine.Contains("You claim ownership of the colony owned by"))
                    {
                        await OutprintAsync(secondToLastLine, ChannelID.slaversID);
                        //await OutprintAsync(secondToLastLine, ChannelID.alliedChatID);
                        await SayAsync("We've Captured a Command Post!", ChannelID.voiceSlaversOnlyID);
                        planetsKaptured++;
                    }
                    else if (lastLine.Contains("It now belongs to"))
                    {
                        await OutprintAsync(fileStrArr[fileStrArr.Length - 3], ChannelID.slaversID);
                        //await OutprintAsync(fileStrArr[fileStrArr.Length - 3], ChannelID.alliedChatID);
                        await SayAsync("We've Captured a Command Post!", ChannelID.voiceSlaversOnlyID);
                        planetsKaptured++;
                    }
                    else if (lastLine.Contains("For successful invasion"))
                    {
                        await OutprintAsync(fileStrArr[fileStrArr.Length - 4], ChannelID.slaversID);
                        //await OutprintAsync(fileStrArr[fileStrArr.Length - 4], ChannelID.alliedChatID);
                        await SayAsync("We've Captured a Command Post!", ChannelID.voiceSlaversOnlyID);
                        planetsKaptured++;
                    }
                }

                if (distress && !bot)
                {
                    if (lastLine.Contains("*** Distress"))
                    {
                        landings++;
                        string colonyName = Algorithms.StringManipulation.GetBetween(lastLine, "from", "on");
                        if (colonyName.Contains("(") && colonyName.Contains("."))
                        {
                            colonyName = Algorithms.StringManipulation.GetBetween(colonyName, ")", ".");
                        }

                        System.Console.WriteLine("colonyName: " + colonyName);
                        string path = "G:/My Drive/Personal Stuff/Starport/PlanetPictures/" + colonyName + ".png";
                        if (File.Exists(path))
                        {
                            await OutprintAsync("@everyone " + lastLine, ChannelID.distressCallsID);
                            await OutprintFileAsync(path, ChannelID.distressCallsID);
                        }
                        else
                        {
                            await OutprintAsync("@everyone " + lastLine, ChannelID.distressCallsID);
                        }
                    }
                    else if (lastLine.Contains("***") && lastLine.Contains("landed"))
                    {
                        landings++;
                        await OutprintAsync(lastLine, ChannelID.distressCallsID);
                    }
                }
                if (alerts && !bot)
                {
                    if (lastLine.Contains("Server Alert!"))
                    {
                        await OutprintAsync("@everyone " + lastLine, ChannelID.slaversID);
                        await OutprintAsync("@everyone " + lastLine, ChannelID.alliedChatID);
                    }
                    else if (lastLine.Contains("U.N. Hotline"))
                    {
                        List<string> alliesList = Diplomacy.allies.ToList<string>();
                        string ally = alliesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync(lastLine, ChannelID.slaversID);
                        await SayAsync(ally + " I see you!", ChannelID.voiceSlaversOnlyID);
                    }
                }

                if (lastLine.Contains("tons of unidentified compounds"))
                {
                    if (lastLine.Contains("contains 0 tons of unidentified compounds") && !lastLine.Contains("System"))
                    {
                        await OutprintAsync("Undomed: " + lastLine, ChannelID.buildingID);
                    }
                    else if (lastLine.Contains("contains") && !lastLine.Contains("System"))
                    {
                        await OutprintAsync(lastLine, ChannelID.nuetrinoID);
                    }
                }

                if (building)
                {
                    //col died

                    if (!bot)
                    {
                        if (lastLine.Contains("was finally abandoned"))
                        {
                            System.TimeSpan days3TimeSpan = new System.TimeSpan(0, 72, 0, 0);
                            System.DateTime days3 = System.DateTime.Now + days3TimeSpan;

                            string title = Algorithms.StringManipulation.GetBetween(lastLine, "colony", "was");

                            await CreateCalendarEventAsync(days3, title, lastLine, ChannelID.buildingID);
                            colsAbandoned++;
                            /*
                            await Outprint(lastLine +
                                '\n' + "Add redome time to Discord Calendar Unimplemented!" +
                                '\n' + end.ToString(), ChannelID.redomeID);*/
                        }
                        //aa
                        else if (
                            (lastLine.Contains("Advanced Architecture lvl 4") && (!lastLine.Contains("Des") || !lastLine.Contains("Mou"))) ||
                            (lastLine.Contains("Advanced Architecture lvl 5") && !lastLine.Contains(".Arc"))
                            )
                        {
                            await OutprintAsync(AtUser(lastLine) + lastLine + " Zounds dat hoe now!", ChannelID.buildingID);
                            //await Say(Algorithms.StringManipulation.GetBetween(lastLine, "on", "discovered") + " is ready to zounds!", ChannelID.voiceBuildingID);
                        }
                        else if (lastLine.Contains("Advanced Architecture lvl 2") && (lastLine.Contains(".Arc") || lastLine.Contains(".arc")))
                        {
                            await OutprintAsync(AtUser(lastLine) + lastLine + " Zounds dat hoe now!", ChannelID.buildingID);
                            //await Say(Algorithms.StringManipulation.GetBetween(lastLine, "on", "discovered") + " is ready to zounds!", ChannelID.voiceBuildingID);
                        }
                        else if (lastLine.Contains("Military Tradition lvl 3") || lastLine.Contains("Military Tradition lvl 4") || lastLine.Contains("Military Tradition lvl 5"))
                        {
                            await OutprintAsync(AtUser(lastLine) + lastLine + " Decent Huge Metro col", ChannelID.buildingID);
                            //await Say(Algorithms.StringManipulation.GetBetween(lastLine, "on", "discovered") + " is ready to build!", ChannelID.voiceBuildingID);
                        }
                        //if bio3
                        else if (lastLine.Contains("completed work on the Biodome Level 3."))
                        {
                            await OutprintAsync(AtUser(lastLine) + lastLine, ChannelID.buildingID);
                            colsBuilt++;
                        }
                    }

                    //Domed new colony && dd
                    if (lastLine.Contains("founding"))
                    {
                        await OutprintAsync(chatLogOwner + " colonized " + lastLand + "!", ChannelID.buildingID);
                        await SayAsync(chatLogOwner + " colonized a new world!", ChannelID.voiceBuildingID);
                    }
                    else if (lastLine.Contains("adding another dome"))
                    {
                        await OutprintAsync(chatLogOwner + " Created a New Double Dome on " + lastLand + "!", ChannelID.buildingID);
                        await OutprintAsync("https://tenor.com/view/cat-shooting-mouth-open-gif-15017033", ChannelID.buildingID);
                        await SayAsync(chatLogOwner + " Created a New Double Dome on " + lastLand + "!", ChannelID.voiceSlaversOnlyID);
                        //await CelebrateUser("Slavers", "I'd like to see them try and take this!", ChannelID.slaversID);
                    }
                }
            }
        }

        private async void OnPictureChangedAsync(object sender, FileSystemEventArgs fileSysEvent)
        {
            string path = fileSysEvent.FullPath;

            System.Console.WriteLine(Path.GetFileName(path));
            await pictureChannel.SendFileAsync(path, Path.GetFileName(path));
            File.Delete(path);
        }

        private string AtUser(string line)
        {
            if (line.Contains("Autism") || line.Contains("Anxiety") || line.Contains("Freeman"))
            {
                return "<@139536795858632705> ";
            }
            else if (line.Contains("Avacado") || line.Contains("Archer") || line.Contains("Archie"))
            {
                return "<@530669734413205505> ";
            }
            else if (line.Contains("Dev"))
            {
                return "<@276593195767431168> ";
            }
            else if (line.Contains("lk"))
            {
                return "<@429101973145387019> ";
            }
            else if (line.Contains("Banana"))
            {
                return "<@535618193251762176> ";
            }
            else if (line.Contains("Jum"))
            {
                return "<@941167776163323944> ";
            }
            else if (line.Contains("tater"))
            {
                return "<@969258165831106581> ";
            }
            else
            {
                return "";
            }
        }
    }
}