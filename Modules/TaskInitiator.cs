//Created by Alexander Fields https://github.com/roku674

using StarportObjects;
using ExcelCSharp;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace DiscordBotUpdates.Modules
{
    internal class TaskInitiator : DBUTask
    {
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
        public static bool loadingExcel { get; set; }
        public static string lastLand { get; set; }
        public static string lastSystem { get; set; }
        public static uint planetsKaptured { get; set; }
        public static uint planetsLost { get; set; }

        public static async Task LoadExcelHoldingsAsync()
        {
            if (!loadingExcel)
            {
                loadingExcel = true;
                holdingsList = new List<Holding>();

                await Task.Delay(3000);
                Excel.Kill();

                await Excel.ConvertFromCSVtoXLSXAsync(Program.filePaths.csvPath, Program.filePaths.excelPath);

                Excel excelHoldings = new Excel(Program.filePaths.excelPath, 1);
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
                    }

                    excelMatrixObj[i, 9] = DateTime.MaxValue;

                    if ((int)excelMatrixObj[i, 46] == 1)
                    {
                        excelMatrixObj[i, 46] = true;
                    }
                    else
                    {
                        excelMatrixObj[i, 46] = false;
                    }

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
                await OutprintAsync("Excel Document Sucessfully loaded into memory!I found " + holdingsList.Count + " Colonies!", Program.channelId.botUpdatesId);
                loadingExcel = false;
            }
        }

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

        public async Task FindEnemyColoniesAsync(string enemy, string folder)
        {
            //System.Console.WriteLine("Finding " + enemy + " colonies!");
            await OutprintAsync(enemy + "'s planets:", Program.channelId.scoutReportsId);

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
                v2.X == (float)planet.galaxyX &&
                v2.Y == (float)planet.galaxyY
                );

                if (pscIndex == -1)
                {
                    planetarySystemCoords.Add(new System.Numerics.Vector2(planet.galaxyX, planet.galaxyY));
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
                channel = Program.channelId.pollutionFinderId;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.pollution > 0 && planet.pollutionRate > 1)
                    {
                        polluting++;
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Disasters: " + planet.disaster + " | Pollution: " + planet.pollution + " + " + planet.pollutionRate + "/day" + '\n';
                        await UpdateCompanionFilesAsync(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "PollutionCrit")
            {
                channel = Program.channelId.pollutionCritId;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.pollution > 40 && planet.pollutionRate > 0)
                    {
                        pollutingCrit++;
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Disasters: " + planet.disaster + " | Pollution: " + planet.pollution + " + " + planet.pollutionRate + "/day" + '\n';
                        await UpdateCompanionFilesAsync(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "Zoundsables")
            {
                channel = Program.channelId.zoundsForHoundsId;
                int zoundsableCounter = 0;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.population < 100000 && StarportHelperClasses.Helper.IsZoundsable(planet.planetType, planet.discoveries))
                    {
                        zoundsableCounter++;
                        string message = AllPlanetInfo(planet);

                        await UpdateCompanionFilesAsync(planet, tempPath, message, type);
                    }
                }
                await OutprintAsync("Zoundsables found: " + zoundsableCounter, channel);
            }
            else if (type == "DD")
            {
                channel = Program.channelId.ddId;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.name.Contains("DD") ||
                       ((planet.name.EndsWith(".D") || planet.name.EndsWith(".DI") || planet.name.Contains(".ZD")))
                       )
                    {
                        ddCount++;
                        string message = AllPlanetInfo(planet);
                        await UpdateCompanionFilesAsync(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "Solar Off")
            {
                channel = Program.channelId.solarOffId;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.solarShots == 0 && planet.population > 1000 || (planet.solarShots > 0 && planet.population > 10000 && planet.ore <= 5000))
                    {
                        solarOff++;
                        string message = AllPlanetInfo(planet);
                        await UpdateCompanionFilesAsync(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "Solar Weak")
            {
                channel = Program.channelId.solarWeakId;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.solarShots < 25 && planet.population > 5000)
                    {
                        solarWeak++;
                        string message = AllPlanetInfo(planet);
                        await UpdateCompanionFilesAsync(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "MilitaryTest")
            {
                uint military = 10000;

                channel = Program.channelId.botTestingId;
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
                        await UpdateCompanionFilesAsync(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "Revolt")
            {
                channel = Program.channelId.revoltFinderId;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.morale < 0 || planet.moraleChange < 0.00d)
                    {
                        negativeMorale++;
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Morale: " + planet.morale + " + " + planet.moraleChange + "/hour" + '\n';
                        await UpdateCompanionFilesAsync(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "Shrinking")
            {
                channel = Program.channelId.shrinkingFinderId;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.popGrowth < -1d && planet.popGrowth > -2000)
                    {
                        negativeGrowth++;
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Population: " + planet.population + " | Growth Rate: " + planet.popGrowth + "/hour" + '\n';
                        await UpdateCompanionFilesAsync(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "All")
            {
                channel = Program.channelId.colonyManagementId;

                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.popGrowth < -1d && planet.popGrowth > -2000)
                    {
                        negativeGrowth++;
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Population: " + planet.population + " | Growth Rate: " + planet.popGrowth + "/hour" + '\n';
                        await UpdateCompanionFilesAsync(planet, tempPath, message, type);
                    }
                    if (planet.morale < 0 || planet.moraleChange < 0.00d)
                    {
                        negativeMorale++;
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Morale: " + planet.morale + " + " + planet.moraleChange + "/hour" + '\n';
                        await UpdateCompanionFilesAsync(planet, tempPath, message, type);
                    }
                    if (planet.pollution > 0 || planet.pollutionRate > 1)
                    {
                        polluting++;
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Disasters: " + planet.disaster + " | Pollution: " + planet.pollution + " + " + planet.pollutionRate + "/day" + '\n';
                        await UpdateCompanionFilesAsync(planet, tempPath, message, type);
                    }
                }
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
                    for (int i = 0; i < remainderLogsArr.Length; i++)
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
                string picturesDir = Program.filePaths.networkPathDir + "/Pictures";
                string[] paths =
                {
                   picturesDir,
                   picturesDir + "/Building",
                   picturesDir + "/Distress",
                   picturesDir + "/Planet-Pictures-Enemy",
                   picturesDir + "/Planet-Pictures-Friendly",
                   picturesDir + "/Planet-Pictures-Undomed",
                   picturesDir + "/Scout-Reports",
                   picturesDir + "/Targets",
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
                                        await OutprintFileAsync(picture, Program.channelId.botUpdatesId);
                                        break;

                                    case 1:
                                        await OutprintFileAsync(picture, Program.channelId.buildingId);
                                        break;

                                    case 2:
                                        await OutprintFileAsync(picture, Program.channelId.distressCallsId);
                                        break;

                                    case 3:
                                        if (Directory.Exists(Program.filePaths.planetPicturesDir + "/Enemy Planets/"))
                                        {
                                            if (picture.Contains("_"))
                                            {
                                                picture.Replace("_", " ");
                                            }
                                            if (!File.Exists(Program.filePaths.planetPicturesDir + "/" + Path.GetFileName(picture)))
                                            {
                                                File.Copy(picture, Program.filePaths.planetPicturesDir + "/" + Path.GetFileName(picture));
                                                await OutprintAsync("Enemy Planet Picture Downloaded!", Program.channelId.planetPicturesEnemyId);
                                                await OutprintFileAsync(picture, Program.channelId.planetPicturesEnemyId);
                                            }
                                            else
                                            {
                                                Algorithms.FileManipulation.DeleteFile(Program.filePaths.planetPicturesDir + "/" + Path.GetFileName(picture));
                                                File.Copy(picture, Program.filePaths.planetPicturesDir + "/" + Path.GetFileName(picture));
                                                await OutprintAsync("Enemy Planet Picture Updated/Replaced!", Program.channelId.planetPicturesEnemyId);
                                                await OutprintFileAsync(picture, Program.channelId.planetPicturesEnemyId);
                                            }
                                        }
                                        else
                                        {
                                            await OutprintAsync(picture + " : was not successfully downloaded!", Program.channelId.planetPicturesEnemyId);
                                        }
                                        break;

                                    case 4:

                                        if (Directory.Exists(Program.filePaths.planetPicturesDir))
                                        {
                                            if (picture.Contains("_"))
                                            {
                                                picture.Replace("_", " ");
                                            }
                                            if (!File.Exists(Program.filePaths.planetPicturesDir + "/" + Path.GetFileName(picture)))
                                            {
                                                File.Copy(picture, Program.filePaths.planetPicturesDir + "/" + Path.GetFileName(picture));
                                                await OutprintAsync("Colony Picture Downloaded!", Program.channelId.planetPicturesFriendlyId);
                                                await OutprintFileAsync(picture, Program.channelId.planetPicturesFriendlyId);
                                            }
                                            else
                                            {
                                                await OutprintAsync(Path.GetFileName(picture) + " was not downloaded as there was a duplicate!", Program.channelId.planetPicturesFriendlyId);
                                                //await OutprintFileAsync(picture, Program.channelId.botUpdatesId);
                                            }
                                        }
                                        else
                                        {
                                            await OutprintAsync(picture + " : was not successfully downloaded!", Program.channelId.planetPicturesFriendlyId);
                                        }

                                        break;

                                    case 5:
                                        if (Directory.Exists(Program.filePaths.planetPicturesDir + "/Undomed/"))
                                        {
                                            if (picture.Contains("_"))
                                            {
                                                picture.Replace("_", " ");
                                            }
                                            if (!File.Exists(Program.filePaths.planetPicturesDir + "/" + Path.GetFileName(picture)))
                                            {
                                                File.Copy(picture, Program.filePaths.planetPicturesDir + "/" + Path.GetFileName(picture));
                                                await OutprintAsync("Enemy Planet Picture Downloaded!", Program.channelId.planetPicturesUndomedId);
                                                await OutprintFileAsync(picture, Program.channelId.planetPicturesUndomedId);
                                            }
                                            else
                                            {
                                                Algorithms.FileManipulation.DeleteFile(Program.filePaths.planetPicturesDir + "/" + Path.GetFileName(picture));
                                                File.Copy(picture, Program.filePaths.planetPicturesDir + "/" + Path.GetFileName(picture));
                                                await OutprintAsync("Enemy Planet Picture Updated/Replaced!", Program.channelId.planetPicturesUndomedId);
                                                await OutprintFileAsync(picture, Program.channelId.planetPicturesUndomedId);
                                            }
                                        }
                                        else
                                        {
                                            await OutprintAsync(picture + " : was not successfully downloaded!", Program.channelId.planetPicturesUndomedId);
                                        }
                                        break;

                                    case 6:
                                        await OutprintFileAsync(picture, Program.channelId.scoutReportsId);
                                        break;

                                    case 7:
                                        await OutprintFileAsync(picture, Program.channelId.targetsId);
                                        break;

                                    default:
                                        await OutprintFileAsync(picture, Program.channelId.botUpdatesId);
                                        break;
                                }
                            }

                            Algorithms.FileManipulation.DeleteFile(picture);
                        }
                    }
                }

                if (i == 1)
                {
                    System.Console.WriteLine("Picture Updater: First pass completed!");
                }

                task.ticker++;
            }
            await OutprintAsync("No Longer Listening for Pictures Updates!", Program.channelId.botCommandsId);
            runningTasks.RemoveAt(taskNum);
            dbuTaskNum--;
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

        public async Task UpdateAllieTxt(string text)
        {
            string alliePath = Program.filePaths.networkPathDir + "/Echo/Allie.txt";
            string[] lines = new string[9];

            for (int i = 0; i < lines.Length; i++)
            {
                lines[i] = " ";
            }

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
                Holding[] lastHolding = new Holding[8];
                for (int i = 0; i < holdingsList.Count - 1; i++)
                {
                    if (planetToBuild.galaxyX == holdingsList[i].galaxyX && planetToBuild.galaxyY == holdingsList[i].galaxyY)
                    {
                        Holding planetInSystem = holdingsList[i];

                        //Capitalize first letter
                        string planetType = planetInSystem.planetType.Replace(
                                   planetInSystem.planetType[0].ToString(),
                                   planetInSystem.planetType[0].ToString().ToUpper());

                        if (lastHolding[0] == null)
                        {
                            for (int j = 0; j < lastHolding.Length; j++)
                            {
                                lastHolding[j] = planetInSystem;
                            }
                        }

                        if (planetInSystem.ore > lastHolding[0].ore)
                        {
                            lines[1] = planetInSystem.location + " Type0 " + planetType;
                            lastHolding[0] = planetInSystem;
                        }
                        else if (planetToBuild.ore > 5000)
                        {
                            lines[1] = lines[1].Replace(lines[1], " Type0 ");
                        }

                        if (planetInSystem.ana > lastHolding[1].ana)
                        {
                            lines[2] = planetInSystem.location + " Type1 " + planetType;
                            lastHolding[1] = planetInSystem;
                        }
                        else if (planetToBuild.ana > 3000)
                        {
                            lines[2] = lines[2].Replace(lines[2], " Type1 ");
                        }

                        if (planetInSystem.med > lastHolding[2].med)
                        {
                            lines[3] = planetInSystem.location + " Type2 " + planetType;
                            lastHolding[2] = planetInSystem;
                        }
                        else if (planetToBuild.med > 1500)
                        {
                            lines[3] = lines[3].Replace(lines[3], " Type2 ");
                        }

                        if (planetInSystem.org > lastHolding[3].org)
                        {
                            lines[4] = planetInSystem.location + " Type3 " + planetType;
                            lastHolding[3] = planetInSystem;
                        }
                        else if (planetToBuild.org > 3000)
                        {
                            lines[4] = lines[4].Replace(lines[4], " Type3 ");
                        }

                        if (planetInSystem.oil > lastHolding[4].oil)
                        {
                            lines[5] = planetInSystem.location + " Type4 " + planetType;
                            lastHolding[4] = planetInSystem;
                        }
                        else if (planetToBuild.oil > 3000)
                        {
                            lines[5] = lines[5].Replace(lines[5], " Type4 ");
                        }

                        if (planetInSystem.ura > lastHolding[5].ura)
                        {
                            lines[6] = planetInSystem.location + " Type5 " + planetType;
                            lastHolding[5] = planetInSystem;
                        }
                        else if (planetToBuild.ura > 3000)
                        {
                            lines[6] = lines[6].Replace(lines[6], " Type5 ");
                        }

                        if (planetInSystem.equ > lastHolding[6].equ)
                        {
                            lines[7] = planetInSystem.location + " Type6 " + planetType;
                            lastHolding[6] = planetInSystem;
                        }
                        else if (planetToBuild.equ > 3000)
                        {
                            lines[7] = lines[7].Replace(lines[7], " Type6 ");
                        }

                        if (planetInSystem.spi > lastHolding[7].spi)
                        {
                            lines[8] = planetInSystem.location + " Type7 " + planetType;
                            lastHolding[7] = planetInSystem;
                        }
                        else if (planetToBuild.spi > 2000)
                        {
                            lines[8] = lines[8].Replace(lines[8], " Type7 ");
                        }
                    }
                }

                for (int i = 0; i < lines.Length - 1; i++)
                {
                    if (string.IsNullOrEmpty(lines[i]))
                    {
                        lines[i] = "Type" + i;
                    }
                }
            }

            await File.WriteAllLinesAsync(alliePath, lines);
            await OutprintAsync(AtUser("Autism"), Program.channelId.botUpdatesId);
            await OutprintFileAsync(alliePath, Program.channelId.botUpdatesId);
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
                else if (lastLine.Contains("Exporting holdings.csv has finished."))
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

                    await Task.Delay(15000);
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
                        int holdingsIndex = tempHoldingsList.FindIndex(planet => planet.location == planetName);

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
                                "Metal Ore: " + tempHoldingsList[holdingsIndex].ore + " | Solar: " + tempHoldingsList[holdingsIndex].solarShots + " / " + tempHoldingsList[holdingsIndex].solarFreq + '\n' +
                                " | Nukes: " + tempHoldingsList[holdingsIndex].nukes + " | Negotiators: " + tempHoldingsList[holdingsIndex].negotiators + " | Compound Mines: " + tempHoldingsList[holdingsIndex].compoundMines + " | Lasers: " + tempHoldingsList[holdingsIndex].laserCannons + '\n' +
                                " | Population: " + tempHoldingsList[holdingsIndex].population + " | Discoveries: " + tempHoldingsList[holdingsIndex].discoveries, Program.channelId.distressCallsId);
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
                        int holdingsIndex = tempHoldingsList.FindIndex(planet => planet.location == planetName);

                        if (colonyName.Contains("(") && colonyName.Contains("."))
                        {
                            colonyName = Algorithms.StringManipulation.GetBetween(colonyName, ")", ".");
                        }

                        //System.Console.WriteLine("colonyName: " + colonyName);
                        string colonyPath = Program.filePaths.planetPicturesDir + "/" + colonyName + ".png";
                        string planetPath = Program.filePaths.planetPicturesDir + "/" + planetName + ".png";

                        if (File.Exists(colonyPath))
                        {
                            await OutprintAsync("@everyone " + lastLine, Program.channelId.distressCallsId);
                            await OutprintFileAsync(colonyPath, Program.channelId.distressCallsId);
                        }
                        else if (File.Exists(planetPath))
                        {
                            await OutprintAsync("@everyone " + lastLine, Program.channelId.distressCallsId);
                            await OutprintFileAsync(planetPath, Program.channelId.distressCallsId);
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
                                "Metal Ore: " + tempHoldingsList[holdingsIndex].ore + " | Solar: " + tempHoldingsList[holdingsIndex].solarShots + " / " + tempHoldingsList[holdingsIndex].solarFreq + '\n' +
                                " | Nukes: " + tempHoldingsList[holdingsIndex].nukes + " | Negotiators: " + tempHoldingsList[holdingsIndex].negotiators + " | Compound Mines: " + tempHoldingsList[holdingsIndex].compoundMines + " | Lasers: " + tempHoldingsList[holdingsIndex].laserCannons + '\n' +
                                " | Population: " + tempHoldingsList[holdingsIndex].population + " | Discoveries: " + tempHoldingsList[holdingsIndex].discoveries, Program.channelId.distressCallsId);
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

                        int holdingsIndex = tempHoldingsList.FindIndex(planet => planet.location == planetName);
                        if (holdingsIndex != -1)
                        {
                            Holding planet = tempHoldingsList[holdingsIndex];
                            if (StarportHelperClasses.Helper.IsZoundsable(planet.planetType, discovery))
                            {
                                string message = AllPlanetInfo(planet);

                                await OutprintAsync(AtUser(planet.owner) + lastLine + " Zounds dat hoe now!" + '\n' + message, Program.channelId.zoundsForHoundsId);
                            }
                        }
                        else
                        {
                            holdingsIndex = tempHoldingsList.FindIndex(planet => planet.location == planetName);
                            if (holdingsIndex != -1)
                            {
                                Holding planet = tempHoldingsList[holdingsIndex];
                                if (StarportHelperClasses.Helper.IsZoundsable(planet.planetType, discovery))
                                {
                                    string message = AllPlanetInfo(planet);

                                    await OutprintAsync(AtUser(planet.owner) + lastLine + " Zounds dat hoe now!" + '\n' + message, Program.channelId.zoundsForHoundsId);
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