//Created by Alexander Fields https://github.com/roku674

using StarportObjects;
using DiscordBotUpdates.Objects;
using ExcelCSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace DiscordBotUpdates.Modules
{
    internal class TaskInitator : DBUTask
    {
        public static readonly string csvPath = "G:/My Drive/Personal Stuff/Starport/holdings.csv";
        public static readonly string excelPath = "G:/My Drive/Personal Stuff/Starport/holdings.xlsx";

        /// <summary>
        /// trails with a backslash
        /// </summary>
        public static readonly string planetPicturesDir = "G:/My Drive/Personal Stuff/Starport/PlanetPictures/";

        public static bool alerts { get; set; }
        public static uint alliesSlain { get; set; }
        public static bool building { get; set; }
        public static uint colsAbandoned { get; set; }
        public static uint colsBuilt { get; set; }
        public static bool distress { get; set; }
        public static uint enemiesSlain { get; set; }
        public static List<Holding> holdingsList { get; set; }
        public static bool kombat { get; set; }
        public static uint landings { get; set; }
        public static string lastLand { get; set; }
        public static string lastSystem { get; set; }
        public static uint planetsKaptured { get; set; }
        public static uint planetsLost { get; set; }

        /// </summary>
        /// <summary>
        /// Call this to start the Distress Calls Listener
        /// </summary>
        /// <returns></returns>
        public async Task ChatLogListenerAsync(uint id, ulong channelId, string owner)
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

                for (uint i = 0; i < duration; i++)
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

        public async Task FindHourlyRedomesAsync()
        {
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

                    await OutprintAsync(atUser + "Upcoming Redome Time: " + calendarEvent.OriginalStartTime.DateTime + "EST" + '\n'
                        + calendarEvent.Summary + '\n'
                        + calendarEvent.Description,
                        ChannelID.redomeId
                        );
                }
            }
        }

        /// <summary>
        /// Call this to start the picture updater
        /// </summary>
        /// <returns></returns>
        public async Task PictureUpdaterAsync(uint id)
        {
            //await channel.SendMessageAsync("Sucessfully Initiated Picture Listener!");
            System.Console.WriteLine("Sucessfully Initiated Picture Listener!");
            await Task.Delay(2000);

            int taskNum = runningTasks.FindIndex(task => task.id == id);

            while (taskNum == -1)
            {
                taskNum = runningTasks.FindIndex(task => task.id == id);
            }

            DBUTaskObj task = runningTasks.ElementAt(taskNum);

            for (uint i = 0; i < duration; i++)
            {
                if (runningTasks[taskNum].isCancelled)
                {
                    i = duration;
                    break;
                }

                await Task.Delay(1000);
                string[] paths =
                {
                    "H:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Pictures",
                    "H:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Pictures/Building",
                    "H:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Pictures/Distress",
                    "H:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Pictures/Planet-Pictures-Enemy",
                    "H:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Pictures/Planet-Pictures-Friendly",
                    "H:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Pictures/Planet-Pictures-Undomed",
                    "H:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Pictures/Scout-Reports",
                    "H:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Pictures/Targets",
                };

                for (int j = 0; j < paths.Length; j++)
                {
                    string[] pictures = Directory.GetFiles(paths[j]);
                    if (pictures.Length > 0)
                    {
                        foreach (string picture in pictures)
                        {
                            System.Console.WriteLine(Path.GetFileName(paths[j]));
                            if (Path.GetFileName(picture) != "desktop.ini")
                            {
                                switch (j)
                                {
                                    case 0:
                                        await OutprintFileAsync(picture, ChannelID.botUpdatesId);
                                        break;

                                    case 1:
                                        await OutprintFileAsync(picture, ChannelID.buildingId);
                                        break;

                                    case 2:
                                        await OutprintFileAsync(picture, ChannelID.distressCallsId);
                                        break;

                                    case 3:
                                        if (Directory.Exists(planetPicturesDir + "Enemy Planets/"))
                                        {
                                            if (picture.Contains("_"))
                                            {
                                                picture.Replace("_", " ");
                                            }
                                            if (!File.Exists(planetPicturesDir + Path.GetFileName(picture)))
                                            {
                                                File.Copy(picture, planetPicturesDir + Path.GetFileName(picture));
                                                await OutprintAsync("Enemy Planet Picture Downloaded!", ChannelID.planetPicturesEnemyId);
                                                await OutprintFileAsync(picture, ChannelID.planetPicturesEnemyId);
                                            }
                                            else
                                            {
                                                File.Delete(planetPicturesDir + Path.GetFileName(picture));
                                                File.Copy(picture, planetPicturesDir + Path.GetFileName(picture));
                                                await OutprintAsync("Enemy Planet Picture Updated/Replaced!", ChannelID.planetPicturesEnemyId);
                                                await OutprintFileAsync(picture, ChannelID.planetPicturesEnemyId);
                                            }
                                        }
                                        else
                                        {
                                            await OutprintAsync(picture + " : was not successfully downloaded!", ChannelID.planetPicturesEnemyId);
                                        }
                                        break;

                                    case 4:

                                        if (Directory.Exists(planetPicturesDir))
                                        {
                                            if (picture.Contains("_"))
                                            {
                                                picture.Replace("_", " ");
                                            }
                                            if (!File.Exists(planetPicturesDir + Path.GetFileName(picture)))
                                            {
                                                File.Copy(picture, planetPicturesDir + Path.GetFileName(picture));
                                                await OutprintAsync("Colony Picture Downloaded!", ChannelID.planetPicturesFriendlyId);
                                                await OutprintFileAsync(picture, ChannelID.planetPicturesFriendlyId);
                                            }
                                            else
                                            {
                                                await OutprintAsync(Path.GetFileName(picture) + " was not downloaded as there was a duplicate!", ChannelID.planetPicturesFriendlyId);
                                                //await OutprintFileAsync(picture, ChannelID.botUpdatesId);
                                            }
                                        }
                                        else
                                        {
                                            await OutprintAsync(picture + " : was not successfully downloaded!", ChannelID.planetPicturesFriendlyId);
                                        }

                                        break;

                                    case 5:
                                        if (Directory.Exists(planetPicturesDir + "Undomed/"))
                                        {
                                            if (picture.Contains("_"))
                                            {
                                                picture.Replace("_", " ");
                                            }
                                            if (!File.Exists(planetPicturesDir + Path.GetFileName(picture)))
                                            {
                                                File.Copy(picture, planetPicturesDir + Path.GetFileName(picture));
                                                await OutprintAsync("Enemy Planet Picture Downloaded!", ChannelID.planetPicturesUndomedId);
                                                await OutprintFileAsync(picture, ChannelID.planetPicturesUndomedId);
                                            }
                                            else
                                            {
                                                File.Delete(planetPicturesDir + Path.GetFileName(picture));
                                                File.Copy(picture, planetPicturesDir + Path.GetFileName(picture));
                                                await OutprintAsync("Enemy Planet Picture Updated/Replaced!", ChannelID.planetPicturesUndomedId);
                                                await OutprintFileAsync(picture, ChannelID.planetPicturesUndomedId);
                                            }
                                        }
                                        else
                                        {
                                            await OutprintAsync(picture + " : was not successfully downloaded!", ChannelID.planetPicturesUndomedId);
                                        }
                                        break;

                                    case 6:
                                        await OutprintFileAsync(picture, ChannelID.scoutReportsId);
                                        break;

                                    case 7:
                                        await OutprintFileAsync(picture, ChannelID.targetsId);
                                        break;

                                    default:
                                        await OutprintFileAsync(picture, ChannelID.botUpdatesId);
                                        break;
                                }
                            }

                            File.Delete(picture);
                        }
                    }
                }

                if (i == 1)
                {
                    System.Console.WriteLine("Picture Updater: First pass completed!");
                }

                task.ticker++;
            }
            await OutprintAsync("No Longer Listening for Pictures Updates!", ChannelID.botCommandsId);
            runningTasks.RemoveAt(taskNum);
            dbuTaskNum--;
        }

        /// <summary>
        /// Call this to start the Message Updater
        /// </summary>
        /// <returns></returns>
        public async Task TextUpdaterAsync(uint id)
        {
            //await Outprint("Sucessfully Initiated " + type + " Listener!", ChannelID.botCommandsId);
            await Task.Delay(500);
            int taskNum = runningTasks.FindIndex(task => task.id == id);

            while (taskNum == -1)
            {
                taskNum = runningTasks.FindIndex(task => task.id == id);
            }

            DBUTaskObj task = runningTasks.ElementAt(taskNum);
            System.Console.WriteLine("Sucessfully Initiated Text Listener!", ChannelID.botCommandsId);
            uint hourlyTracker = 0;
            uint TwoMinuteTracker = 0;
            for (uint i = 0; i < duration; i++)
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
                        await OutprintAsync("https://tenor.com/view/correr-despavoridos-enchufe-tv-huir-invasion-extraterrestre-corran-por-sus-vidas-gif-24995288", ChannelID.slaversId);
                    }
                    if (((int)planetsKaptured - (int)planetsLost) >= 15)
                    {
                        await OutprintAsync("https://tenor.com/view/cat-shooting-mouth-open-gif-15017033", ChannelID.slaversId);
                    }
                    if (colsBuilt >= 10)
                    {
                        await OutprintAsync("https://tenor.com/view/construct-construction-nail-and-hammer-build-worker-gif-13899535", ChannelID.slaversId);
                    }
                    if (enemiesSlain >= 10)
                    {
                        await OutprintAsync("https://tenor.com/view/starwars-the-force-awakens-xwing-air-combat-gif-4813295", ChannelID.slaversId);
                    }
                    if (alliesSlain >= 10)
                    {
                        await OutprintAsync("https://tenor.com/view/press-f-pay-respect-coffin-burial-gif-12855021", ChannelID.slaversId);
                    }

                    await OutprintAsync(
                        "@everyone Daily Report: " + '\n'
                        + "We Lawst: " + planetsLost + '\n'
                        + "We Kaptured: " + planetsKaptured + '\n'
                        + "Allies Slain: " + alliesSlain + '\n'
                        + "Enemies Slain: " + enemiesSlain + '\n'
                        + "Landings: " + landings + '\n'
                        + "Colonies Abanonded: " + colsAbandoned + " (They just went out for milk and cigarettes)" + '\n'
                        + "Colonies Built: " + colsBuilt, ChannelID.slaversId);

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
                    System.Console.WriteLine("Running hourly Redomes!");
                    _ = Task.Run(() => FindHourlyRedomesAsync());
                    hourlyTracker = 0;
                }

                TwoMinuteTracker++;
                hourlyTracker++;
                task.ticker++;
            }

            await OutprintAsync("No Longer Listening for Text Updates!", ChannelID.botCommandsId);
            runningTasks.RemoveAt(taskNum);
            dbuTaskNum--;
        }

        public async Task UpdateAllieTxt(string text)
        {
            string alliePath = Directory.GetCurrentDirectory() + "/Echo/Allie.txt";
            string[] lines = new string[9];

            if (text == "Stop")
            {
                lines[0] = "Stop";
            }
            else
            {
                if (holdingsList == null)
                {
                    await LoadExcelHoldingsAsync();
                }

                lines[0] = "  Command Build " + text;
                Holding planetToBuild = holdingsList.Find(p => p.location == text);
                Holding[] lastPlanet = new Holding[8];
                for (int i = 0; i < holdingsList.Count - 1; i++)
                {
                    if (planetToBuild.galaxyX == holdingsList[i].galaxyX && planetToBuild.galaxyY == holdingsList[i].galaxyY)
                    {
                        Holding planetInSystem = holdingsList[i];

                        //Capitalize first letter
                        string planetType = planetInSystem.planetType.Replace(
                                   planetInSystem.planetType[0].ToString(),
                                   planetInSystem.planetType[0].ToString().ToUpper());

                        if (lastPlanet[0] == null)
                        {
                            for (int j = 0; j < lastPlanet.Length; j++)
                            {
                                lastPlanet[j] = planetInSystem;
                            }
                        }

                        if (planetInSystem.ore > lastPlanet[0].ore)
                        {
                            lines[1] = planetInSystem.location + " Type0 " + planetType;
                            lastPlanet[0] = planetInSystem;
                        }
                        else if (planetToBuild.ore > 3000)
                        {
                            lines[1] = lines[1].Replace(planetType, "");
                        }

                        if (planetInSystem.ana > lastPlanet[1].ana)
                        {
                            lines[2] = planetInSystem.location + " Type1 " + planetType;
                            lastPlanet[1] = planetInSystem;
                        }
                        else if (planetToBuild.ana > 3000)
                        {
                            lines[1] = lines[1].Replace(planetType, "");
                        }

                        if (planetInSystem.med > lastPlanet[2].med)
                        {
                            lines[3] = planetInSystem.location + " Type2 " + planetType;
                            lastPlanet[2] = planetInSystem;
                        }
                        else if (planetToBuild.med > 3000)
                        {
                            lines[1] = lines[1].Replace(planetType, "");
                        }

                        if (planetInSystem.org > lastPlanet[3].org)
                        {
                            lines[4] = planetInSystem.location + " Type3 " + planetType;
                            lastPlanet[3] = planetInSystem;
                        }
                        else if (planetToBuild.org > 3000)
                        {
                            lines[1] = lines[1].Replace(planetType, "");
                        }

                        if (planetInSystem.oil > lastPlanet[4].oil)
                        {
                            lines[5] = planetInSystem.location + " Type4 " + planetType;
                            lastPlanet[4] = planetInSystem;
                        }
                        else if (planetToBuild.oil > 3000)
                        {
                            lines[1] = lines[1].Replace(planetType, "");
                        }

                        if (planetInSystem.ura > lastPlanet[5].ura)
                        {
                            lines[6] = planetInSystem.location + " Type5 " + planetType;
                            lastPlanet[5] = planetInSystem;
                        }
                        else if (planetToBuild.ura > 3000)
                        {
                            lines[1] = lines[1].Replace(planetType, "");
                        }

                        if (planetInSystem.equ > lastPlanet[6].equ)
                        {
                            lines[7] = planetInSystem.location + " Type6 " + planetType;
                            lastPlanet[6] = planetInSystem;
                        }
                        else if (planetToBuild.equ > 3000)
                        {
                            lines[1] = lines[1].Replace(planetType, "");
                        }

                        if (planetInSystem.spi > lastPlanet[7].spi)
                        {
                            lines[8] = planetInSystem.location + " Type7 " + planetType;
                            lastPlanet[7] = planetInSystem;
                        }
                        else if (planetToBuild.spi > 3000)
                        {
                            lines[1] = lines[1].Replace(planetType, "");
                        }
                    }
                }
            }

            await File.WriteAllLinesAsync(alliePath, lines);
            await OutprintAsync(AtUser("Autism"), ChannelID.botUpdatesId);
            await OutprintFileAsync(alliePath, ChannelID.botUpdatesId);
        }

        internal string AllPlanetInfo(Holding planet)
        {
            return planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | " + planet.planetType + " | " + planet.owner + '\n'
                + "Population: " + planet.population + " + " + planet.popGrowth + "/hour" + " | Morale: " + planet.morale + " + " + planet.moraleChange + "/hour" + '\n'
                + "  Disasters: " + planet.disaster + " | Pollution: " + planet.pollution + " + " + planet.pollutionRate + " / day" + '\n'
                + "  Discoveries: " + planet.discoveries + "/ " + planet.numDiscoveries + " Total Discoveries | Building: " + planet.currentlyBuilding + " | Solar: " + planet.solarShots + " / " + planet.solarFreq + '\n'
                + "Nukes: " + planet.nukes + " | Negotiators: " + planet.negotiators + " | Compound Mines: " + planet.compoundMines + " | Lasers: " + planet.laserCannons + " | Shields: " + planet.shields + '\n'
                + "/setworkforces " + planet.percConstruct + " " + planet.percResearch + " " + planet.percMilitary + " " + planet.percHarvest + '\n'
                + "Resources: " + '\n'
                + "  Metal: " + planet.ore + " | Anaerobes: " + planet.ana + " | Medicine: " + planet.med + '\n'
                + "  Organics: " + planet.org + " | Oil: " + planet.oil + " | Uranium: " + planet.ura + '\n'
                + "  Equipment: " + planet.equ + " | Spice: " + planet.spi +
                +'\n' + '\n'
                + " ___________________________________________________________________________"
                + '\n' + '\n';
        }

        internal async Task FindEnemyColoniesAsync(string enemy, string folder)
        {
            //System.Console.WriteLine("Finding " + enemy + " colonies!");
            await OutprintAsync(enemy + "'s planets:", ChannelID.scoutReportsId);

            await File.WriteAllTextAsync(folder + "/enemyCols.txt", " ");
            foreach (string fileName in Directory.EnumerateFiles(folder, "*.txt"))
            {
                //System.Console.WriteLine(fileName);
                string fileContents = File.ReadAllText(fileName);
                if (fileContents.Contains(enemy))
                {
                    await File.AppendAllTextAsync(folder + "/enemyCols.txt", Path.GetFileNameWithoutExtension(fileName) + '\n');
                }
            }
            await OutprintFileAsync(folder + "/enemyCols.txt", ChannelID.scoutReportsId);
            File.Delete(folder + "/enemyCols.txt");
        }

        internal async Task FindListAsync(string type)
        {
            ulong channel = ChannelID.botUpdatesId;
            List<Holding> localHoldingsList = holdingsList;
            if (localHoldingsList == null)
            {
                await LoadExcelHoldingsAsync();
                localHoldingsList = holdingsList;
            }
            string tempPath = Directory.GetCurrentDirectory() + "/Temp" + type + "Dir/" + type + "Corporate.txt";
            string tempDir = Directory.GetCurrentDirectory() + "/Temp" + type + "Dir";
            if (!Directory.Exists(tempDir))
            {
                Directory.CreateDirectory(tempDir);
            }
            await File.WriteAllTextAsync(tempPath, "");
            Holding origin = holdingsList.Find(planet => planet.location.Contains("Beta Doradus"));
            localHoldingsList = StarportHelperClasses.HoldingsSorter.SortByDistance(localHoldingsList, origin);
            int ddCount = 0, negativeGrowth = 0, negativeMorale = 0, polluting = 0, pollutingCrit = 0, solarOff = 0, solarWeak = 0, militaryWeak = 0;

            if (type == "Pollution")
            {
                channel = ChannelID.pollutionFinderId;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.pollution > 0 && planet.pollutionRate > 1)
                    {
                        polluting++;
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Disasters: " + planet.disaster + " | Pollution: " + planet.pollution + " + " + planet.pollutionRate + "/day" + '\n';
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "PollutionCrit")
            {
                channel = ChannelID.pollutionCritId;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.pollution > 40 && planet.pollutionRate > 0)
                    {
                        pollutingCrit++;
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Disasters: " + planet.disaster + " | Pollution: " + planet.pollution + " + " + planet.pollutionRate + "/day" + '\n';
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "Zoundsables")
            {
                channel = ChannelID.zoundsForHoundsId;
                int zoundsableCounter = 0;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.population < 100000 && StarportHelperClasses.Helper.IsZoundsable(planet.planetType, planet.discoveries))
                    {
                        zoundsableCounter++;
                        string message = AllPlanetInfo(planet);

                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                }
                await OutprintAsync("Zoundsables found: " + zoundsableCounter, channel);
            }
            else if (type == "DD")
            {
                channel = ChannelID.ddId;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.name.Contains("DD") ||
                       ((planet.name.EndsWith(".D") || planet.name.EndsWith(".DI") || planet.name.Contains(".ZD")))
                       )
                    {
                        ddCount++;
                        string message = AllPlanetInfo(planet);
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "Solar Off")
            {
                channel = ChannelID.solarOffId;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.solarShots == 0 && planet.population > 1000 || (planet.solarShots > 0 && planet.population > 10000 && planet.ore <= 5000))
                    {
                        solarOff++;
                        string message = AllPlanetInfo(planet);
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "Solar Weak")
            {
                channel = ChannelID.solarWeakId;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.solarShots < 25 && planet.population > 5000)
                    {
                        solarWeak++;
                        string message = AllPlanetInfo(planet);
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "MilitaryTest")
            {
                uint military = 10000;

                channel = ChannelID.botTestingId;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.discoveries.Contains("MT lvl 1"))
                    {
                        military = 7150;
                    }
                    else if (planet.discoveries.Contains("MT lvl 2"))
                    {
                        military = 5600;
                    }
                    else if (planet.discoveries.Contains("MT lvl 3"))
                    {
                        military = 4550;
                    }
                    else if (planet.discoveries.Contains("MT lvl 4"))
                    {
                        military = 3850;
                    }
                    else if (planet.discoveries.Contains("MT lvl 5"))
                    {
                        military = 3350;
                    }

                    if ((planet.percMilitary / 100) * planet.population < ((military * 0.4f) / military))
                    {
                        militaryWeak++;
                        string message = AllPlanetInfo(planet);
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "Revolt")
            {
                channel = ChannelID.revoltFinderId;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.morale < 0 || planet.moraleChange < 0.00d)
                    {
                        negativeMorale++;
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Morale: " + planet.morale + " + " + planet.moraleChange + "/hour" + '\n';
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "Shrinking")
            {
                channel = ChannelID.shrinkingFinderId;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.popGrowth < -1d && planet.popGrowth > -2000)
                    {
                        negativeGrowth++;
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Population: " + planet.population + " | Growth Rate: " + planet.popGrowth + "/hour" + '\n';
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "All")
            {
                channel = ChannelID.colonyManagementId;

                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.popGrowth < -1d && planet.popGrowth > -2000)
                    {
                        negativeGrowth++;
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Population: " + planet.population + " | Growth Rate: " + planet.popGrowth + "/hour" + '\n';
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                    if (planet.morale < 0 || planet.moraleChange < 0.00d)
                    {
                        negativeMorale++;
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Morale: " + planet.morale + " + " + planet.moraleChange + "/hour" + '\n';
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                    if (planet.pollution > 0 || planet.pollutionRate > 1)
                    {
                        polluting++;
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Disasters: " + planet.disaster + " | Pollution: " + planet.pollution + " + " + planet.pollutionRate + "/day" + '\n';
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                }
            }
            else
            {
                await OutprintAsync(type + " was not recognized!", ChannelID.botErrorsId);
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
                    await OutprintAsync(AtUser(file), channel);

                    if (!string.IsNullOrEmpty(File.ReadAllText(file)))
                    {
                        await OutprintFileAsync(file, channel);
                    }
                    else
                    {
                        await OutprintAsync("No" + type + " was found!", channel);
                    }

                    File.Delete(file);
                }
            }
            await OutprintFileAsync(tempPath, channel);
            File.Delete(tempPath);

            //clean up remaining files
            string[] remainingFiles = Directory.GetFiles(tempDir);
            foreach (string file in remainingFiles)
            {
                File.Delete(file);
            }
            Directory.Delete(tempDir);
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

            holdingsList.OrderBy(hops => hops.hopsAway);

            foreach (Holding planet in holdingsList)
            {
                if (text == "Nukes" && (planet.nukes > 0 || planet.negotiators > 0) && planet.hopsAway <= 5)
                {
                    string message = planet.location + " | " + planet.name + " | Hops Away: " + planet.hopsAway + " | Nukes: " + planet.nukes + " | Negotiators: " + planet.negotiators + '\n';
                    await File.AppendAllTextAsync(tempWeapons, message);
                }
                if (text == "Defenses" && (planet.laserCannons > 0 || planet.compoundMines > 0 || planet.flakCannons > 0) && planet.hopsAway <= 5)
                {
                    string message = planet.location + " | " + planet.name + " | Hops Away: " + planet.hopsAway + " | Lasers: " + planet.laserCannons + " | Compound Mines: " + planet.compoundMines + " Flaks: " + planet.flakCannons + '\n';
                    await File.AppendAllTextAsync(tempWeapons, message);
                }
                if (text == "Shields" && planet.shields > 0)
                {
                    string message = planet.location + " | " + planet.name + " | Hops Away: " + planet.hopsAway + " Shields: " + planet.shields;
                    await File.AppendAllTextAsync(tempWeapons, message);
                }
            }

            await OutprintFileAsync(tempWeapons, ChannelID.slaversId);
            File.Delete(tempWeapons);

            Directory.Delete(Directory.GetCurrentDirectory() + "/TempWeaponsDir");
        }

        internal static async Task LoadExcelHoldingsAsync()
        {
            holdingsList = new List<Holding>();

            await Task.Delay(3000);
            Excel.Kill();

            await Excel.ConvertFromCSVtoXLSXAsync(csvPath, excelPath);

            Excel excelHoldings = new Excel(excelPath, 1);
            object[,] excelMatrixObj = excelHoldings.ReadCellRange();
            string[,] excelMatrix = new string[excelHoldings.rowCount + 1, excelHoldings.colCount + 1];

            //im starting this at two becuase column is the title
            for (int i = 2; i < excelHoldings.rowCount + 1; i++)
            {
                for (int j = 1; j <= excelHoldings.colCount; j++)
                {
                    if (excelMatrixObj[i, j] == null)
                    {
                        excelMatrixObj[i, j] = "";
                    }
                    excelMatrix[i, j] = excelMatrixObj[i, j].ToString();

                    int tempInt = 0;
                    double tempDouble = 0;
                    if (int.TryParse(excelMatrixObj[i, j].ToString(), out tempInt))
                    {
                        excelMatrixObj[i, j] = tempInt;
                    }
                    else if (double.TryParse(excelMatrixObj[i, j].ToString(), out tempDouble))
                    {
                        excelMatrixObj[i, j] = tempDouble;
                    }
                    else
                    {
                        excelMatrixObj[i, j] = excelMatrixObj[i, j].ToString();
                    }

                    //System.Console.WriteLine("[" + i + "," + j + "]" + rowArr[i, j].GetType());
                }
                excelMatrixObj[i, 9] = DateTime.MaxValue;
                //System.Console.WriteLine("[" + i + "," + 9 + "]" + rowArr[i, 9].GetType());
                if ((int)excelMatrixObj[i, 46] == 1)
                {
                    excelMatrixObj[i, 46] = true;
                }
                else
                {
                    excelMatrixObj[i, 46] = false;
                }
                //System.Console.WriteLine("[" + i + "," + 46 + "]" + rowArr[i, 46].GetType());

                Holding holdings = new Holding(
                    excelMatrix[i, 1],
                    int.Parse(excelMatrix[i, 2]),
                    excelMatrix[i, 3],
                    excelMatrix[i, 4],
                    int.Parse(excelMatrix[i, 5]),
                    int.Parse(excelMatrix[i, 6]),
                    excelMatrix[i, 7],
                    excelMatrix[i, 8],
                    (DateTime)excelMatrixObj[i, 9],
                    int.Parse(excelMatrix[i, 10]),
                    double.Parse(excelMatrix[i, 11]),
                    double.Parse(excelMatrix[i, 12]),
                    double.Parse(excelMatrix[i, 13]),
                    excelMatrix[i, 14],
                    int.Parse(excelMatrix[i, 15]),
                    double.Parse(excelMatrix[i, 16]),
                    int.Parse(excelMatrix[i, 17]),
                    double.Parse(excelMatrix[i, 18]),
                    int.Parse(excelMatrix[i, 19]),
                    double.Parse(excelMatrix[i, 20]),
                    int.Parse(excelMatrix[i, 21]),
                    int.Parse(excelMatrix[i, 22]),
                    int.Parse(excelMatrix[i, 23]),
                    int.Parse(excelMatrix[i, 24]),
                    excelMatrix[i, 25],
                    int.Parse(excelMatrix[i, 26]),
                    int.Parse(excelMatrix[i, 27]),
                    int.Parse(excelMatrix[i, 28]),
                    int.Parse(excelMatrix[i, 29]),
                    int.Parse(excelMatrix[i, 40]),
                    int.Parse(excelMatrix[i, 31]),
                    int.Parse(excelMatrix[i, 32]),
                    int.Parse(excelMatrix[i, 33]),
                    int.Parse(excelMatrix[i, 34]),
                    int.Parse(excelMatrix[i, 35]),
                    int.Parse(excelMatrix[i, 36]),
                    int.Parse(excelMatrix[i, 37]),
                    int.Parse(excelMatrix[i, 38]),
                    int.Parse(excelMatrix[i, 39]),
                    int.Parse(excelMatrix[i, 40]),
                    int.Parse(excelMatrix[i, 41]),
                    int.Parse(excelMatrix[i, 42]),
                    int.Parse(excelMatrix[i, 43]),
                    int.Parse(excelMatrix[i, 44]),
                    excelMatrix[i, 45],
                    (bool)excelMatrixObj[i, 46]
                    );

                holdingsList.Add(holdings);
            }
            excelHoldings.Close();
            await OutprintAsync("Excel Document Sucessfully loaded into memory!I found " + holdingsList.Count + " Colonies!", ChannelID.botUpdatesId);
        }

        internal async Task ReadPlanetPicturesAndInfoFoldersAsync()
        {
            if (Directory.Exists("H:/My Drive/planet_pictures_and_info"))
            {
                string[] dirs = Directory.GetDirectories("H:/My Drive/planet_pictures_and_info", "*", SearchOption.AllDirectories);

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
                            if (!File.Exists(planetPicturesDir + newName))
                            {
                                File.Copy(picture, "H:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Pictures/Planet-Pictures/" + newName);
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
                            if (!File.Exists("G:/My Drive/Personal Stuff/Starport/PlanetPictures/Enemy Planets/" + newName))
                            {
                                File.Copy(picture, "G:/My Drive/Personal Stuff/Starport/PlanetPictures/Enemy Planets/" + newName);
                            }
                            else
                            {
                                File.Delete("G:/My Drive/Personal Stuff/Starport/PlanetPictures/Enemy Planets/" + newName);
                                File.Copy(picture, "G:/My Drive/Personal Stuff/Starport/PlanetPictures/Enemy Planets/" + newName);
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
                            if (!File.Exists("G:/My Drive/Personal Stuff/Starport/PlanetPictures/Undomed/" + newName))
                            {
                                File.Copy(picture, "G:/My Drive/Personal Stuff/Starport/PlanetPictures/Undomed/" + newName);
                            }
                            else
                            {
                                File.Delete("G:/My Drive/Personal Stuff/Starport/PlanetPictures/Undomed/" + newName);
                                File.Copy(picture, "G:/My Drive/Personal Stuff/Starport/PlanetPictures/Undomed/" + newName);
                            }
                        }
                    }
                }
            }
            await OutprintAsync("Read Folders Completed!", ChannelID.botUpdatesId);
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

        private async void OnChatChangedAsync(object sender, FileSystemEventArgs fileSysEvent)
        {
            string filePath = fileSysEvent.FullPath;
            string[] fileStrArr = new string[0];

            string[] split = Path.GetFileName(filePath).Split(" ");
            string chatLogOwner = split[0];

            try
            {
                fileStrArr = await File.ReadAllLinesAsync(filePath);
            }
            catch (System.Exception ex)
            {
                await OutprintAsync(ex.ToString(), ChannelID.botErrorsId);
            }

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
                    await OutprintAsync(lastLine, ChannelID.botTakingResourcesId);
                }
                else if (lastLine.Contains("tons of unidentified compounds"))
                {
                    if (lastLine.Contains("contains 0 tons of unidentified compounds") && !lastLine.Contains("System"))
                    {
                        await OutprintAsync("Undomed: " + lastLine, ChannelID.buildingId);
                    }
                    else if (lastLine.Contains("contains") && !lastLine.Contains("System"))
                    {
                        await OutprintAsync(lastLine, ChannelID.nuetrinoId);
                    }
                }
                else if (lastLine.Contains("Fellow corporation member"))
                {
                    await OutprintAsync(AtUser(lastLine) + lastLine, ChannelID.newsId);
                }
                else if (lastLine.Contains("Exporting holdings.csv has finished."))
                {
                    string csv = "";
                    if (File.Exists("C:/Users/ZANDER/StarportGE/holdings.csv"))
                    {
                        csv = "C:/Users/ZANDER/StarportGE/holdings.csv";
                        Algorithms.FileManipulation.FileDeleteIfExists(csvPath);
                        File.Copy(csv, csvPath); //Copy local to internet
                    }
                    else if (File.Exists("C:/Users/ALEX/StarportGE/holdings.csv"))
                    {
                        csv = "C:/Users/ALEX/StarportGE/holdings.csv";
                        Algorithms.FileManipulation.FileDeleteIfExists(csvPath);
                        File.Copy(csv, csvPath); //Copy Local to internet
                    }
                    System.Console.WriteLine("Copied csv to internet...");
                    //await OutprintAsync("Copied local csv to internet", ChannelID.botUpdatesId);

                    await Task.Delay(15000);
                    await Task.Run(() => LoadExcelHoldingsAsync());
                }
                else if (lastLine.Contains("Connection to server lost due to"))
                {
                    await OutprintAsync(AtUser("Autism") + "I lost connection!", ChannelID.newsId);
                    await OutprintAsync(AtUser("Autism") + "I lost connection!", ChannelID.botUpdatesId);
                }

                if (kombat)
                {
                    //warped in
                    if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && (lastLine.Contains("warped into") || lastLine.Contains("entered the system")))
                    {
                        List<string> enemiesList = Diplomacy.enemies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync("@everyone " + chatLogOwner + ": " + lastLine + " in " + lastSystem, ChannelID.enemySightingsId);
                        await SayAsync(enemy + " spotted! By " + chatLogOwner + " in " + lastSystem, ChannelID.voiceSlaversOnlyId);
                    }
                    else if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("warped out"))
                    {
                        List<string> enemiesList = Diplomacy.allies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync("@everyone " + lastLine + " in " + lastSystem, ChannelID.enemySightingsId);
                        await SayAsync(enemy + " ran away cause he's a bitch nigga", ChannelID.voiceSlaversOnlyId);
                    }
                    else if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("landed on a planet"))
                    {
                        List<string> enemiesList = Diplomacy.allies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync(lastLine + " in " + lastSystem, ChannelID.enemySightingsId);
                        await SayAsync(enemy + " landed!", ChannelID.voiceSlaversOnlyId);
                    }
                    else if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("docked"))
                    {
                        List<string> enemiesList = Diplomacy.allies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync(lastLine + " in " + lastSystem, ChannelID.enemySightingsId);
                        await SayAsync(enemy + " Re-Shielded!", ChannelID.voiceSlaversOnlyId);
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
                                await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + lastLine, ChannelID.newsId);
                                //await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + lastLine, ChannelID.alliedChatId);
                                enemiesSlain++;
                            }
                            else if (!string.IsNullOrEmpty(ally) && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + lastLine, ChannelID.newsId);
                                //await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + lastLine, ChannelID.alliedChatId);
                            }
                            else
                            {
                                await OutprintAsync(lastLine, ChannelID.newsId);
                                //await OutprintAsync(lastLine, ChannelID.alliedChatId);
                            }

                            await SayAsync(enemy + " Has Been Slain!", ChannelID.voiceSlaversOnlyId);
                        }
                        else if (lastLine.Contains("For blowing up"))
                        {
                            enemiesSlain++;
                            if (secondToLastLine.Contains("Defenses") && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + secondToLastLine, ChannelID.newsId);
                                //await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + secondToLastLine, ChannelID.alliedChatId);
                                enemiesSlain++;
                            }
                            else if (!string.IsNullOrEmpty(ally) && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + secondToLastLine, ChannelID.newsId);
                                //await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + secondToLastLine, ChannelID.alliedChatId);
                            }
                            else
                            {
                                await OutprintAsync(secondToLastLine, ChannelID.newsId);
                                //await OutprintAsync(secondToLastLine, ChannelID.alliedChatId);
                            }

                            await SayAsync(enemy + " Has Been Slain!", ChannelID.voiceSlaversOnlyId);
                        }
                        else if (lastLine.Contains("You recover"))
                        {
                            enemiesSlain++;
                            if (thirdToLastLine.Contains("Defenses") && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + thirdToLastLine, ChannelID.newsId);
                                //await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + secondToLastLine, ChannelID.alliedChatId);
                                enemiesSlain++;
                            }
                            else if (!string.IsNullOrEmpty(ally) && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + thirdToLastLine, ChannelID.newsId);
                                //await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + secondToLastLine, ChannelID.alliedChatId);
                            }
                            else
                            {
                                await OutprintAsync(thirdToLastLine, ChannelID.newsId);
                                //await OutprintAsync(secondToLastLine, ChannelID.alliedChatId);
                            }

                            await SayAsync(enemy + " Has Been Slain!", ChannelID.voiceSlaversOnlyId);
                        }
                        else if (lastLine.Contains("shot down " + ally) && !ally.Equals(" "))
                        {
                            alliesSlain++;
                            await OutprintAsync(lastLine + " Help " + ally + " Nigga. Damn!", ChannelID.newsId);
                            //await OutprintAsync(lastLine + " Help " + ally + " Nigga. Damn!", ChannelID.alliedChatId);
                            await SayAsync(ally + " Has Been Slain!", ChannelID.voiceSlaversOnlyId);
                        }
                    }

                    //invasions
                    if (lastLine.Contains("was invaded and taken"))
                    {
                        await OutprintAsync(AtUser(lastLine) + lastLine, ChannelID.distressCallsId);
                        await OutprintAsync(AtUser(lastLine) + lastLine, ChannelID.recapListId);
                        string planetName = Algorithms.StringManipulation.GetBetween(lastLine, "on", "(");
                        int holdingsIndex = holdingsList.FindIndex(planet => planet.location == planetName);

                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/crying-meme-black-guy-cries-sad-man-thank-god-for-my-reefer-hood-news-gif-24902056 " + '\n' +
                                "WE Lost Double Dome!", ChannelID.slaversId);
                            await OutprintAsync("https://tenor.com/view/crying-meme-black-guy-cries-sad-man-thank-god-for-my-reefer-hood-news-gif-24902056 " + '\n' +
                                "WE Lost Double Dome!", ChannelID.newsId);
                        }
                        await SayAsync("We've Lost a Command Post!", ChannelID.voiceSlaversOnlyId);

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
                                await OutprintAsync(experience + '\n' + e.ToString(), ChannelID.botUpdatesId);
                            }

                            if (exp > 50000)
                            {
                                planetsLost++;
                            }
                        }

                        if (holdingsIndex != -1)
                        {
                            await OutprintAsync(
                                "Metal Ore: " + holdingsList[holdingsIndex].ore + " | Solar: " + holdingsList[holdingsIndex].solarShots + " / " + holdingsList[holdingsIndex].solarFreq + '\n' +
                                " | Nukes: " + holdingsList[holdingsIndex].nukes + " | Negotiators: " + holdingsList[holdingsIndex].negotiators + " | Compound Mines: " + holdingsList[holdingsIndex].compoundMines + " | Lasers: " + holdingsList[holdingsIndex].laserCannons + '\n' +
                                " | Population: " + holdingsList[holdingsIndex].population + " | Discoveries: " + holdingsList[holdingsIndex].discoveries, ChannelID.distressCallsId);
                        }
                    }
                    else if (lastLine.Contains("captured the colony"))
                    {
                        await OutprintAsync(lastLine, ChannelID.newsId);
                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                               "WE CAPTURED A DOUBLE DOME!", ChannelID.slaversId);
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                               "WE CAPTURED A DOUBLE DOME!", ChannelID.newsId);
                        }
                        //await OutprintAsync(lastLine, ChannelID.alliedChatId);
                        await SayAsync("We've Captured a Command Post!", ChannelID.voiceSlaversOnlyId);
                        planetsKaptured++;
                    }
                    else if (lastLine.Contains("You claim ownership of the colony owned by"))
                    {
                        await OutprintAsync(secondToLastLine, ChannelID.newsId);
                        //await OutprintAsync(secondToLastLine, ChannelID.alliedChatId);
                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", ChannelID.slaversId);
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", ChannelID.newsId);
                        }
                        await SayAsync("We've Captured a Command Post!", ChannelID.voiceSlaversOnlyId);
                        planetsKaptured++;
                    }
                    else if (lastLine.Contains("It now belongs to"))
                    {
                        await OutprintAsync(fileStrArr[fileStrArr.Length - 3], ChannelID.newsId);
                        //await OutprintAsync(fileStrArr[fileStrArr.Length - 3], ChannelID.alliedChatId);
                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", ChannelID.slaversId);
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", ChannelID.newsId);
                        }
                        await SayAsync("We've Captured a Command Post!", ChannelID.voiceSlaversOnlyId);
                        planetsKaptured++;
                    }
                    else if (lastLine.Contains("For successful invasion"))
                    {
                        await OutprintAsync(fileStrArr[fileStrArr.Length - 4], ChannelID.newsId);
                        //await OutprintAsync(fileStrArr[fileStrArr.Length - 4], ChannelID.alliedChatId);
                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", ChannelID.slaversId);
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", ChannelID.newsId);
                        }
                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", ChannelID.newsId);
                        }
                        await SayAsync("We've Captured a Command Post!", ChannelID.voiceSlaversOnlyId);
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
                        int holdingsIndex = holdingsList.FindIndex(planet => planet.location == planetName);

                        if (colonyName.Contains("(") && colonyName.Contains("."))
                        {
                            colonyName = Algorithms.StringManipulation.GetBetween(colonyName, ")", ".");
                        }

                        //System.Console.WriteLine("colonyName: " + colonyName);
                        string colonyPath = planetPicturesDir + colonyName + ".png";
                        string planetPath = planetPicturesDir + planetName + ".png";

                        if (File.Exists(colonyPath))
                        {
                            await OutprintAsync("@everyone " + lastLine, ChannelID.distressCallsId);
                            await OutprintFileAsync(colonyPath, ChannelID.distressCallsId);
                        }
                        else if (File.Exists(planetPath))
                        {
                            await OutprintAsync("@everyone " + lastLine, ChannelID.distressCallsId);
                            await OutprintFileAsync(planetPath, ChannelID.distressCallsId);
                        }
                        else
                        {
                            await OutprintAsync("@everyone " + lastLine, ChannelID.distressCallsId);
                        }

                        if (holdingsList == null)
                        {
                            await LoadExcelHoldingsAsync();
                        }

                        if (holdingsIndex != -1)
                        {
                            await OutprintAsync(
                                "Metal Ore: " + holdingsList[holdingsIndex].ore + " | Solar: " + holdingsList[holdingsIndex].solarShots + " / " + holdingsList[holdingsIndex].solarFreq + '\n' +
                                " | Nukes: " + holdingsList[holdingsIndex].nukes + " | Negotiators: " + holdingsList[holdingsIndex].negotiators + " | Compound Mines: " + holdingsList[holdingsIndex].compoundMines + " | Lasers: " + holdingsList[holdingsIndex].laserCannons + '\n' +
                                " | Population: " + holdingsList[holdingsIndex].population + " | Discoveries: " + holdingsList[holdingsIndex].discoveries, ChannelID.distressCallsId);
                        }
                        else
                        {
                            await OutprintAsync("Couldnt find " + planetName + " in the spreadsheet!", ChannelID.botErrorsId);
                        }
                    }
                    else if (lastLine.Contains("***") && lastLine.Contains("landed"))
                    {
                        landings++;
                        await OutprintAsync(lastLine, ChannelID.distressCallsId);
                    }
                }
                if (alerts)
                {
                    if (lastLine.Contains("Server Alert!"))
                    {
                        if (secondToLastLine.Contains("Server Alert!"))
                        {
                            await OutprintAsync("@everyone " + secondToLastLine, ChannelID.slaversId);
                        }
                        await OutprintAsync("@everyone " + lastLine, ChannelID.slaversId);
                        //await OutprintAsync("@everyone " + lastLine, ChannelID.alliedChatId);
                    }
                    else if (lastLine.Contains("U.N. Hotline"))
                    {
                        List<string> alliesList = Diplomacy.allies.ToList<string>();
                        string ally = alliesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync(lastLine, ChannelID.newsId);
                        await SayAsync(ally + " I see you!", ChannelID.voiceSlaversOnlyId);
                    }
                    else if (lastLine.Contains("Empress Allie says to Slavers"))
                    {
                        await OutprintAsync(lastLine, ChannelID.botUpdatesId);
                    }
                    else if (lastLine.Contains("due to pollution"))
                    {
                        await OutprintAsync(AtUser(lastLine) + lastLine, ChannelID.newsId);
                        await SayAsync(AtUser(lastLine) + " your colony had a disasta", ChannelID.voiceSlaversOnlyId);
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

                        await CreateCalendarEventAsync(days3, title, lastLine, ChannelID.buildingId);
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

                        int holdingsIndex = holdingsList.FindIndex(planet => planet.location == planetName);
                        if (holdingsIndex != -1)
                        {
                            Holding planet = holdingsList[holdingsIndex];
                            if (StarportHelperClasses.Helper.IsZoundsable(planet.planetType, discovery))
                            {
                                string message = AllPlanetInfo(planet);

                                await OutprintAsync(AtUser(planet.owner) + lastLine + " Zounds dat hoe now!" + '\n' + message, ChannelID.zoundsForHoundsId);
                            }
                        }
                        else
                        {
                            if (holdingsList == null)
                            {
                                await LoadExcelHoldingsAsync();
                            }
                            holdingsIndex = holdingsList.FindIndex(planet => planet.location == planetName);
                            if (holdingsIndex != -1)
                            {
                                Holding planet = holdingsList[holdingsIndex];
                                if (StarportHelperClasses.Helper.IsZoundsable(planet.planetType, discovery))
                                {
                                    string message = AllPlanetInfo(planet);

                                    await OutprintAsync(AtUser(planet.owner) + lastLine + " Zounds dat hoe now!" + '\n' + message, ChannelID.zoundsForHoundsId);
                                }
                            }
                            else
                            {
                                await OutprintAsync(AtUser("Autism") + lastLine + '\n' + planetName + " got Adv Arch, but i couldn't find " + planetName + " in holdings!", ChannelID.botUpdatesId);
                            }
                        }
                    }
                    else if (lastLine.Contains("Military Tradition lvl 3") || lastLine.Contains("Military Tradition lvl 4") || lastLine.Contains("Military Tradition lvl 5"))
                    {
                        await OutprintAsync(AtUser(lastLine) + lastLine + " Decent Huge Metro col", ChannelID.buildingId);
                    }
                    //if bio3
                    else if (lastLine.Contains("completed work on the Biodome Level 3."))
                    {
                        await OutprintAsync(AtUser(lastLine) + lastLine, ChannelID.buildingId);
                        colsBuilt++;
                    }

                    //Domed new colony && dd
                    if (lastLine.Contains("founding"))
                    {
                        await OutprintAsync(chatLogOwner + " colonized " + lastLand + "!", ChannelID.buildingId);
                        await SayAsync(chatLogOwner + " colonized a new world!", ChannelID.voiceBuildingId);
                    }
                    else if (lastLine.Contains("adding another dome"))
                    {
                        await OutprintAsync(chatLogOwner + " Created a New Double Dome on " + lastLand + "!", ChannelID.buildingId);
                        await OutprintAsync("https://tenor.com/view/cat-shooting-mouth-open-gif-15017033", ChannelID.buildingId);
                        await SayAsync(chatLogOwner + " Created a New Double Dome on " + lastLand + "!", ChannelID.voiceSlaversOnlyId);
                        //await CelebrateUser("Slavers", "I'd like to see them try and take this!", ChannelID.slaversId);
                    }
                }
            }
        }

        private async Task RunThroughTextAsync()
        {
            if (Directory.Exists("H:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Channel"))
            {
                string[] filePaths = Directory.GetFiles("H:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Channel");

                for (int i = 0; i < filePaths.Length; i++)
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
                                    await OutprintAsync(fileAsArr, ChannelID.botUpdatesId);

                                    await File.WriteAllTextAsync(filePaths[i], " "); //now clear it out
                                    await OutprintAsync(filePaths[i] + " cleared!", ChannelID.botUpdatesId);
                                    System.Console.WriteLine(filePaths[i] + " cleared!");
                                }
                                else if (Path.GetFileName(filePaths[i]).Equals("building.txt"))
                                {
                                    await OutprintAsync(fileAsArr, ChannelID.buildingId);

                                    await File.WriteAllTextAsync(filePaths[i], " "); //now clear it out
                                    await OutprintAsync(filePaths[i] + " cleared!", ChannelID.botUpdatesId);
                                    System.Console.WriteLine(filePaths[i] + " cleared!");
                                }
                                else if (Path.GetFileName(filePaths[i]).Equals("distress.txt"))
                                {
                                    await OutprintAsync(fileAsArr, ChannelID.distressCallsId);

                                    await File.WriteAllTextAsync(filePaths[i], " "); //now clear it out
                                    await OutprintAsync(filePaths[i] + " cleared!", ChannelID.botUpdatesId);
                                    System.Console.WriteLine(filePaths[i] + " cleared!");
                                }
                                else if (Path.GetFileName(filePaths[i]).Equals("scoutReports.txt"))
                                {
                                    await OutprintAsync(fileAsArr, ChannelID.scoutReportsId);

                                    await File.WriteAllTextAsync(filePaths[i], " "); //now clear it out
                                    await OutprintAsync(filePaths[i] + " cleared!", ChannelID.botUpdatesId);
                                    System.Console.WriteLine(filePaths[i] + " cleared!");
                                }
                            }
                        }
                    }
                }
            }
        }

        private async Task UpdateCompanionFiles(Holding planet, string path, string message, string type)
        {
            await File.AppendAllTextAsync(path, message);

            string fileName = AtUser(planet.owner);
            if (AtUser(planet.owner) != "")
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