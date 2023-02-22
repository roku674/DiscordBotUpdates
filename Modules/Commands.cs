//Created by Alexander Fields https://github.com/roku674
using Discord.Commands;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotUpdates.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        internal TaskInitiator init = new TaskInitiator();

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

                    await ReplyAsync("TaskId: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted + " | Lifetime: " + SecondsToTime(dbuTask.ticker)
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

                    List<string> botNamesList = Program.bots.cylons.ToList<string>();
                    string bot = botNamesList.Find(s => text.Contains(s));

                    if (dbuTask.owner.Equals(bot))
                    {
                        await ReplyAsync("TaskId: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted + " | Lifetime: " + SecondsToTime(dbuTask.ticker)
                            + '\n'
                            + "Was ended at " + System.DateTime.Now);

                        dbuTask.Cancel();
                        DBUTask.runningTasks.Remove(dbuTask);
                    }
                    else if (dbuTask.owner.Equals("Client"))
                    {
                        await ReplyAsync("TaskId: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted + " | Lifetime: " + SecondsToTime(dbuTask.ticker)
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
            //await MessageUpdater(Program.channelId.distressCallsId, "Distress Signal Updater", "Client", "distress");
            //await MessageUpdater(Program.channelId.slaversId, "Warped In & Out Updater", "Client", "warpedInOut");
        }    

        [Command("run ClearProgram.botsEcho")]
        public async Task ClearBotsEcho()
        {
            string[] files = Directory.GetFiles(Program.filePaths.networkPathDir + "/Echo", "*.txt");

            for (int i = 0; i < files.Length; i++)
            {
                await File.WriteAllTextAsync(files[i], "");

                await ReplyAsync(Path.GetFileName(files[i]) + "Text Files Cleared!");
            }
        }

        [Command("run ConvertCSV")]
        public async Task ConvertCSV()
        {
            string path = "";
            if (File.Exists(Program.filePaths.csvLocal))
            {
                path = Program.filePaths.csvLocal;
            }

            await ExcelCSharp.Excel.ConvertFromCSVtoXLSXAsync(path, Program.filePaths.excelPath);

            await ReplyAsync("Finished converting csv to xlsx!");
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
            if (Program.bots.cylons.Any(s => text.Contains(s)))
            {
                List<string> botNamesList = Program.bots.cylons.ToList<string>();

                string bot = botNamesList.Find(s => text.Contains(s));

                string botEcho = text.Replace(bot + " ", "");
                botEcho.TrimStart();

                if (File.Exists(Program.filePaths.networkPathDir + "/Echo/" + bot + ".txt"))
                {
                    await File.AppendAllTextAsync(Program.filePaths.networkPathDir + "/Echo/" + bot + ".txt", botEcho);
                }
                else
                {
                    File.Create(Program.filePaths.networkPathDir + "/Echo/" + bot + ".txt").Close();
                    await ReplyAsync("Created" + bot + ".txt ! Recommend ReRunning!");
                }

                await ReplyAsync("Sending: " + botEcho + " to " + bot);
            }
        }

        [Command("request enemyPlanets")]
        public async Task EnemyPlanetsGet([Remainder] string text)
        {
            await Task.Run(() => init.FindEnemyColoniesAsync(text, Program.filePaths.enemyPlanetsDir));
        }

        [Command("request hourlyRedomes")]
        public async Task HourlyRedomeGet()
        {
            _ = Task.Run(() => init.FindHourlyRedomesAsync());
            await ReplyAsync("Hourly redomes posted to redome channel!");
        }

        [Command("request List")]
        public async Task ListGet([Remainder] string text)
        {
            if (text == "All")
            {
                await init.FindListAsync(text);
                await init.FindListAsync("DD");
                await init.FindListAsync("Military");
                await init.FindListAsync("PollutionCrit");
                await init.FindListAsync("Solar Off");
                await init.FindListAsync("Solar Weak");
                await init.FindListAsync("Zoundsables");
            }
            else
            {
                await Task.Run(() => init.FindListAsync(text));
            }
        }

        [Command("request planet")]
        public async Task PlanetGet([Remainder] string text)
        {
            await ReplyAsync($"Finding {text}...");
            await ReplyAsync(await init.FindPlanetAsync(text));
        }

        [Command("request System")]
        public async Task SystemGet([Remainder] string text)
        {
            await ReplyAsync(await init.FindSystemAsync(text));
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
                "    request hourlyRedomes" +
                '\n' +
                "    request Info" +
                '\n' +
                "    request List (Type)" +
                '\n' +
                "    request Lifetime" +
                '\n' +
                "    request Listeners" +
                '\n' +
                "    request OwnedSystems" +
                '\n' +
                "    request planet (planetName)" +
                '\n' +
                "    request planetsQuote" +
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
                "    run ClearProgram.botsEcho" +
                 '\n' +
                "    run ConvertCSV" +
                '\n' +
                "    run Deactivate" +
                '\n' +
                "    run Echo (BotName) (Command/Chat) (line)" +
                '\n' +
                "    run FindRemainingLogs" +
                '\n' +
                "    run Info" +
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
            if (Directory.Exists(Program.filePaths.starportDir))
            {
                DirectoryInfo directory = Directory.GetParent(Program.filePaths.starportDir);
                owner = directory.Name;
            }

            await DBUTask.OutprintAsync("Owner: " + owner + '\n'
                        + "We Lost: " + TaskInitiator.planetsLost + '\n'
                        + "We Kaptured: " + TaskInitiator.planetsKaptured + '\n'
                        + "Allies Slain: " + TaskInitiator.alliesSlain + '\n'
                        + "Enemies Slain: " + TaskInitiator.enemiesSlain + '\n'
                        + "landings: " + TaskInitiator.landings + '\n'
                        + "Colonies Abanonded: " + TaskInitiator.colsAbandoned + '\n'
                        + "Colonies Built: " + TaskInitiator.colsBuilt, Program.channelId.botCommandsId);
        }

        [Command("run Info")]
        public async Task InfoPost([Remainder] string text)
        {
            string[] temp = text.Split(" ");

            for (int i = 0; i < temp.Length; i++)
            {
                temp[i] = temp[i].Trim();
            }

            TaskInitiator.planetsLost = uint.Parse(temp[0]);
            TaskInitiator.planetsKaptured = uint.Parse(temp[1]);
            TaskInitiator.alliesSlain = uint.Parse(temp[2]);
            TaskInitiator.enemiesSlain = uint.Parse(temp[3]);
            TaskInitiator.landings = uint.Parse(temp[4]);
            TaskInitiator.colsAbandoned = uint.Parse(temp[5]);
            TaskInitiator.colsBuilt = uint.Parse(temp[6]);

            await InfoGet();
        }

        [Command("run FindRemainingLogs")]
        public async Task FindRemainingLogs()
        {
            _ = Task.Run(() => init.FindRemainingLogs());
            await ReplyAsync("Remaining Logs Loaded");
        }

        [Command("request OwnedSystems")]
        public async Task OwnedSystemsGet()
        {
            await ReplyAsync("Finding every system...");
            _ = Task.Run(() => init.FindEverySystemWithColonies());
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
                "Building: " + TaskInitiator.building +
                '\n' +
                "Distress: " + TaskInitiator.distress +
                '\n' +
                "Kombat: " + TaskInitiator.kombat +
                '\n' +
                "Server Reset: " + TaskInitiator.alerts);
        }

        [Command("run ListenerChatLog")]
        public async Task ListenerChatLogPost([Remainder] string text)
        {
            await ReplyAsync("By Your Command! Listening for Chatlog changes!");

            if (text.Equals("All"))
            {
                await ChatLogListener(Program.channelId.botUpdatesId, "Chat Log Listener", "Client");
                /*
                foreach (string botName in cylons)
                {
                    await ChatLogListener(Program.channelId.botUpdatesId, "Chat Log Listener", botName);
                }*/
            }
            else if (Program.bots.cylons.Any(s => text.Contains(s)))
            {
                await ChatLogListener(Program.channelId.botUpdatesId, "Chat Log Listener", text);
            }
            else if (text.Equals("Client"))
            {
                await ChatLogListener(Program.channelId.botUpdatesId, "Chat Log Listener", "Client");
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
            await Task.Run(() => TaskInitiator.PingAPI());
            await ReplyAsync("Pong");
        }

        [Command("run planetsCaptured")]
        public async Task PlanetsCapturedChange([Remainder] string text)
        {
            text = text.Trim();
            TaskInitiator.planetsKaptured = uint.Parse(text);

            await DBUTask.OutprintAsync("We Kaptured: " + TaskInitiator.planetsKaptured, Program.channelId.botCommandsId);
        }

        [Command("run planetsLost")]
        public async Task PlanetsLostChange([Remainder] string text)
        {
            text = text.Trim();
            TaskInitiator.planetsLost = uint.Parse(text);

            await DBUTask.OutprintAsync("We Lost: " + TaskInitiator.planetsLost, Program.channelId.botCommandsId);
        }

        [Command("request planetTallies")]
        public async Task PlanetTalliesGet()
        {
            await DBUTask.OutprintAsync(
                "We Lauwst: " + TaskInitiator.planetsLost + '\n'
                + "We Kaptured: " + TaskInitiator.planetsKaptured, Program.channelId.slaversId);
        }

        [Command("request planetsQuote")]
        public async Task QuoteGet([Remainder] string owner)
        {
            uint arctics = 0, arcticsZ = 0, deserts = 0, desertsZ = 0, earths = 0, earthsZ = 0, greenhouses = 0, greenhousesZ = 0, mountains = 0, mountainsZ = 0, oceans = 0, oceansZ = 0, paradises = 0, paradisesZ = 0, rockies = 0, rockiesZ = 0, volcanics = 0, volcanicsZ = 0, invasions = 0, dd = 0;

            if (TaskInitiator.holdingsList != null)
            {
                foreach (StarportObjects.Holding planet in TaskInitiator.holdingsList)
                {
                    if (planet.Owner.Equals(owner))
                    {
                        if (planet.Name.EndsWith(".I") || planet.Name.EndsWith(".ZI") || planet.Name.EndsWith(".ZDI"))
                        {
                            invasions++;
                        }
                        if (planet.Name.EndsWith(".D") || planet.Name.EndsWith(".DI") || planet.Name.Contains(".ZD"))
                        {
                            dd++;
                        }
                        if (planet.Population >= 5000)
                        {
                            if (planet.PlanetType.Equals("arctic"))
                            {
                                arctics++;
                                if (planet.Population >= 90000)
                                {
                                    arcticsZ++;
                                }
                            }
                            else if (planet.PlanetType.Equals("desert"))
                            {
                                deserts++;
                                if (planet.Population >= 90000)
                                {
                                    desertsZ++;
                                }
                            }
                            else if (planet.PlanetType.Equals("earthlike"))
                            {
                                earths++;
                                if (planet.Population >= 90000)
                                {
                                    earthsZ++;
                                }
                            }
                            else if (planet.PlanetType.Equals("greenhouse"))
                            {
                                greenhouses++;
                                if (planet.Population >= 90000)
                                {
                                    greenhousesZ++;
                                }
                            }
                            else if (planet.PlanetType.Equals("mountainous"))
                            {
                                mountains++;
                                if (planet.Population >= 90000)
                                {
                                    mountainsZ++;
                                }
                            }
                            else if (planet.PlanetType.Equals("oceanic"))
                            {
                                oceans++;
                                if (planet.Population >= 90000)
                                {
                                    oceansZ++;
                                }
                            }
                            else if (planet.PlanetType.Equals("Intergalactic paradise"))
                            {
                                paradises++;
                                if (planet.Population >= 90000)
                                {
                                    paradisesZ++;
                                }
                            }
                            else if (planet.PlanetType.Equals("rocky"))
                            {
                                rockies++;
                                if (planet.Population >= 90000)
                                {
                                    rockiesZ++;
                                }
                            }
                            else if (planet.PlanetType.Equals("volcanic"))
                            {
                                volcanics++;
                                if (planet.Population >= 90000)
                                {
                                    volcanicsZ++;
                                }
                            }
                        }
                    }
                }

                uint totals = arctics + deserts + earths + greenhouses + mountains + oceans + paradises + rockies + volcanics;
                uint totalsZ = arcticsZ + desertsZ + earthsZ + greenhousesZ + mountainsZ + oceansZ + paradisesZ + rockies + volcanics;

                string quote = "Arc " + arcticsZ + "/" + arctics +
                    "|~{yellow}~Des " + desertsZ + "/" + deserts +
                    "|~{green}~Earth " + earthsZ + "/" + earths +
                    "|~{orange}~Green " + greenhousesZ + "/" + greenhouses +
                    "|~{purple}~Mount " + mountainsZ + "/" + mountains +
                    "|~{blue}~Oce " + oceansZ + "/" + oceans +
                    "|~{pink}~IGPs ~{link}1:" + paradises + "~" +
                    "|~{gray}~Roc " + rockiesZ + "/" + rockies +
                    "|~{red}~Volc " + volcanicsZ + "/" + volcanics +
                    "|~{link}25:Caps:~ " + invasions +
                    "|~{green}~DDs: " + dd +
                    "|~{cyan}~" + totalsZ + " Zounds/" + totals + "~{link}21: Cols~";
                await ReplyAsync(quote);
            }
            else
            {
                await ReplyAsync("Holdings list was null!");
            }
        }

        [Command("run readExcel")]
        public async Task ReadExcelDocument()
        {
            _ = Task.Run(() => TaskInitiator.LoadExcelHoldingsAsync());
            await ReplyAsync("Running Load Holdings...");
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
                    await ReplyAsync("TaskId: " + task.task.Id + " | " + "Task Purpose: " + task.purpose + " | Task Owner: " + task.owner + " | Initiated at " + task.timeStarted + " | Lifetime: " + SecondsToTime(task.ticker));
                }
            }
            else
            {
                await ReplyAsync("No Tasks are currently Running!");
            }
        }

        internal async Task ChatLogListener(ulong channelId, string purpose, string owner)
        {
            uint listenerNum = DBUTask.dbuTaskNum++;
            Task task = Task.Run(() => init.ChatLogListenerAsync(listenerNum, channelId, owner));
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

        private string SecondsToTime(uint seconds)
        {
            System.TimeSpan timespan = new System.TimeSpan(0, 0, 0, (int)seconds);
            string timeSpanString = timespan.Days + " Days:" + timespan.Hours + ":" + timespan.Minutes + ":" + timespan.Seconds + "";
            return timeSpanString;
        }
    }
}