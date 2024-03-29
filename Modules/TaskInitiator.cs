﻿//Created by Alexander Fields https://github.com/roku674

using StarportObjects;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Data;
using System.Net.Http;
using Optimization.Utility;
using Optimization.Objects;
using System;
using Newtonsoft.Json;

namespace DiscordBotUpdates.Modules
{
    internal class TaskInitiator : DBUTask
    {
        public static bool alerts {
            get; set;
        }

        public static uint alliesSlain {
            get; set;
        }

        public static bool building {
            get; set;
        }

        public static uint colsAbandoned {
            get; set;
        }

        public static uint colsBuilt {
            get; set;
        }

        public static string currentServer {
            get;set;
        }

        public static bool distress {
            get; set;
        }

        public static uint enemiesSlain {
            get; set;
        }

        public static List<Holding> holdingsList {
            get; set;
        }

        public static bool kombat {
            get; set;
        }

        public static uint landings {
            get; set;
        }

        public static bool loadingExcel {
            get; set;
        }

        public static string lastLand {
            get; set;
        }

        public static string lastSystem {
            get; set;
        }

        public static uint planetsKaptured {
            get; set;
        }

        public static uint planetsLost {
            get; set;
        }

        public static async Task LoadExcelHoldingsAsync()
        {
            if (!loadingExcel)
            {
                loadingExcel = true;
                holdingsList = new List<Holding>();

                HttpClient client = new HttpClient();

                client = APICaller.AddRequestHeaders(client,
                    Settings.Configuration["API:StarportGE:host"],
                    Settings.Configuration["API:StarportGE:keyName"],
                    Settings.Configuration["API:StarportGE:key"],
                    null);

                string month = "";
                string day = "";

                if (DateTime.Now.Month < 10)
                {
                    month = "0" + DateTime.Now.Month;
                }
                else
                {
                    month = DateTime.Now.Month.ToString();
                }
                if (DateTime.Now.Day < 10)
                {
                    day = "0" + DateTime.Now.Day;
                }
                else
                {
                    day = DateTime.Now.Day.ToString();
                }

                FileInfo fileInfo = new FileInfo(Program.filePaths.csvLocal);
                FileObj file = new FileObj(
                    $"holdings_{DateTime.Now.Year}{month}{day}.csv",
                    "csv",
                    System.IO.File.ReadAllText(Program.filePaths.csvLocal),
                    File.ReadAllBytes(Program.filePaths.csvLocal),
                    fileInfo.CreationTime,
                    fileInfo.LastAccessTime,
                    fileInfo.Length,
                    null,
                    null
                    );

                string fileJson = Newtonsoft.Json.JsonConvert.SerializeObject(file);

                if (string.IsNullOrWhiteSpace(currentServer))
                {
                    currentServer = "IC4";
                    System.Console.WriteLine("Current server set to " + currentServer);
                }

                try
                {
                    string resposne = APICaller.PutJson(
                        client,
                        fileJson,
                        $"{Settings.Configuration["API:StarportGE:url"]}/{Settings.Configuration["API:StarportGE:Put:csv"]}server={currentServer}"
                        ).Result;

                    string json = APICaller.GetResponseBodyFromApiAsync(client,
                    $"{Settings.Configuration["API:StarportGE:url"]}/{Settings.Configuration["API:StarportGE:Get:allcolonies"]}server={currentServer}",
                    0).Result;

                    holdingsList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Holding>>(json);
                }
                catch (System.Exception e)
                {
                    string tempFile = Path.GetTempFileName().Replace(".tmp", ".txt");
                    File.WriteAllText(tempFile, fileJson);

                    await OutprintAsync(e.ToString(), Program.channelId.botErrorsId);
                    await OutprintFileAsync(tempFile, Program.channelId.botErrorsId);

                    File.Delete(tempFile);
                }

                client.Dispose();
                await OutprintAsync("Excel Document Sucessfully loaded into memory!I found " + holdingsList.Count + " Colonies!", Program.channelId.botUpdatesId);
                loadingExcel = false;
            }
        }

        public static async Task<bool> PingAPI()
        {
            HttpClient client = new HttpClient();

            client = APICaller.AddRequestHeaders(client,
                Settings.Configuration["API:StarportGE:host"],
                Settings.Configuration["API:StarportGE:keyName"],
                Settings.Configuration["API:StarportGE:key"],
                null);

            string ping = APICaller.GetResponseBodyFromApiAsync(client,
               $"{Settings.Configuration["API:StarportGE:url"]}/{Settings.Configuration["API:StarportGE:Get:ping"]}",
               0).Result;

            if (!ping.Contains("API"))
            {
                await OutprintAsync("API is down!", Program.channelId.botUpdatesId);
                await OutprintAsync("API is down!", Program.channelId.botErrorsId);

                return false;
            }
            return true;
        }

