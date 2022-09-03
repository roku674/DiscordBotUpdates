﻿//Created by Alexander Fields https://github.com/roku674
using Discord.Commands;
using DiscordBotUpdates.Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotUpdates.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        internal TaskInitator init = new TaskInitator();
        private readonly string[] cylons = { "Allie", "Bitcoin", "Probation" };
        private readonly string[] listenerNames = { "building", "distress", "kombat", "serverResets", "All" };

        [Command("run AllTasks")]
        public async Task AllTasksRun()
        {
            System.Console.WriteLine("All Tasks Running...");
            _ = Task.Run(() => BotUpdaterPost());
            _ = Task.Run(() => ListenerPost("All"));
            _ = Task.Run(() => ListenerChatLogPost("All"));

            await Task.Delay(33);
        }

        [Command("run StopAllTasks")]
        public async Task AllTasksStop()
        {
            if (DBUTask.runningTasks.Count > 0)
            {
                for (int i = DBUTask.runningTasks.Count - 1; i >= 0; i--)
                {
                    DBUTask.DBUTaskObj dbuTask = DBUTask.runningTasks[i];

                    await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted + " | Lifetime: " + SecondsToTime(dbuTask.ticker)
                        + '\n'
                        + "Was ended at " + System.DateTime.Now);

                    dbuTask.Cancel();
                    DBUTask.runningTasks.Remove(dbuTask);
                }
            }
            else
            {
                await ReplyAsync("There are no tasks to stop!");
            }
        }

        [Command("run StopTasks")]
        public async Task BotTasksStop([Remainder] string text)
        {
            if (DBUTask.runningTasks.Count > 0)
            {
                for (int i = DBUTask.runningTasks.Count - 1; i >= 0; i--)
                {
                    DBUTask.DBUTaskObj dbuTask = DBUTask.runningTasks[i];

                    List<string> botNamesList = cylons.ToList<string>();
                    string bot = botNamesList.Find(s => text.Contains(s));

                    if (dbuTask.owner.Equals(bot))
                    {
                        await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted + " | Lifetime: " + SecondsToTime(dbuTask.ticker)
                            + '\n'
                            + "Was ended at " + System.DateTime.Now);

                        dbuTask.Cancel();
                        DBUTask.runningTasks.Remove(dbuTask);
                    }
                    else if (dbuTask.owner.Equals("Client"))
                    {
                        await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted + " | Lifetime: " + SecondsToTime(dbuTask.ticker)
                            + '\n'
                            + "Was ended at " + System.DateTime.Now);

                        dbuTask.Cancel();
                        DBUTask.runningTasks.Remove(dbuTask);
                    }
                }
            }
            else
            {
                await ReplyAsync("There are no tasks to stop!");
            }
        }

        [Command("run BotUpdater")]
        public async Task BotUpdaterPost()
        {
            await ReplyAsync("By Your Command! Listening for messages and pictures for " + DBUTask.duration + " seconds!");

            await MessageUpdater("Message Updater", "Client");
            //await MessageUpdater(ChannelID.distressCallsID, "Distress Signal Updater", "Client", "distress");
            //await MessageUpdater(ChannelID.slaversID, "Warped In & Out Updater", "Client", "warpedInOut");

            await PictureUpdater(ChannelID.botUpdatesID, "Picture Updater", "Client");
        }

        [Command("run Build")]
        public async Task BuildMacroCreator([Remainder] string text)
        {
            await init.UpdateAllieTxt(text);
        }

        [Command("run ClearBotsEcho")]
        public async Task ClearBotsEcho()
        {
            string[] files = Directory.GetFiles(Directory.GetCurrentDirectory() + "/Echo", "*.txt");

            for (int i = 0; i < files.Length; i++)
            {
                await File.WriteAllTextAsync(files[i], "");

                await ReplyAsync(Path.GetFileName(files[i]) + "Text Files Cleared!");
            }
        }

        [Command("run Deactivate")]
        public async Task DeactivateProgramPost()
        {
            await ReplyAsync("Client will be stopped now...");
            await Program.client.StopAsync();
        }

        [Command("request diplomacy")]
        public async Task DiplomacyGet()
        {
            string allies = string.Join('\n', StarportObjects.Diplomacy.allies);
            string enemies = string.Join('\n', StarportObjects.Diplomacy.enemies);
            string nap = string.Join('\n', StarportObjects.Diplomacy.nap);
            await ReplyAsync("Allies: " + '\n' + allies + '\n'
                + '\n' + "Enemies: " + '\n' + enemies + '\n'
                + '\n' + "NAP: " + '\n' + nap
                );
        }

        [Command("run Echo")]
        public async Task EchoPost([Remainder] string text)
        {
            if (cylons.Any(s => text.Contains(s)))
            {
                List<string> botNamesList = cylons.ToList<string>();

                string bot = botNamesList.Find(s => text.Contains(s));

                /*
                if (string.IsNullOrEmpty(bot) || bot.Equals(" "))
                {
                    botNamesList = builderBots.ToList<string>();
                    bot = botNamesList.Find(s => text.Contains(s));
                }*/

                string botEcho = text.Replace(bot + " ", "");
                botEcho.TrimStart();

                if (File.Exists(Directory.GetCurrentDirectory() + "/Echo/" + bot + ".txt"))
                {
                    await File.AppendAllTextAsync(Directory.GetCurrentDirectory() + "/Echo/" + bot + ".txt", botEcho);
                }
                else
                {
                    File.Create(Directory.GetCurrentDirectory() + "/Echo/" + bot + ".txt").Close();
                    await ReplyAsync("Created" + bot + ".txt ! Recommend ReRunning!");
                }

                await ReplyAsync("Sending: " + botEcho + " to " + bot);
            }
        }

        [Command("request enemyPlanets")]
        public async Task EnemyPlanetsGet([Remainder] string text)
        {
            await Task.Run(() => init.FindEnemyColoniesAsync(text, "G:/My Drive/Personal Stuff/Starport/PlanetPictures/Enemy Planets"));
        }

        [Command("request List")]
        public async Task RevoltGet([Remainder] string text)
        {
            await Task.Run(() => init.FindListAsync(text));
        }

        [Command("request WF")]
        public async Task WeaponsFactoryGet([Remainder] string text)
        {
            await Task.Run(() => init.FindWeaponsNearMeAsync(text));
        }

        [Command("Help")]
        public async Task Help()
        {
            await ReplyAsync("Bot Commands:" +
                '\n' +
                "  Ping" +
                '\n' +
                "  Help" +
                '\n' + '\n' +
                "  Gets: " +
                '\n' +
                "    request enemyPlanets (ownerName/corporation)" +
                '\n' +
                "    request List (Type)" +
                '\n' +
                "    request Lifetime" +
                '\n' +
                "    request Listeners" +
                '\n' +
                "    request planet (planetName)" +
                '\n' +
                "    request planetQuote" +
                '\n' +
                "    request planetTallies" +
                '\n' +
                "    request RunningTasks" +
                '\n' +
                "    request WF (defenses/nukes/shields)" +
                '\n' + '\n' +
                "  Posts: " +
                '\n' +
                "    run AllTasks" +
                '\n' +
                "    run BotUpdater" +
                '\n' +
                "    run ClearBotsEcho" +
                '\n' +
                "    run Deactivate" +
                '\n' +
                "    run Echo (BotName) (Command/Chat) (line)" +
                '\n' +
                "    run/request Info" +
                '\n' +
                "    run ListenerChatLog (Client/Bot Name)" +
                '\n' +
                "    run Listener (Listener type)" +
                 '\n' +
                "    run planetsCaptured #" +
                 '\n' +
                "    run planetsLost #" +
                '\n' +
                "    run StopAllTasks" +
                '\n' +
                "    run readExcel" +
                '\n' +
                "    run readFolders" +
                '\n' +
                "    run StopClientTasks" +
                '\n' +
                "    run StopListener (Listener type)" +
                '\n' +
                "    run StopTasks (BotName or client)"
                );
        }

        [Command("request Info")]
        public async Task InfoGet()
        {
            string owner = "Client";
            if (Directory.Exists("C:/Users/ZANDER/StarportGE/ChatLogs"))
            {
                owner = "Cylon";
            }
            else if (Directory.Exists("C:/Users/ALEX/StarportGE/ChatLogs"))
            {
                owner = "Laptop";
            }
            await DBUTask.OutprintAsync("Owner: " + owner + '\n'
                        + "We Lost: " + TaskInitator.planetsLost + '\n'
                        + "We Kaptured: " + TaskInitator.planetsKaptured + '\n'
                        + "Allies Slain: " + TaskInitator.alliesSlain + '\n'
                        + "Enemies Slain: " + TaskInitator.enemiesSlain + '\n'
                        + "landings: " + TaskInitator.landings + '\n'
                        + "Colonies Abanonded: " + TaskInitator.colsAbandoned + '\n'
                        + "Colonies Built: " + TaskInitator.colsBuilt, ChannelID.botCommandsID);
        }

        [Command("run Info")]
        public async Task InfoPost([Remainder] string text)
        {
            string[] temp = text.Split(" ");

            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = temp[i].Trim();
            }

            TaskInitator.planetsLost = uint.Parse(temp[0]);
            TaskInitator.planetsKaptured = uint.Parse(temp[1]);
            TaskInitator.alliesSlain = uint.Parse(temp[2]);
            TaskInitator.enemiesSlain = uint.Parse(temp[3]);
            TaskInitator.landings = uint.Parse(temp[4]);
            TaskInitator.colsAbandoned = uint.Parse(temp[5]);
            TaskInitator.colsBuilt = uint.Parse(temp[6]);

            await InfoGet();
        }

        [Command("request Lifetime")]
        public async Task LifetimeGet()
        {
            System.TimeSpan lifetime = System.DateTime.Now - Program.dateTime;
            await ReplyAsync("Initiated at " + Program.dateTime + " EST. Has been running for: " +
                '\n' +
                lifetime.Days + " Days " +
                '\n' +
                lifetime.Hours + " Hours " +
                '\n' +
                lifetime.Minutes + " Minutes " +
                '\n' +
                lifetime.Seconds + " Seconds"
                );
        }

        [Command("request Listeners")]
        public async Task ListenerActivityGet()
        {
            await ReplyAsync(
                "Building: " + TaskInitator.building +
                '\n' +
                "Distress: " + TaskInitator.distress +
                '\n' +
                "Kombat: " + TaskInitator.kombat +
                '\n' +
                "Server Reset: " + TaskInitator.alerts);
        }

        [Command("run ListenerChatLog")]
        public async Task ListenerChatLogPost([Remainder] string text)
        {
            await ReplyAsync("By Your Command! Listening for Chatlog changes!");

            if (text.Equals("All"))
            {
                await ChatLogListener(ChannelID.botUpdatesID, "Chat Log Listener", "Client");
                /*
                foreach (string botName in cylons)
                {
                    await ChatLogListener(ChannelID.botUpdatesID, "Chat Log Listener", botName);
                }*/
            }
            else if (cylons.Any(s => text.Contains(s)))
            {
                await ChatLogListener(ChannelID.botUpdatesID, "Chat Log Listener", text);
            }
            else if (text.Equals("Client"))
            {
                await ChatLogListener(ChannelID.botUpdatesID, "Chat Log Listener", "Client");
            }
        }

        [Command("run Listener")]
        public async Task ListenerPost([Remainder] string text)
        {
            if (listenerNames.Any(s => text.Contains(s)))
            {
                List<string> listenerList = listenerNames.ToList<string>();
                string listener = listenerList.Find(s => text.Contains(s));

                if (listener.Equals("building"))
                {
                    await Task.Run(() => init.SetBuildingAsync(true));
                }
                else if (listener.Equals("distress"))
                {
                    await Task.Run(() => init.SetDistressAsync(true));
                }
                else if (listener.Equals("kombat"))
                {
                    await Task.Run(() => init.SetKombatAsync(true));
                }
                else if (listener.Equals("alerts"))
                {
                    await Task.Run(() => init.SetAlertsAsync(true));
                }
                else if (text.Equals("All"))
                {
                    await Task.Run(() => init.SetAllAsync(true));
                }
                await ReplyAsync("By Your Command! Listening for " + listener + " updates");
            }
        }

        [Command("run StopListener")]
        public async Task ListenerStop([Remainder] string text)
        {
            if (listenerNames.Any(s => text.Contains(s)))
            {
                List<string> listenerList = listenerNames.ToList<string>();
                string listener = listenerList.Find(s => text.Contains(s));

                if (listener.Equals("building"))
                {
                    await Task.Run(() => init.SetBuildingAsync(false));
                }
                else if (listener.Equals("distress"))
                {
                    await Task.Run(() => init.SetDistressAsync(false));
                }
                else if (listener.Equals("kombat"))
                {
                    await Task.Run(() => init.SetKombatAsync(false));
                }
                else if (listener.Equals("serverResets"))
                {
                    await Task.Run(() => init.SetAlertsAsync(false));
                }
                else if (listener.Equals("All"))
                {
                    await Task.Run(() => init.SetAllAsync(false));
                }
                await ReplyAsync("By Your Command! Stopped Listening for " + listener + " updates");
            }
        }

        [Command("Ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong");
        }

        [Command("request planet")]
        public async Task PlanetPictureGet([Remainder] string text)
        {
            string[] allfiles = Directory.GetFiles("G:/My Drive/Personal Stuff/Starport/PlanetPictures", "*.*", SearchOption.AllDirectories);
            string textPNG = text + ".png";

            foreach (string file in allfiles)
            {
                if (textPNG.Equals(Path.GetFileName(file)))
                {
                    System.Console.WriteLine(textPNG
                       + '\n' + file);

                    DirectoryInfo directoryInfo = Directory.GetParent(file);
                    //System.Console.WriteLine(directoryInfo.Name);
                    if (directoryInfo.Name == "PlanetPictures")
                    {
                        await DBUTask.OutprintAsync("Friendly Planet Requested: ", ChannelID.planetPicturesID);
                        await DBUTask.OutprintFileAsync(file, ChannelID.planetPicturesID);

                        await ReplyAsync("File printed to planet pictures!");
                    }
                    else if (directoryInfo.Name == "Enemy Planets")
                    {
                        await DBUTask.OutprintAsync("Enemy Planet Requested: ", ChannelID.slaversID);
                        await DBUTask.OutprintFileAsync(file, ChannelID.slaversID);
                        if (File.Exists(directoryInfo.FullName + "/" + text + ".txt"))
                        {
                            await DBUTask.OutprintFileAsync(directoryInfo.FullName + "/" + text + ".txt", ChannelID.slaversID);
                        }
                        await ReplyAsync("File printed to slavers!");
                    }
                    else if (directoryInfo.Name == "Undomed")
                    {
                        await DBUTask.OutprintAsync("Friendly Planet Requested: ", ChannelID.buildingID);
                        await DBUTask.OutprintFileAsync(file, ChannelID.buildingID);
                        await ReplyAsync("File printed to building!");
                    }

                    return;
                }
            }

            await ReplyAsync("no planet was found in our folders!");
        }

        [Command("run planetsCaptured")]
        public async Task PlanetsCapturedChange([Remainder] string text)
        {
            text = text.Trim();
            TaskInitator.planetsKaptured = uint.Parse(text);

            await DBUTask.OutprintAsync("We Kaptured: " + TaskInitator.planetsKaptured, ChannelID.botCommandsID);
        }

        [Command("run planetsLost")]
        public async Task PlanetsLostChange([Remainder] string text)
        {
            text = text.Trim();
            TaskInitator.planetsLost = uint.Parse(text);

            await DBUTask.OutprintAsync("We Lost: " + TaskInitator.planetsLost, ChannelID.botCommandsID);
        }

        [Command("request planetTallies")]
        public async Task PlanetTalliesGet()
        {
            await DBUTask.OutprintAsync(
                "We Lauwst: " + TaskInitator.planetsLost + '\n'
                + "We Kaptured: " + TaskInitator.planetsKaptured, ChannelID.slaversID);
        }

        [Command("request planetsQuote")]
        public async Task QuoteGet()
        {
            string quote = "";
            /*
            string quote = "Arc " + arcticsZ + "/" + arctics +
                "|~{yellow}~Des " + desertsZ + "/" + deserts +
                "|~{green}~Earth " + earthsZ + "/" + earthlikes +
                "|~{orange}~Green " + greenhousesZ + "/" + greenhouses +
                "|~{purple}~Mount " + mountainsZ + "/" + mountains +
                "|~{blue}~Oce " + oceansZ + "/" + oceans +
                "|~{pink}~IGPs ~{link}1:" + paradises + "~" +
                "|~{gray}~Roc " + rockiesZ + "/" + rockies +
                "|~{red}~Volc " + volcanicsZ + "/" + volcanics +
                "|~{link}25:Caps:~ " + invasions +
                "|~{green}~Traded: " + traded +
                "|~{cyan}~" + totalsZ + " Zounds/" + totals + "~{link}21: Cols~";*/

            await ReplyAsync(quote);
        }

        [Command("run readExcel")]
        public async Task ReadExcelDocument()
        {
            await Task.Run(() => init.LoadExcelHoldingsAsync());
        }

        [Command("run readFolders")]
        public async Task ReadPlanetPicturesAndInfoFolders()
        {
            _ = Task.Run(() => init.ReadPlanetPicturesAndInfoFoldersAsync());
            await Task.Delay(1);
        }

        [Command("request RunningTasks")]
        public async Task RunningTasksGet()
        {
            if (DBUTask.runningTasks.Count > 0)
            {
                foreach (DBUTask.DBUTaskObj task in DBUTask.runningTasks)
                {
                    await ReplyAsync("TaskID: " + task.task.Id + " | " + "Task Purpose: " + task.purpose + " | Task Owner: " + task.owner + " | Initiated at " + task.timeStarted + " | Lifetime: " + SecondsToTime(task.ticker));
                }
            }
            else
            {
                await ReplyAsync("No Tasks are currently Running!");
            }
        }

        internal async Task ChatLogListener(ulong channelID, string purpose, string owner)
        {
            uint listenerNum = DBUTask.dbuTaskNum++;
            Task task = Task.Run(() => init.ChatLogListenerAsync(listenerNum, channelID, owner));
            DBUTask.runningTasks.Add(new DBUTask.DBUTaskObj(task, purpose, owner, listenerNum, null));
            await Task.Delay(500);
        }

        internal async Task MessageUpdater(string purpose, string owner)
        {
            uint id = DBUTask.dbuTaskNum++;
            Task task = Task.Run(() => init.TextUpdaterAsync(id));
            DBUTask.runningTasks.Add(new DBUTask.DBUTaskObj(task, purpose, owner, id, null));
            await Task.Delay(500);
        }

        internal async Task PictureUpdater(ulong channelID, string purpose, string owner)
        {
            uint picturesNum = DBUTask.dbuTaskNum++;
            Task task = Task.Run(() => init.PictureUpdaterAsync(picturesNum));
            DBUTask.runningTasks.Add(new DBUTask.DBUTaskObj(task, purpose, owner, picturesNum, null));
            await Task.Delay(500);
        }

        private string SecondsToTime(uint seconds)
        {
            System.TimeSpan timespan = new System.TimeSpan(0, 0, 0, (int)seconds);
            string timeSpanString = timespan.Days + " Days:" + timespan.Hours + ":" + timespan.Minutes + ":" + timespan.Seconds + "";
            return timeSpanString;
        }
    }
}