        /// </summary> <summary> Call this to start the Distress Calls Listener </summary>
        /// <returns></returns>
        public async Task ChatLogListenerAsync(uint id, ulong channelId, string owner)
        {
            System.Console.WriteLine("ChatLog Listener Executed!");

            FileSystemWatcher watcher = new FileSystemWatcher();

            if (owner.Equals("Client"))
            {
                if (Directory.Exists(Program.filePaths.chatLogsDir))
                {
                    watcher.Path = Program.filePaths.chatLogsDir;
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

                Discord.IMessageChannel channel = Program.client.GetChannel(channelId) as Discord.IMessageChannel;
                //await channel.SendMessageAsync("Sucessfully ran ChatLog Listener!");
                System.Console.WriteLine("Sucessfully ran ChatLog Listener!");

                await Task.Delay(2000);

                int taskNum = runningTasks.FindIndex(task => task.id == id);
                DBUTaskObj task = runningTasks.ElementAt(taskNum);

                for (uint i = 0;i < duration;i++)
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

        public async Task FindEnemyColoniesAsync(string enemy, string folder)
        {
            //System.Console.WriteLine("Finding " + enemy + " colonies!");
            await OutprintAsync(enemy + "'s planets:", Program.channelId.scoutReportsId);

            await System.IO.File.WriteAllTextAsync(folder + "/enemyCols.txt", " ");
            foreach (string fileName in Directory.EnumerateFiles(folder, "*.txt"))
            {
                //System.Console.WriteLine(fileName);
                string fileContents = System.IO.File.ReadAllText(fileName);
                if (fileContents.Contains(enemy))
                {
                    await System.IO.File.AppendAllTextAsync(folder + "/enemyCols.txt", Path.GetFileNameWithoutExtension(fileName) + '\n');
                }
            }
            await OutprintFileAsync(folder + "/enemyCols.txt", Program.channelId.scoutReportsId);
            Algorithms.FileManipulation.DeleteFile(folder + "/enemyCols.txt");
        }

        /// <summary>
        /// Finds every system with colonies and outputs a json to botupdates
        /// </summary>
        /// <returns></returns>
        public async Task FindEverySystemWithColonies()
        {
            await LoadExcelHoldingsAsync();

            List<System.Numerics.Vector2> planetarySystemCoords = new List<System.Numerics.Vector2>();
            List<Holding> localHoldingsList = holdingsList;

            foreach (Holding planet in localHoldingsList)
            {
                int pscIndex = planetarySystemCoords.FindIndex(v2 =>
                v2.X == (float)planet.GalaxyX &&
                v2.Y == (float)planet.GalaxyY
                );

                if (pscIndex == -1)
                {
                    planetarySystemCoords.Add(new System.Numerics.Vector2(planet.GalaxyX, planet.GalaxyY));
                }
            }

            planetarySystemCoords = Algorithms.Mathematics.SortByDistanceVector2(planetarySystemCoords);
            List<string> planetarySystemCoordsAsStr = new List<string>();

            foreach (System.Numerics.Vector2 vector2 in planetarySystemCoords)
            {
                planetarySystemCoordsAsStr.Add("(" + vector2.X + "," + vector2.Y + ")");
            }
            string json = Directory.GetCurrentDirectory() + "/planetarySystemCoords.json";

            Algorithms.FileManipulation.DeleteFile(json);
            File.Create(json).Close();
            await File.WriteAllTextAsync(json, Newtonsoft.Json.JsonConvert.SerializeObject(planetarySystemCoordsAsStr));

            await OutprintFileAsync(Directory.GetCurrentDirectory() + "/planetarySystemCoords.json", Program.channelId.botUpdatesId);

            Algorithms.FileManipulation.DeleteFile(json);
        }

        public async Task FindHourlyRedomesAsync()
        {
            System.Console.WriteLine("Finding Hourly Redomes...");
            Google.Apis.Calendar.v3.EventsResource.ListRequest request = Program.service.Events.List("primary");
            request.TimeMin = DateTime.Now;
            request.TimeMax = DateTime.Now.AddHours(1);
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 50;
            request.OrderBy = Google.Apis.Calendar.v3.EventsResource.ListRequest.OrderByEnum.StartTime;

            Google.Apis.Calendar.v3.Data.Events calendarEvents = request.Execute();

            if (calendarEvents.Items != null && calendarEvents.Items.Count > 0)
            {
                foreach (Google.Apis.Calendar.v3.Data.Event calendarEvent in calendarEvents.Items)
                {
                    string atUser = "";
                    if (AtUser(calendarEvent.Summary) != "")
                    {
                        atUser = AtUser(calendarEvent.Summary);
                    }
                    else if (AtUser(calendarEvent.Description) != "")
                    {
                        atUser = AtUser(calendarEvent.Description);
                    }

                    await OutprintAsync(atUser + "Upcoming Redome Time: " + calendarEvent.Start.DateTime + "EST" + '\n'
                        + calendarEvent.Summary + '\n'
                        + calendarEvent.Description,
                        Program.channelId.redomeId
                        );
                }
            }
        }

        public async Task FindListAsync(string type)
        {
            ulong channel = Program.channelId.botUpdatesId;

            await LoadExcelHoldingsAsync();
            List<Holding> localHoldingsList = holdingsList;

            string tempPath = Directory.GetCurrentDirectory() + "/Temp" + type + "Dir/" + type + "Corporate.txt";
            string tempDir = Directory.GetCurrentDirectory() + "/Temp" + type + "Dir";

            Algorithms.FileManipulation.CreateDirectory(tempDir);

            await File.WriteAllTextAsync(tempPath, "");

            Holding origin = holdingsList[0];

            localHoldingsList = StarportHelperClasses.HoldingsSorter.SortByDistance(localHoldingsList, origin);
            int ddCount = 0, negativeGrowth = 0, negativeMorale = 0, polluting = 0, pollutingCrit = 0, solarOff = 0, solarWeak = 0, militaryWeak = 0;

            if (type == "Pollution")
            {
             
            }
            else if (type == "PollutionCrit")
            {
             
            }
            else if (type == "Zoundsables")
            {
             
            }
            else if (type == "DD")
            {
               
            }
            else if (type == "Solar Off")
            {
                channel = Program.channelId.solarOffId;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.SolarShots == 0 && planet.Population > 1000 || (planet.SolarShots > 0 && planet.Population > 10000 && planet.Ore <= 5000))
                    {
                        solarOff++;
                        string message = AllPlanetInfo(planet);
                        await UpdateCompanionFilesAsync(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "Solar Weak")
            {
              
            }
            else if (type == "MilitaryTest")
            {
               
            }
            else if (type == "Revolt")
            {
               
            }
            else if (type == "Shrinking")
            {
                
            }
            else if (type == "All")
            {

            }
            else
            {
                await OutprintAsync(type + " was not recognized!", Program.channelId.botErrorsId);
            }

            await OutprintAsync("Totals: " + '\n'
                           + "DD's: " + ddCount + '\n'
                           + "Negative Growth: " + negativeGrowth + '\n'
                           + "Negative Morale: " + negativeMorale + '\n'
                           + "Polluting: " + polluting + '\n'
                           + "  Critial : " + pollutingCrit + '\n'
                           + "Solars:" + '\n'
                           + "  Disabled/Off: " + solarOff + '\n'
                           + "  Weak: " + solarWeak + '\n'
                           + "Military: " + '\n'
                           + "  Weak: " + militaryWeak
                           ,
                           channel);

            //print out the files
            foreach (string file in Directory.GetFiles(tempDir))
            {
                if (AtUser(file) != "")
                {
                    await OutprintAsync(AtUser(file) + DateTime.Now.ToString(), channel);

                    if (!string.IsNullOrEmpty(File.ReadAllText(file)))
                    {
                        await OutprintFileAsync(file, channel);
                    }
                    else
                    {
                        await OutprintAsync("No " + type + " was found!", channel);
                    }

                    Algorithms.FileManipulation.DeleteFile(file);
                }
            }
            await OutprintFileAsync(tempPath, channel);
            Algorithms.FileManipulation.DeleteFile(tempPath);

            //clean up remaining files
            string[] remainingFiles = Directory.GetFiles(tempDir);
            foreach (string file in remainingFiles)
            {
                Algorithms.FileManipulation.DeleteFile(file);
            }
            Algorithms.FileManipulation.DeleteDirectory(tempDir, true);
            System.Console.WriteLine("DELETED! " + tempDir);
        }

        public async Task FindRemainingLogs()
        {
            string remainingLogs = Program.filePaths.chatLogsDir + "/RemainingLogs.txt";

            if (File.Exists(remainingLogs))
            {
                string[] remainderLogsArr = File.ReadAllLines(remainingLogs);
                while (remainderLogsArr.Length > 1)
                {
                    for (int i = 0;i < remainderLogsArr.Length;i++)
                    {
                        ChatLogsReaderAsync(remainderLogsArr, "Remainder").Wait();

                        remainderLogsArr = remainderLogsArr.Take(remainderLogsArr.Length - 1).ToArray();
                    }

                    await File.WriteAllLinesAsync(remainingLogs, remainderLogsArr);

                    System.Console.WriteLine("Lines remaining in file: " + remainderLogsArr.Length);

                    if (remainderLogsArr.Length < 1)
                    {
                        System.Console.WriteLine("Remainder file deleted");
                        await OutprintAsync(AtUser("Autism") + remainingLogs + "Deleted!", Program.channelId.botUpdatesId);
                        Algorithms.FileManipulation.DeleteFile(remainingLogs);
                    }
                }
            }
        }

        /// <summary>
        /// Call this to start the Message Updater
        /// </summary>
        /// <returns></returns>
        public async Task TextUpdaterAsync(uint id)
        {
            //await Outprint("Sucessfully Initiated " + type + " Listener!", Program.channelId.botCommandsId);
            await Task.Delay(500);
            int taskNum = runningTasks.FindIndex(task => task.id == id);

            while (taskNum == -1)
            {
                taskNum = runningTasks.FindIndex(task => task.id == id);
            }

            DBUTaskObj task = runningTasks.ElementAt(taskNum);
            System.Console.WriteLine("Sucessfully Initiated Text Listener!", Program.channelId.botCommandsId);
            uint hourlyTracker = 0;
            uint TwoMinuteTracker = 0;
            for (uint i = 0;i < duration;i++)
            {
                if (runningTasks[taskNum].isCancelled)
                {
                    i = duration;
                    break;
                }

                await Task.Delay(1000);

                if (i == 1)
                {
                    System.Console.WriteLine("Text Updater: First pass completed!");
                }

                System.TimeSpan timeSpan = System.DateTime.Now - System.DateTime.Today.AddDays(1).AddSeconds(-1);

                //System.Console.WriteLine(timeSpan.Hours + ":" + timeSpan.Minutes + ":" + timeSpan.Seconds);

                //midnight trigger
                if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 0)
                {
                    if (planetsLost >= 15)
                    {
                        await OutprintAsync("https://tenor.com/view/correr-despavoridos-enchufe-tv-huir-invasion-extraterrestre-corran-por-sus-vidas-gif-24995288", Program.channelId.slaversId);
                    }
                    if (((int)planetsKaptured - (int)planetsLost) >= 15)
                    {
                        await OutprintAsync("https://tenor.com/view/cat-shooting-mouth-open-gif-15017033", Program.channelId.slaversId);
                    }
                    if (colsBuilt >= 10)
                    {
                        await OutprintAsync("https://tenor.com/view/construct-construction-nail-and-hammer-build-worker-gif-13899535", Program.channelId.slaversId);
                    }
                    if (enemiesSlain >= 10)
                    {
                        await OutprintAsync("https://tenor.com/view/starwars-the-force-awakens-xwing-air-combat-gif-4813295", Program.channelId.slaversId);
                    }
                    if (alliesSlain >= 10)
                    {
                        await OutprintAsync("https://tenor.com/view/press-f-pay-respect-coffin-burial-gif-12855021", Program.channelId.slaversId);
                    }

                    await OutprintAsync(
                        "@everyone Daily Report: " + '\n'
                        + "We Lawst: " + planetsLost + '\n'
                        + "We Kaptured: " + planetsKaptured + '\n'
                        + "Allies Slain: " + alliesSlain + '\n'
                        + "Enemies Slain: " + enemiesSlain + '\n'
                        + "Landings: " + landings + '\n'
                        + "Colonies Abanonded: " + colsAbandoned + " (They just went out for milk and cigarettes)" + '\n'
                        + "Colonies Built: " + colsBuilt, Program.channelId.slaversId);

                    planetsKaptured = 0;
                    planetsLost = 0;
                    alliesSlain = 0;
                    enemiesSlain = 0;
                    landings = 0;
                    colsAbandoned = 0;
                    colsBuilt = 0;

                    await FindListAsync("All");
                    await FindListAsync("Military");
                    await FindListAsync("PollutionCrit");
                    await FindListAsync("Solar Off");
                    await FindListAsync("Zoundsables");

                    //await FindListAsync("DD");
                }

                if (TwoMinuteTracker == 120)
                {
                    _ = Task.Run(() => RunThroughTextAsync());
                    TwoMinuteTracker = 0;
                }
                if (hourlyTracker == 3600)
                {
                    _ = OutprintAsync("$run info " + planetsLost + " " + planetsKaptured + " " + alliesSlain + " " + enemiesSlain + " " + landings + " " + colsAbandoned + " " + colsBuilt,
                        Program.channelId.botUpdatesId);
                    System.Console.WriteLine("Running hourly Redomes!");
                    _ = Task.Run(() => FindHourlyRedomesAsync());
                    hourlyTracker = 0;
                }

                TwoMinuteTracker++;
                hourlyTracker++;
                task.ticker++;
            }

            await OutprintAsync("No Longer Listening for Text Updates!", Program.channelId.botCommandsId);
            runningTasks.RemoveAt(taskNum);
            dbuTaskNum--;
        }

        internal string AllPlanetInfo(Holding planet)
        {
            return planet.Location + " (" + planet.GalaxyX + "," + planet.GalaxyY + ")" + " | " + planet.Name + " | " + planet.PlanetType + " | " + planet.Owner + '\n'
                + "Population: " + planet.Population + " + " + planet.PopGrowth + "/hour" + " | Morale: " + planet.Morale + " + " + planet.MoraleChange + "/hour" + '\n'
                + "  Disasters: " + planet.Disaster + " | Pollution: " + planet.Pollution + " + " + planet.PollutionRate + " / day" + '\n'
                + "  Discoveries: " + planet.Discoveries + "/ " + planet.NumDiscoveries + " Total Discoveries | Building: " + planet.CurrentlyBuilding + " | Solar: " + planet.SolarShots + " / " + planet.SolarFreq + '\n'
                + "Nukes: " + planet.Nukes + " | Negotiators: " + planet.Negotiators + " | Compound Mines: " + planet.CompoundMines + " | Lasers: " + planet.LaserCannons + " | Shields: " + planet.Shields + '\n'
                + "/setworkforces " + planet.PercConstruct + " " + planet.PercResearch + " " + planet.PercMilitary + " " + planet.PercHarvest + '\n'
                + "Resources: " + '\n'
                + "  Metal: " + planet.Ore + " | Anaerobes: " + planet.Ana + " | Medicine: " + planet.Med + '\n'
                + "  Organics: " + planet.Org + " | Oil: " + planet.Oil + " | Uranium: " + planet.Ura + '\n'
                + "  Equipment: " + planet.Equ + " | Spice: " + planet.Spi +
                +'\n' + '\n'
                + " ___________________________________________________________________________"
                + '\n' + '\n';
        }

        internal async Task<string> FindPlanetAsync(string text)
        {
            HttpClient client = new HttpClient();

            client = APICaller.AddRequestHeaders(client,
                Settings.Configuration["API:StarportGE:host"],
                Settings.Configuration["API:StarportGE:keyName"],
                Settings.Configuration["API:StarportGE:key"],
                null);

            return await APICaller.GetResponseBodyFromApiAsync(client,
                  $"{Settings.Configuration["API:StarportGE:url"]}/{Settings.Configuration["API:StarportGE:Get:planetByName"]}name={text}&server={currentServer}",
                  0);
        }

        internal async Task<string> FindSystemAsync(string text)
        {
            HttpClient client = new HttpClient();

            client = APICaller.AddRequestHeaders(client,
                Settings.Configuration["API:StarportGE:host"],
                Settings.Configuration["API:StarportGE:keyName"],
                Settings.Configuration["API:StarportGE:key"],
                null);

            return await APICaller.GetResponseBodyFromApiAsync(client,
                   $"{Settings.Configuration["API:StarportGE:url"]}/{Settings.Configuration["API:StarportGE:Get:systemByName"]}name={text}&server={currentServer}",
                   0);        
        }

        internal async Task FindWeaponsNearMeAsync(string text)
        {
            if (holdingsList == null)
            {
                await LoadExcelHoldingsAsync();
            }
            string tempWeapons = Directory.GetCurrentDirectory() + "/TempWeaponsDir/weaponsOfMassDestruction.txt";

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/TempWeaponsDir"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/TempWeaponsDir");
            }
            await File.WriteAllTextAsync(tempWeapons, "");

            holdingsList.OrderBy(hops => hops.HopsAway);

            foreach (Holding planet in holdingsList)
            {
                if (text == "Nukes" && (planet.Nukes > 0 || planet.Negotiators > 0) && planet.HopsAway <= 5)
                {
                    string message = planet.Location + " | " + planet.Name + " | Hops Away: " + planet.HopsAway + " | Nukes: " + planet.Nukes + " | Negotiators: " + planet.Negotiators + '\n';
                    await File.AppendAllTextAsync(tempWeapons, message);
                }
                if (text == "Defenses" && (planet.LaserCannons > 0 || planet.CompoundMines > 0 || planet.FlakCannons > 0) && planet.HopsAway <= 5)
                {
                    string message = planet.Location + " | " + planet.Name + " | Hops Away: " + planet.HopsAway + " | Lasers: " + planet.LaserCannons + " | Compound Mines: " + planet.CompoundMines + " Flaks: " + planet.FlakCannons + '\n';
                    await File.AppendAllTextAsync(tempWeapons, message);
                }
                if (text == "Shields" && planet.Shields > 0)
                {
                    string message = planet.Location + " | " + planet.Name + " | Hops Away: " + planet.HopsAway + " Shields: " + planet.Shields;
                    await File.AppendAllTextAsync(tempWeapons, message);
                }
            }

            await OutprintFileAsync(tempWeapons, Program.channelId.slaversId);
            Algorithms.FileManipulation.DeleteFile(tempWeapons);

            Algorithms.FileManipulation.DeleteDirectory(Directory.GetCurrentDirectory() + "/TempWeaponsDir", true);
            Console.WriteLine("DELETED!" + Directory.GetCurrentDirectory() + "/TempWeaponsDir");
        }

        internal async Task ReadPlanetPicturesAndInfoFoldersAsync()
        {
            string picturesDir = Program.filePaths.networkPathDir + "/Pictures";
            if (Directory.Exists(Program.filePaths.picturesAndInfo))
            {
                string[] dirs = Directory.GetDirectories(Program.filePaths.picturesAndInfo, "*", SearchOption.AllDirectories);

                foreach (string dir in dirs)
                {
                    DirectoryInfo directoryInfo = new DirectoryInfo(dir);

                    if (directoryInfo.Name == "our_colonies")
                    {
                        string[] pictures = Directory.GetFiles(dir);

                        foreach (string picture in pictures)
                        {
                            string newName = Path.GetFileName(picture);
                            newName = Algorithms.StringManipulation.RemoveDuplicates(newName);

                            //File.Move(picture, dir + "/" + newName);
                            if (!File.Exists(Program.filePaths.planetPicturesDir + "/" + newName))
                            {
                                File.Copy(picture, picturesDir + "/Planet-Pictures/" + newName);
                            }
                        }
                    }
                    else if (directoryInfo.Name == "enemy_colonies")
                    {
                        string[] pictures = Directory.GetFiles(dir);

                        foreach (string picture in pictures)
                        {
                            string newName = Path.GetFileName(picture);
                            newName = Algorithms.StringManipulation.RemoveDuplicates(newName);

                            //File.Move(picture, dir + "/" + newName);
                            if (!File.Exists(Program.filePaths.enemyPlanetsDir + "/" + newName))
                            {
                                File.Copy(picture, Program.filePaths.enemyPlanetsDir + "/" + newName);
                            }
                            else
                            {
                                Algorithms.FileManipulation.DeleteFile(Program.filePaths.enemyPlanetsDir + "/" + newName);
                                File.Copy(picture, Program.filePaths.enemyPlanetsDir + "/" + newName);
                            }
                        }
                    }
                    else if (directoryInfo.Name == "open_to_build")
                    {
                        string[] pictures = Directory.GetFiles(dir);

                        foreach (string picture in pictures)
                        {
                            string newName = Path.GetFileName(picture);
                            newName = Algorithms.StringManipulation.RemoveDuplicates(newName);

                            //File.Move(picture, dir + "/" + newName);
                            if (!File.Exists(Program.filePaths.undomedDir + "/" + newName))
                            {
                                File.Copy(picture, Program.filePaths.undomedDir + "/" + newName);
                            }
                            else
                            {
                                Algorithms.FileManipulation.DeleteFile(Program.filePaths.undomedDir + "/" + newName);
                                File.Copy(picture, Program.filePaths.undomedDir + "/" + newName);
                            }
                        }
                    }
                }
            }
            await OutprintAsync("Read Folders Completed!", Program.channelId.botUpdatesId);
        }

        internal async Task SetAlertsAsync(bool v)
        {
            alerts = v;
            await Task.Delay(0);
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

        private async Task ChatLogsReaderAsync(string[] fileStrArr, string chatLogOwner)
        {
            List<Holding> tempHoldingsList;
            if (holdingsList == null)
            {
                await LoadExcelHoldingsAsync();
            }
            tempHoldingsList = holdingsList;

            if (fileStrArr.Length > 0)
            {
                string lastLine = fileStrArr[fileStrArr.Length - 1];
                string secondToLastLine = fileStrArr[fileStrArr.Length - 2];
                string thirdToLastLine = fileStrArr[fileStrArr.Length - 3];

                //These dont really have a category
                if (lastLine.Contains("Landed on ") && lastLine.Contains("world"))
                {
                    lastLand = Algorithms.StringManipulation.GetBetween(lastLine, "on", ",");
                    lastLand = lastLand.Replace("world ", "");
                }
                else if (lastLine.Contains("Warped to"))
                {
                    lastSystem = Algorithms.StringManipulation.GetBetween(lastLine, "to", ",");
                }
                else if (lastLine.Contains("Taking you directly"))
                {
                    lastLand = Algorithms.StringManipulation.GetBetween(secondToLastLine, "on", ",");
                    lastLand = lastLand.Replace("world ", "");
                }
                else if (lastLine.Contains("(automated) picked up"))
                {
                    await OutprintAsync(lastLine, Program.channelId.botTakingResourcesId);
                }
                else if (lastLine.Contains("tons of unidentified compounds"))
                {
                    if (lastLine.Contains("contains 0 tons of unidentified compounds") && !lastLine.Contains("System"))
                    {
                        await OutprintAsync("Undomed: " + lastLine, Program.channelId.buildingId);
                    }
                    else if (lastLine.Contains("contains") && !lastLine.Contains("System"))
                    {
                        await OutprintAsync(lastLine, Program.channelId.nuetrinoId);
                    }
                }
                else if (lastLine.Contains("Fellow corporation member"))
                {
                    await OutprintAsync(AtUser(lastLine) + lastLine, Program.channelId.newsId);
                }
                else if (lastLine.Contains("Exporting holdings.csv"))
                {
                    string csv = "";
                    if (File.Exists(Program.filePaths.csvLocal))
                    {
                        csv = Program.filePaths.csvLocal;
                        Algorithms.FileManipulation.DeleteFile(Program.filePaths.csvPath);
                        File.Copy(csv, Program.filePaths.csvPath); //Copy local to internet
                    }

                    System.Console.WriteLine("Copied csv to internet...");
                    //await OutprintAsync("Copied local csv to internet", Program.channelId.botUpdatesId);

                    await Task.Delay(5000);
                    await Task.Run(() => LoadExcelHoldingsAsync());
                }
                else if (lastLine.Contains("Connection to server lost due to"))
                {
                    await OutprintAsync(AtUser("Autism") + "I lost connection!", Program.channelId.newsId);
                    await OutprintAsync(AtUser("Autism") + "I lost connection!", Program.channelId.botUpdatesId);
                }

                if (kombat)
                {
                    //warped in
                    if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && (lastLine.Contains("warped into") || lastLine.Contains("entered the system")))
                    {
                        List<string> enemiesList = Diplomacy.enemies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync("@everyone " + chatLogOwner + ": " + lastLine + " in " + lastSystem, Program.channelId.enemySightingsId);
                        await SayAsync(enemy + " spotted! By " + chatLogOwner + " in " + lastSystem, Program.channelId.voiceSlaversOnlyId);
                    }
                    else if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("warped out"))
                    {
                        List<string> enemiesList = Diplomacy.allies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync("@everyone " + lastLine + " in " + lastSystem, Program.channelId.enemySightingsId);
                        await SayAsync(enemy + " ran away cause he's a bitch nigga", Program.channelId.voiceSlaversOnlyId);
                    }
                    else if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("landed on a planet"))
                    {
                        List<string> enemiesList = Diplomacy.allies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync(lastLine + " in " + lastSystem, Program.channelId.enemySightingsId);
                        await SayAsync(enemy + " landed!", Program.channelId.voiceSlaversOnlyId);
                    }
                    else if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("docked"))
                    {
                        List<string> enemiesList = Diplomacy.allies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync(lastLine + " in " + lastSystem, Program.channelId.enemySightingsId);
                        await SayAsync(enemy + " Re-Shielded!", Program.channelId.voiceSlaversOnlyId);
                    }

                    //shot downs
                    if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("shot down") && !lastLine.Contains("shouts"))
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
                                await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + lastLine, Program.channelId.newsId);
                                //await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + lastLine, Program.channelId.alliedChatId);
                                enemiesSlain++;
                            }
                            else if (!string.IsNullOrEmpty(ally) && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + lastLine, Program.channelId.newsId);
                                //await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + lastLine, Program.channelId.alliedChatId);
                            }
                            else
                            {
                                await OutprintAsync(lastLine, Program.channelId.newsId);
                                //await OutprintAsync(lastLine, Program.channelId.alliedChatId);
                            }

                            await SayAsync(enemy + " Has Been Slain!", Program.channelId.voiceSlaversOnlyId);
                        }
                        else if (lastLine.Contains("For blowing up"))
                        {
                            enemiesSlain++;
                            if (secondToLastLine.Contains("Defenses") && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + secondToLastLine, Program.channelId.newsId);
                                //await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + secondToLastLine, Program.channelId.alliedChatId);
                                enemiesSlain++;
                            }
                            else if (!string.IsNullOrEmpty(ally) && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + secondToLastLine, Program.channelId.newsId);
                                //await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + secondToLastLine, Program.channelId.alliedChatId);
                            }
                            else
                            {
                                await OutprintAsync(secondToLastLine, Program.channelId.newsId);
                                //await OutprintAsync(secondToLastLine, Program.channelId.alliedChatId);
                            }

                            await SayAsync(enemy + " Has Been Slain!", Program.channelId.voiceSlaversOnlyId);
                        }
                        else if (lastLine.Contains("You recover"))
                        {
                            enemiesSlain++;
                            if (thirdToLastLine.Contains("Defenses") && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + thirdToLastLine, Program.channelId.newsId);
                                //await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + secondToLastLine, Program.channelId.alliedChatId);
                                enemiesSlain++;
                            }
                            else if (!string.IsNullOrEmpty(ally) && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + thirdToLastLine, Program.channelId.newsId);
                                //await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + secondToLastLine, Program.channelId.alliedChatId);
                            }
                            else
                            {
                                await OutprintAsync(thirdToLastLine, Program.channelId.newsId);
                                //await OutprintAsync(secondToLastLine, Program.channelId.alliedChatId);
                            }

                            await SayAsync(enemy + " Has Been Slain!", Program.channelId.voiceSlaversOnlyId);
                        }
                        else if (lastLine.Contains("shot down " + ally) && !ally.Equals(" "))
                        {
                            alliesSlain++;
                            await OutprintAsync(lastLine + " Help " + ally + " Nigga. Damn!", Program.channelId.newsId);
                            //await OutprintAsync(lastLine + " Help " + ally + " Nigga. Damn!", Program.channelId.alliedChatId);
                            await SayAsync(ally + " Has Been Slain!", Program.channelId.voiceSlaversOnlyId);
                        }
                    }

                    //invasions
                    if (lastLine.Contains("was invaded and taken"))
                    {
                        await OutprintAsync(AtUser(lastLine) + lastLine, Program.channelId.distressCallsId);
                        await OutprintAsync(AtUser(lastLine) + lastLine, Program.channelId.recapListId);
                        string planetName = Algorithms.StringManipulation.GetBetween(lastLine, "on", "(");
                        int holdingsIndex = tempHoldingsList.FindIndex(planet => planet.Location == planetName);

                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/crying-meme-black-guy-cries-sad-man-thank-god-for-my-reefer-hood-news-gif-24902056 " + '\n' +
                                "WE Lost Double Dome!", Program.channelId.slaversId);
                            await OutprintAsync("https://tenor.com/view/crying-meme-black-guy-cries-sad-man-thank-god-for-my-reefer-hood-news-gif-24902056 " + '\n' +
                                "WE Lost Double Dome!", Program.channelId.newsId);
                        }
                        await SayAsync("We've Lost a Command Post!", Program.channelId.voiceSlaversOnlyId);

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
                                await OutprintAsync(experience + '\n' + e.ToString(), Program.channelId.botUpdatesId);
                            }

                            if (exp > 50000)
                            {
                                planetsLost++;
                            }
                        }

                        if (holdingsIndex != -1)
                        {
                            await OutprintAsync(
                                "Metal Ore: " + tempHoldingsList[holdingsIndex].Ore + " | Solar: " + tempHoldingsList[holdingsIndex].SolarShots + " / " + tempHoldingsList[holdingsIndex].SolarFreq + '\n' +
                                " | Nukes: " + tempHoldingsList[holdingsIndex].Nukes + " | Negotiators: " + tempHoldingsList[holdingsIndex].Negotiators + " | Compound Mines: " + tempHoldingsList[holdingsIndex].CompoundMines + " | Lasers: " + tempHoldingsList[holdingsIndex].LaserCannons + '\n' +
                                " | Population: " + tempHoldingsList[holdingsIndex].Population + " | Discoveries: " + tempHoldingsList[holdingsIndex].Discoveries, Program.channelId.distressCallsId);
                        }
                    }
                    else if (lastLine.Contains("captured the colony"))
                    {
                        await OutprintAsync(lastLine, Program.channelId.newsId);
                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                               "WE CAPTURED A DOUBLE DOME!", Program.channelId.slaversId);
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                               "WE CAPTURED A DOUBLE DOME!", Program.channelId.newsId);
                        }
                        //await OutprintAsync(lastLine, Program.channelId.alliedChatId);
                        await SayAsync("We've Captured a Command Post!", Program.channelId.voiceSlaversOnlyId);
                        planetsKaptured++;
                    }
                    else if (lastLine.Contains("You claim ownership of the colony owned by"))
                    {
                        await OutprintAsync(secondToLastLine, Program.channelId.newsId);
                        //await OutprintAsync(secondToLastLine, Program.channelId.alliedChatId);
                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", Program.channelId.slaversId);
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", Program.channelId.newsId);
                        }
                        await SayAsync("We've Captured a Command Post!", Program.channelId.voiceSlaversOnlyId);
                        planetsKaptured++;
                    }
                    else if (lastLine.Contains("It now belongs to"))
                    {
                        await OutprintAsync(fileStrArr[fileStrArr.Length - 3], Program.channelId.newsId);
                        //await OutprintAsync(fileStrArr[fileStrArr.Length - 3], Program.channelId.alliedChatId);
                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", Program.channelId.slaversId);
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", Program.channelId.newsId);
                        }
                        await SayAsync("We've Captured a Command Post!", Program.channelId.voiceSlaversOnlyId);
                        planetsKaptured++;
                    }
                    else if (lastLine.Contains("For successful invasion"))
                    {
                        await OutprintAsync(fileStrArr[fileStrArr.Length - 4], Program.channelId.newsId);
                        //await OutprintAsync(fileStrArr[fileStrArr.Length - 4], Program.channelId.alliedChatId);
                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", Program.channelId.slaversId);
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", Program.channelId.newsId);
                        }
                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", Program.channelId.newsId);
                        }
                        await SayAsync("We've Captured a Command Post!", Program.channelId.voiceSlaversOnlyId);
                        planetsKaptured++;
                    }
                }

                if (distress)
                {
                    if (lastLine.Contains("*** Distress"))
                    {
                        landings++;
                        string colonyName = Algorithms.StringManipulation.GetBetween(lastLine, "from", "on");
                        string planetName = Algorithms.StringManipulation.GetBetween(lastLine, "on", "(");
                        int holdingsIndex = tempHoldingsList.FindIndex(planet => planet.Location == planetName);

                        if (colonyName.Contains("(") && colonyName.Contains("."))
                        {
                            colonyName = Algorithms.StringManipulation.GetBetween(colonyName, ")", ".");
                        }

                        //System.Console.WriteLine("colonyName: " + colonyName);
                        string colonyPath = Program.filePaths.planetPicturesDir + "/" + colonyName + ".png";
                        string planetPath = null;

                        string json = await FindPlanetAsync(planetName);

                        if (json == null)
                        {
                            await OutprintAsync($"There was a problem fetching {planetName}'s picture", Program.channelId.distressCallsId);
                        }
                        else
                        {
                            Planet planet = JsonConvert.DeserializeObject<Planet>(json);

                            if(planet.Picture == null)
                            {
                                await OutprintAsync($"{planetName}'s picture does not exist!", Program.channelId.distressCallsId);
                            }
                            else
                            {
                                string tempFile = Path.GetTempFileName().Replace(".tmp", planet.Picture.FileExtension);
                                File.WriteAllBytes(tempFile, planet.Picture.FileBytes);
                                planetPath = tempFile;
                            }

                        }


                        if (File.Exists(colonyPath))
                        {
                            await OutprintAsync("@everyone " + lastLine, Program.channelId.distressCallsId);
                            await OutprintFileAsync(colonyPath, Program.channelId.distressCallsId);
                        }
                        else if (File.Exists(planetPath))
                        {
                            await OutprintAsync("@everyone " + lastLine, Program.channelId.distressCallsId);
                            await OutprintFileAsync(planetPath, Program.channelId.distressCallsId);
                            File.Delete(planetPath);
                        }
                        else
                        {
                            await OutprintAsync("@everyone " + lastLine, Program.channelId.distressCallsId);
                        }

                        if (tempHoldingsList == null)
                        {
                            await LoadExcelHoldingsAsync();
                        }

                        if (holdingsIndex != -1)
                        {
                            await OutprintAsync(
                                "Metal Ore: " + tempHoldingsList[holdingsIndex].Ore + " | Solar: " + tempHoldingsList[holdingsIndex].SolarShots + " / " + tempHoldingsList[holdingsIndex].SolarFreq + '\n' +
                                " | Nukes: " + tempHoldingsList[holdingsIndex].Nukes + " | Negotiators: " + tempHoldingsList[holdingsIndex].Negotiators + " | Compound Mines: " + tempHoldingsList[holdingsIndex].CompoundMines + " | Lasers: " + tempHoldingsList[holdingsIndex].LaserCannons + '\n' +
                                " | Population: " + tempHoldingsList[holdingsIndex].Population + " | Discoveries: " + tempHoldingsList[holdingsIndex].Discoveries, Program.channelId.distressCallsId);
                        }
                        else
                        {
                            await OutprintAsync("Couldnt find " + planetName + " in the spreadsheet!", Program.channelId.botErrorsId);
                        }
                    }
                    else if (lastLine.Contains("***") && lastLine.Contains("landed"))
                    {
                        landings++;
                        await OutprintAsync(lastLine, Program.channelId.distressCallsId);
                    }
                }
                if (alerts)
                {
                    if (lastLine.Contains("Server Alert!"))
                    {
                        if (secondToLastLine.Contains("Server Alert!"))
                        {
                            await OutprintAsync("@everyone " + secondToLastLine, Program.channelId.slaversId);
                        }
                        await OutprintAsync("@everyone " + lastLine, Program.channelId.slaversId);
                        //await OutprintAsync("@everyone " + lastLine, Program.channelId.alliedChatId);
                    }
                    else if (lastLine.Contains("U.N. Hotline"))
                    {
                        List<string> alliesList = Diplomacy.allies.ToList<string>();
                        string ally = alliesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync(lastLine, Program.channelId.newsId);
                        await SayAsync(ally + " I see you!", Program.channelId.voiceSlaversOnlyId);
                    }
                    else if (lastLine.Contains("Empress Allie says to Slavers"))
                    {
                        await OutprintAsync(lastLine, Program.channelId.botUpdatesId);
                    }
                    else if (lastLine.Contains("due to pollution"))
                    {
                        await OutprintAsync(AtUser(lastLine) + lastLine, Program.channelId.newsId);
                        await SayAsync(AtUser(lastLine) + " your colony had a disasta", Program.channelId.voiceSlaversOnlyId);
                    }
                }
                if (building)
                {
                    string[] str = { "]", ")", "_", "." };
                    //col died
                    if (lastLine.Contains("was finally abandoned"))
                    {
                        System.TimeSpan days3TimeSpan = new System.TimeSpan(0, 72, 0, 0);
                        System.DateTime days3 = System.DateTime.Now + days3TimeSpan;

                        string title = Algorithms.StringManipulation.GetBetween(lastLine, "colony", "was");

                        await CreateCalendarEventAsync(days3, title, lastLine, Program.channelId.buildingId);
                        colsAbandoned++;
                    }
                    //aa
                    else if (lastLine.Contains("Advanced Architecture lvl"))
                    {
                        string planetName = Algorithms.StringManipulation.GetBetween(lastLine, " on ", "discovered");
                        string discovery = Algorithms.StringManipulation.GetBetween(lastLine, "the secret of", ".");
                        discovery = discovery.Replace("Advanced Architecture", "Arch");
                        if (planetName.Contains("["))
                        {
                            planetName = Algorithms.StringManipulation.GetBetween(planetName, " on ", "[");
                        }
                        else
                        {
                            planetName = Algorithms.StringManipulation.GetBetween(planetName, " on ", "(");
                        }

                        int holdingsIndex = tempHoldingsList.FindIndex(planet => planet.Location == planetName);
                        if (holdingsIndex != -1)
                        {
                            Holding planet = tempHoldingsList[holdingsIndex];
                            if (StarportHelperClasses.Helper.IsZoundsable(planet.PlanetType, discovery))
                            {
                                string message = AllPlanetInfo(planet);

                                await OutprintAsync(AtUser(planet.Owner) + lastLine + " Zounds dat hoe now!" + '\n' + message, Program.channelId.zoundsForHoundsId);
                            }
                        }
                        else
                        {
                            holdingsIndex = tempHoldingsList.FindIndex(planet => planet.Location == planetName);
                            if (holdingsIndex != -1)
                            {
                                Holding planet = tempHoldingsList[holdingsIndex];
                                if (StarportHelperClasses.Helper.IsZoundsable(planet.PlanetType, discovery))
                                {
                                    string message = AllPlanetInfo(planet);

                                    await OutprintAsync(AtUser(planet.Owner) + lastLine + " Zounds dat hoe now!" + '\n' + message, Program.channelId.zoundsForHoundsId);
                                }
                            }
                            else
                            {
                                await OutprintAsync(lastLine + '\n' + planetName + " got Adv Arch, but i couldn't find " + planetName + " in holdings!", Program.channelId.botUpdatesId);
                            }
                        }
                    }
                    else if (lastLine.Contains("Military Tradition lvl 3") || lastLine.Contains("Military Tradition lvl 4") || lastLine.Contains("Military Tradition lvl 5"))
                    {
                        await OutprintAsync(AtUser(lastLine) + lastLine + " Decent Huge Metro col", Program.channelId.buildingId);
                    }
                    //if bio3
                    else if (lastLine.Contains("completed work on the Biodome Level 3."))
                    {
                        await OutprintAsync(AtUser(lastLine) + lastLine, Program.channelId.buildingId);
                        colsBuilt++;
                    }

                    //Domed new colony && dd
                    if (lastLine.Contains("founding"))
                    {
                        await OutprintAsync(chatLogOwner + " colonized " + lastLand + "!", Program.channelId.buildingId);
                        await SayAsync(chatLogOwner + " colonized a new world!", Program.channelId.voiceBuildingId);
                    }
                    else if (lastLine.Contains("adding another dome"))
                    {
                        await OutprintAsync(chatLogOwner + " Created a New Double Dome on " + lastLand + "!", Program.channelId.buildingId);
                        await OutprintAsync("https://tenor.com/view/cat-shooting-mouth-open-gif-15017033", Program.channelId.buildingId);
                        await SayAsync(chatLogOwner + " Created a New Double Dome on " + lastLand + "!", Program.channelId.voiceSlaversOnlyId);
                        //await CelebrateUser("Slavers", "I'd like to see them try and take this!", Program.channelId.slaversId);
                    }
                }
            }
        }

        private async void OnChatChangedAsync(object sender, FileSystemEventArgs fileSysEvent)
        {
            string filePath = fileSysEvent.FullPath;
            string[] fileStrArr = new string[0];

            string serversJson = File.ReadAllText(Directory.GetCurrentDirectory() + "/servers.json");
            ServerDictionary serverDict = JsonConvert.DeserializeObject<ServerDictionary>(serversJson);
            foreach (KeyValuePair<string,string> kvp in serverDict.Servers)
            {
                if (filePath.Contains(kvp.Key))
                {
                    currentServer = kvp.Value;
                }
            }         
            
            string[] split = Path.GetFileName(filePath).Split(" ");
            string chatLogOwner = split[0];

            try
            {
                fileStrArr = await File.ReadAllLinesAsync(filePath);
            }
            catch (System.Exception ex)
            {
                await OutprintAsync(ex.ToString(), Program.channelId.botErrorsId);
            }

            _ = ChatLogsReaderAsync(fileStrArr, chatLogOwner);
        }

        private async Task RunThroughTextAsync()
        {
            if (Directory.Exists(Program.filePaths.networkPathDir + "/Channel"))
            {
                string[] filePaths = Directory.GetFiles(Program.filePaths.networkPathDir + "/Channel");

                for (int i = 0;i < filePaths.Length;i++)
                {
                    if (File.Exists(filePaths[i]))
                    {
                        string[] fileAsArr = await File.ReadAllLinesAsync(filePaths[i], default);

                        if (fileAsArr != null)
                        {
                            if (fileAsArr.Length >= 1 && fileAsArr[i] != " " && fileAsArr[i] != "")
                            {
                                if (Path.GetFileName(filePaths[i]).Equals("botUpdates.txt"))
                                {
                                    await OutprintAsync(fileAsArr, Program.channelId.botUpdatesId);

                                    await File.WriteAllTextAsync(filePaths[i], " "); //now clear it out
                                    await OutprintAsync(filePaths[i] + " cleared!", Program.channelId.botUpdatesId);
                                    System.Console.WriteLine(filePaths[i] + " cleared!");
                                }
                                else if (Path.GetFileName(filePaths[i]).Equals("building.txt"))
                                {
                                    await OutprintAsync(fileAsArr, Program.channelId.buildingId);

                                    await File.WriteAllTextAsync(filePaths[i], " "); //now clear it out
                                    await OutprintAsync(filePaths[i] + " cleared!", Program.channelId.botUpdatesId);
                                    System.Console.WriteLine(filePaths[i] + " cleared!");
                                }
                                else if (Path.GetFileName(filePaths[i]).Equals("distress.txt"))
                                {
                                    await OutprintAsync(fileAsArr, Program.channelId.distressCallsId);

                                    await File.WriteAllTextAsync(filePaths[i], " "); //now clear it out
                                    await OutprintAsync(filePaths[i] + " cleared!", Program.channelId.botUpdatesId);
                                    System.Console.WriteLine(filePaths[i] + " cleared!");
                                }
                                else if (Path.GetFileName(filePaths[i]).Equals("scoutReports.txt"))
                                {
                                    await OutprintAsync(fileAsArr, Program.channelId.scoutReportsId);

                                    await File.WriteAllTextAsync(filePaths[i], " "); //now clear it out
                                    await OutprintAsync(filePaths[i] + " cleared!", Program.channelId.botUpdatesId);
                                    System.Console.WriteLine(filePaths[i] + " cleared!");
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task UpdateCompanionFilesAsync(Holding planet, string path, string message, string type)
        {
            await File.AppendAllTextAsync(path, message);

            string fileName = AtUser(planet.Owner);
            if (AtUser(planet.Owner) != "")
            {
                string[] remove = { ">", "<", "@" };
                foreach (string str in remove)
                {
                    fileName = fileName.Replace(str, "");
                }

                await File.AppendAllTextAsync(Directory.GetCurrentDirectory() + "/Temp" + type + "Dir/" + fileName + ".txt", message);
            }
        }
    }
}