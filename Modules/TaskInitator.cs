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
        public static readonly string excelPath = "G:/My Drive/Personal Stuff/Starport/holdings.xlsx";
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

            for (int i = 0; i < duration; i++)
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
                    "H:/My Drive/Shared/DiscordBotUpdates/DiscordBotUpdates/bin/Release/netcoreapp3.1/Pictures/Planet-Pictures",
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
                                        await OutprintFileAsync(picture, ChannelID.botUpdatesID);
                                        break;

                                    case 1:
                                        await OutprintFileAsync(picture, ChannelID.buildingID);
                                        break;

                                    case 2:
                                        await OutprintFileAsync(picture, ChannelID.distressCallsID);
                                        break;

                                    case 3:

                                        if (Directory.Exists("G:/My Drive/Personal Stuff/Starport/PlanetPictures"))
                                        {
                                            if (picture.Contains("_"))
                                            {
                                                picture.Replace("_", " ");
                                            }
                                            if (!File.Exists(planetPicturesDir + Path.GetFileName(picture)))
                                            {
                                                File.Copy(picture, planetPicturesDir + Path.GetFileName(picture));
                                                await OutprintAsync("Colony Picture Downloaded!", ChannelID.planetPicturesID);
                                                await OutprintFileAsync(picture, ChannelID.planetPicturesID);
                                            }
                                            else
                                            {
                                                await OutprintAsync(Path.GetFileName(picture) + " was not downloaded as there was a duplicate!", ChannelID.botUpdatesID);
                                                //await OutprintFileAsync(picture, ChannelID.botUpdatesID);
                                            }
                                        }
                                        else
                                        {
                                            await OutprintAsync(picture + " : was not successfully downloaded!", ChannelID.planetPicturesID);
                                        }

                                        break;

                                    case 4:
                                        await OutprintFileAsync(picture, ChannelID.scoutReportsID);
                                        break;

                                    case 5:
                                        await OutprintFileAsync(picture, ChannelID.targetsID);
                                        break;

                                    default:
                                        await OutprintFileAsync(picture, ChannelID.botUpdatesID);
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
            await OutprintAsync("No Longer Listening for Pictures Updates!", ChannelID.botCommandsID);
            runningTasks.RemoveAt(taskNum);
            dbuTaskNum--;
        }

        /// <summary>
        /// Call this to start the Message Updater
        /// </summary>
        /// <returns></returns>
        public async Task TextUpdaterAsync(uint id)
        {
            //await Outprint("Sucessfully Initiated " + type + " Listener!", ChannelID.botCommandsID);
            await Task.Delay(500);
            int taskNum = runningTasks.FindIndex(task => task.id == id);

            while (taskNum == -1)
            {
                taskNum = runningTasks.FindIndex(task => task.id == id);
            }

            DBUTaskObj task = runningTasks.ElementAt(taskNum);
            System.Console.WriteLine("Sucessfully Initiated Text Listener!", ChannelID.botCommandsID);

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
                    System.Console.WriteLine("Text Updater: First pass completed!");
                }

                System.TimeSpan timeSpan = System.DateTime.Now - System.DateTime.Today.AddDays(1).AddSeconds(-1);

                //System.Console.WriteLine(timeSpan.Hours + ":" + timeSpan.Minutes + ":" + timeSpan.Seconds);

                //midnight trigger
                if (timeSpan.Hours == 0 && timeSpan.Minutes == 0 && timeSpan.Seconds == 0)
                {
                    if (planetsLost >= 15)
                    {
                        await OutprintAsync("https://tenor.com/view/correr-despavoridos-enchufe-tv-huir-invasion-extraterrestre-corran-por-sus-vidas-gif-24995288", ChannelID.slaversID);
                    }
                    if (((int)planetsKaptured - (int)planetsLost) >= 15)
                    {
                        await OutprintAsync("https://tenor.com/view/cat-shooting-mouth-open-gif-15017033", ChannelID.slaversID);
                    }
                    if (colsBuilt >= 10)
                    {
                        await OutprintAsync("https://tenor.com/view/construct-construction-nail-and-hammer-build-worker-gif-13899535", ChannelID.slaversID);
                    }
                    if (enemiesSlain >= 10)
                    {
                        await OutprintAsync("https://tenor.com/view/starwars-the-force-awakens-xwing-air-combat-gif-4813295", ChannelID.slaversID);
                    }
                    if (alliesSlain >= 10)
                    {
                        await OutprintAsync("https://tenor.com/view/press-f-pay-respect-coffin-burial-gif-12855021", ChannelID.slaversID);
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

                if (i % 120 == 0)
                {
                    _ = Task.Run(() => RunThroughTextAsync());
                }

                task.ticker++;
            }

            await OutprintAsync("No Longer Listening for Text Updates!", ChannelID.botCommandsID);
            runningTasks.RemoveAt(taskNum);
            dbuTaskNum--;
        }

        internal async Task FindEnemyColoniesAsync(string enemy, string folder)
        {
            //System.Console.WriteLine("Finding " + enemy + " colonies!");
            await OutprintAsync(enemy + "'s planets:", ChannelID.scoutReportsID);

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
            await OutprintFileAsync(folder + "/enemyCols.txt", ChannelID.scoutReportsID);
            File.Delete(folder + "/enemyCols.txt");
        }

        internal async Task FindListAsync(string type)
        {
            ulong channel = ChannelID.botUpdatesID;
            List<Holding> localHoldingsList = holdingsList;
            if (localHoldingsList == null)
            {
                await LoadExcelHoldingsAsync();
                localHoldingsList = holdingsList;
            }
            string tempPath = Directory.GetCurrentDirectory() + "/Temp" + type + "Dir/" + type + "Corporate.txt";

            if (!Directory.Exists(Directory.GetCurrentDirectory() + "/Temp" + type + "Dir"))
            {
                Directory.CreateDirectory(Directory.GetCurrentDirectory() + "/Temp" + type + "Dir");
            }
            await File.WriteAllTextAsync(tempPath, "");

            localHoldingsList.OrderBy(hops => hops.hopsAway);
            if (type == "Pollution")
            {
                channel = ChannelID.pollutionFinderID;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.pollution > 0 || planet.pollutionRate > 0)
                    {
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Disasters: " + planet.disaster + " | Pollution: " + planet.pollution + " + " + planet.pollutionRate + "/day" + '\n';
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "Revolt")
            {
                channel = ChannelID.revoltFinderID;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.morale < 0 || planet.moraleChange < 0.00d)
                    {
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Morale: " + planet.morale + " + " + planet.moraleChange + "/hour" + '\n';
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "Shrinking")
            {
                channel = ChannelID.shrinkingFinderID;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.popGrowth < -1d && planet.popGrowth > -2000)
                    {
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Population: " + planet.population + " | Growth Rate: " + planet.popGrowth + "/hour" + '\n';
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "Zoundsable")
            {
                channel = ChannelID.buildingID;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.population < 100000 && isZoundsable(planet.planetType, planet.discoveries))
                    {
                        string message =
                           planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | " + planet.planetType + " | Population: " + planet.population + " | Growth Rate: " + planet.popGrowth + "/hour" + '\n'
                           + "Discoveries: " + planet.discoveries + " | Building: " + planet.currentlyBuilding + " | Solar: " + planet.solarShots + " / " + planet.solarFreq + '\n'
                           + "Resources: " + '\n' + "_______________________________________" + '\n'
                           + "Metal: " + planet.ore + " | Anaerobes: " + planet.ana + " | Medicine: " + planet.med + '\n'
                           + "Organics: " + planet.org + " | Oil: " + planet.oil + " | Uranium: " + planet.ura + '\n'
                           + "Equipment: " + planet.equ + " | Spice: " + planet.spi + '\n' + '\n';

                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                }
            }
            else if (type == "All")
            {
                channel = ChannelID.colonyManagementID;
                foreach (Holding planet in localHoldingsList)
                {
                    if (planet.popGrowth < -1d && planet.popGrowth > -2000)
                    {
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Population: " + planet.population + " | Growth Rate: " + planet.popGrowth + "/hour" + '\n';
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                    if (planet.morale < 0 || planet.moraleChange < 0.00d)
                    {
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Morale: " + planet.morale + " + " + planet.moraleChange + "/hour" + '\n';
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                    if (planet.pollution > 0 || planet.pollutionRate > 0)
                    {
                        string message = planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | Disasters: " + planet.disaster + " | Pollution: " + planet.pollution + " + " + planet.pollutionRate + "/day" + '\n';
                        await UpdateCompanionFiles(planet, tempPath, message, type);
                    }
                }
            }
            else
            {
                await OutprintAsync(type + " was not recognized!", ChannelID.botErrorsID);
            }

            foreach (string file in Directory.GetFiles(Directory.GetCurrentDirectory() + "/Temp" + type + "Dir"))
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

            Directory.Delete(Directory.GetCurrentDirectory() + "/Temp" + type + "Dir");
        }

        internal async Task FindWeaponsNearMeAsync(bool nukes, bool defenses)
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

            if (nukes)
            {
                foreach (Holding holdings in holdingsList)
                {
                    if ((holdings.nukes > 0 || holdings.negotiators > 0) && holdings.hopsAway <= 5)
                    {
                        string message = holdings.location + " | " + holdings.name + " : " + holdings.hopsAway + " | Nukes: " + holdings.nukes + " | Negotiators: " + holdings.negotiators + '\n';
                        await File.AppendAllTextAsync(tempWeapons, message);
                    }
                }
            }
            else if (defenses)
            {
                foreach (Holding holdings in holdingsList)
                {
                    if ((holdings.laserCannons > 0 || holdings.compoundMines > 0 || holdings.flakCannons > 0) && holdings.hopsAway <= 5)
                    {
                        string message = holdings.location + " | " + holdings.name + " : " + holdings.hopsAway + " | Lasers: " + holdings.laserCannons + " | Compound Mines: " + holdings.compoundMines + " Flaks: " + holdings.flakCannons + '\n';
                        await File.AppendAllTextAsync(tempWeapons, message);
                    }
                }
            }

            await OutprintFileAsync(tempWeapons, ChannelID.slaversID);
            File.Delete(tempWeapons);

            Directory.Delete(Directory.GetCurrentDirectory() + "/TempWeaponsDir");
        }

        internal async Task LoadExcelHoldingsAsync()
        {
            holdingsList = new List<Holding>();
            Excel.Kill();

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
            await OutprintAsync("Excel Document Sucessfully loaded into memory!I found " + holdingsList.Count + " Colonies!", ChannelID.botUpdatesID);
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
            await OutprintAsync("Read Folders Completed!", ChannelID.botUpdatesID);
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

        private string AtUser(string line)
        {
            if (line.Contains("Autism") || line.Contains("Anxiety") || line.Contains("Anxiety.jar") || line.Contains("Freeman") || line.Contains("139536795858632705"))
            {
                return "<@139536795858632705> ";
            }
            else if (line.Contains("Avacado") || line.Contains("Archer") || line.Contains("Archie") || line.Contains("CaptArcher") || line.Contains("530669734413205505"))
            {
                return "<@530669734413205505> ";
            }
            else if (line.Contains("Banana") || line.Contains("BANANA") || line.Contains("BananaDei") || line.Contains("535618193251762176"))
            {
                return "<@535618193251762176> ";
            }
            else if (line.Contains("Dev") || line.Contains("DEV") || line.Contains("276593195767431168"))
            {
                return "<@276593195767431168> ";
            }
            else if (line.Contains("Jum") || line.Contains("JUM") || line.Contains("Jumjumbub1410") || line.Contains("941167776163323944"))
            {
                return "<@941167776163323944> ";
            }
            else if (line.Contains("lk") || line.Contains("LK") || line.Contains("leader") || line.Contains("Leader") || line.Contains("Leaderkiller") || line.Contains("429101973145387019"))
            {
                return "<@429101973145387019> ";
            }
            else if (line.Contains("muzza") || line.Contains("MUZZA") || line.Contains("Muzza") || line.Contains("Muzza269u") || line.Contains("999054521776996372"))
            {
                return "<@999054521776996372> ";
            }
            else if (line.Contains("tater") || line.Contains("Tater") || line.Contains("Taterchip") || line.Contains("969258165831106581"))
            {
                return "<@969258165831106581> ";
            }
            else
            {
                return "";
            }
        }

        private bool isZoundsable(string planetType, string research)
        {
            string[] arch2Up = new string[] { "Arch lvl 2", "Arch lvl 3", "Arch lvl 4", "Arch lvl 5" };
            string[] arch3Up = new string[] { "Arch lvl 3", "Arch lvl 4", "Arch lvl 5" };
            string[] arch4Up = new string[] { "Arch lvl 4", "Arch lvl 5" };

            if (planetType == "arctic" && arch2Up.Any(s => research.Contains(s)))
            {
                return true;
            }
            else if ((planetType == "rocky" || planetType == "greenhouse" || planetType == "Intergalactic paradise") && arch3Up.Any(s => research.Contains(s)))
            {
                return true;
            }
            else if ((planetType == "earthlike" || planetType == "volcanic" || planetType == "oceanic") && arch4Up.Any(s => research.Contains(s)))
            {
                return true;
            }
            else if ((planetType == "mountainous" || planetType == "desert") && research.Contains("Arch lvl 5"))
            {
                return true;
            }

            return false;
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
                await OutprintAsync(ex.ToString(), ChannelID.botErrorsID);
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
                    await OutprintAsync(lastLine, ChannelID.botTakingResourcesID);
                }
                else if (lastLine.Contains("tons of unidentified compounds"))
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
                else if (lastLine.Contains("Fellow corporation member"))
                {
                    await OutprintAsync(lastLine, ChannelID.slaversID);
                }

                if (kombat)
                {
                    //warped in
                    if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && (lastLine.Contains("warped into") || lastLine.Contains("entered the system")))
                    {
                        List<string> enemiesList = Diplomacy.enemies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync("@everyone " + chatLogOwner + ": " + lastLine + " in " + lastSystem, ChannelID.enemySightingsID);
                        await SayAsync(enemy + " spotted! By " + chatLogOwner + " in " + lastSystem, ChannelID.voiceSlaversOnlyID);
                    }
                    else if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("warped out"))
                    {
                        List<string> enemiesList = Diplomacy.allies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync("@everyone " + lastLine + " in " + lastSystem, ChannelID.enemySightingsID);
                        await SayAsync(enemy + " ran away cause he's a bitch nigga", ChannelID.voiceSlaversOnlyID);
                    }
                    else if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("landed on a planet"))
                    {
                        List<string> enemiesList = Diplomacy.allies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync(lastLine + " in " + lastSystem, ChannelID.enemySightingsID);
                        await SayAsync(enemy + " landed!", ChannelID.voiceSlaversOnlyID);
                    }
                    else if (Diplomacy.enemies.Any(s => lastLine.Contains(s)) && lastLine.Contains("docked"))
                    {
                        List<string> enemiesList = Diplomacy.allies.ToList<string>();
                        string enemy = enemiesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync(lastLine + " in " + lastSystem, ChannelID.enemySightingsID);
                        await SayAsync(enemy + " Re-Shielded!", ChannelID.voiceSlaversOnlyID);
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
                        else if (lastLine.Contains("You recover"))
                        {
                            enemiesSlain++;
                            if (thirdToLastLine.Contains("Defenses") && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + thirdToLastLine, ChannelID.slaversID);
                                //await OutprintAsync("Nice Job! " + ally + "'s defenses clapped " + enemy + " | " + secondToLastLine, ChannelID.alliedChatID);
                                enemiesSlain++;
                            }
                            else if (!string.IsNullOrEmpty(ally) && !string.IsNullOrEmpty(ally) && !ally.Equals(" "))
                            {
                                await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + thirdToLastLine, ChannelID.slaversID);
                                //await OutprintAsync("Nice Job! " + ally + " beat " + enemy + "'s fuckin ass" + " | " + secondToLastLine, ChannelID.alliedChatID);
                            }
                            else
                            {
                                await OutprintAsync(thirdToLastLine, ChannelID.slaversID);
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
                        await OutprintAsync(AtUser(lastLine) + lastLine, ChannelID.distressCallsID);
                        await OutprintAsync(AtUser(lastLine) + lastLine, ChannelID.recapListID);

                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/crying-meme-black-guy-cries-sad-man-thank-god-for-my-reefer-hood-news-gif-24902056 " + '\n' +
                                "WE Lost Double Dome!", ChannelID.slaversID);
                        }
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
                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                               "WE CAPTURED A DOUBLE DOME!", ChannelID.slaversID);
                        }
                        //await OutprintAsync(lastLine, ChannelID.alliedChatID);
                        await SayAsync("We've Captured a Command Post!", ChannelID.voiceSlaversOnlyID);
                        planetsKaptured++;
                    }
                    else if (lastLine.Contains("You claim ownership of the colony owned by"))
                    {
                        await OutprintAsync(secondToLastLine, ChannelID.slaversID);
                        //await OutprintAsync(secondToLastLine, ChannelID.alliedChatID);
                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", ChannelID.slaversID);
                        }
                        await SayAsync("We've Captured a Command Post!", ChannelID.voiceSlaversOnlyID);
                        planetsKaptured++;
                    }
                    else if (lastLine.Contains("It now belongs to"))
                    {
                        await OutprintAsync(fileStrArr[fileStrArr.Length - 3], ChannelID.slaversID);
                        //await OutprintAsync(fileStrArr[fileStrArr.Length - 3], ChannelID.alliedChatID);
                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", ChannelID.slaversID);
                        }
                        await SayAsync("We've Captured a Command Post!", ChannelID.voiceSlaversOnlyID);
                        planetsKaptured++;
                    }
                    else if (lastLine.Contains("For successful invasion"))
                    {
                        await OutprintAsync(fileStrArr[fileStrArr.Length - 4], ChannelID.slaversID);
                        //await OutprintAsync(fileStrArr[fileStrArr.Length - 4], ChannelID.alliedChatID);
                        if (lastLine.Contains("DD"))
                        {
                            await OutprintAsync("https://tenor.com/view/success-great-job-nice-great-success-great-gif-5586706 " + '\n' +
                                "WE CAPTURED A DOUBLE DOME!", ChannelID.slaversID);
                        }
                        await SayAsync("We've Captured a Command Post!", ChannelID.voiceSlaversOnlyID);
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
                            await OutprintAsync("@everyone " + lastLine, ChannelID.distressCallsID);
                            await OutprintFileAsync(colonyPath, ChannelID.distressCallsID);
                        }
                        else if (File.Exists(planetPath))
                        {
                            await OutprintAsync("@everyone " + lastLine, ChannelID.distressCallsID);
                            await OutprintFileAsync(planetPath, ChannelID.distressCallsID);
                        }
                        else
                        {
                            await OutprintAsync("@everyone " + lastLine, ChannelID.distressCallsID);
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
                                " | Population: " + holdingsList[holdingsIndex].population + " | Discoveries: " + holdingsList[holdingsIndex].discoveries, ChannelID.distressCallsID);
                        }
                        else
                        {
                            await OutprintAsync("Couldnt find " + planetName + " in the spreadsheet!", ChannelID.botErrorsID);
                        }
                    }
                    else if (lastLine.Contains("***") && lastLine.Contains("landed"))
                    {
                        landings++;
                        await OutprintAsync(lastLine, ChannelID.distressCallsID);
                    }
                }
                if (alerts)
                {
                    if (lastLine.Contains("Server Alert!"))
                    {
                        if (secondToLastLine.Contains("Server Alert!"))
                        {
                            await OutprintAsync("@everyone " + secondToLastLine, ChannelID.slaversID);
                        }
                        await OutprintAsync("@everyone " + lastLine, ChannelID.slaversID);
                        //await OutprintAsync("@everyone " + lastLine, ChannelID.alliedChatID);
                    }
                    else if (lastLine.Contains("U.N. Hotline"))
                    {
                        List<string> alliesList = Diplomacy.allies.ToList<string>();
                        string ally = alliesList.Find(s => lastLine.Contains(s));

                        await OutprintAsync(lastLine, ChannelID.slaversID);
                        await SayAsync(ally + " I see you!", ChannelID.voiceSlaversOnlyID);
                    }
                    else if (lastLine.Contains("Empress Allie says to Slavers"))
                    {
                        await OutprintAsync(lastLine, ChannelID.botUpdatesID);
                    }
                    else if (lastLine.Contains("due to pollution"))
                    {
                        await OutprintAsync(lastLine, ChannelID.slaversID);
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

                        await CreateCalendarEventAsync(days3, title, lastLine, ChannelID.buildingID);
                        colsAbandoned++;
                        /*
                        await Outprint(lastLine +
                            '\n' + "Add redome time to Discord Calendar Unimplemented!" +
                            '\n' + end.ToString(), ChannelID.redomeID);*/
                    }
                    //aa
                    else if (lastLine.Contains("Advanced Architecture lvl"))
                    {
                        string planetName = Algorithms.StringManipulation.GetBetween(lastLine, "on", "(");
                        int holdingsIndex = holdingsList.FindIndex(planet => planet.location == planetName);

                        if (holdingsIndex != -1)
                        {
                            Holding planet = holdingsList[holdingsIndex];
                            string message =
                                planet.location + " (" + planet.galaxyX + "," + planet.galaxyY + ")" + " | " + planet.name + " | " + planet.planetType + " | Population: " + planet.population + " | Growth Rate: " + planet.popGrowth + "/hour" + '\n'
                                 + "Discoveries: " + planet.discoveries + " | Building: " + planet.currentlyBuilding + " | Solar: " + planet.solarShots + " / " + planet.solarFreq + '\n'
                                 + "Resources: " + '\n' + "_______________________________________" + '\n'
                                 + "Metal: " + planet.ore + " | Anaerobes: " + planet.ana + " | Medicine: " + planet.med + '\n'
                                 + "Organics: " + planet.org + " | Oil: " + planet.oil + " | Uranium: " + planet.ura + '\n'
                                 + "Equipment: " + planet.equ + " | Spice: " + planet.spi + '\n';

                            await OutprintAsync(AtUser(lastLine) + lastLine + " Zounds dat hoe now!" + '\n' + message, ChannelID.buildingID);
                        }
                        else
                        {
                            await OutprintAsync("Colony was not found in the spreadsheet!", ChannelID.buildingID);
                        }

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
                                    await OutprintAsync(fileAsArr, ChannelID.botUpdatesID);

                                    await File.WriteAllTextAsync(filePaths[i], " "); //now clear it out
                                    await OutprintAsync(filePaths[i] + " cleared!", ChannelID.botUpdatesID);
                                    System.Console.WriteLine(filePaths[i] + " cleared!");
                                }
                                else if (Path.GetFileName(filePaths[i]).Equals("building.txt"))
                                {
                                    await OutprintAsync(fileAsArr, ChannelID.buildingID);

                                    await File.WriteAllTextAsync(filePaths[i], " "); //now clear it out
                                    await OutprintAsync(filePaths[i] + " cleared!", ChannelID.botUpdatesID);
                                    System.Console.WriteLine(filePaths[i] + " cleared!");
                                }
                                else if (Path.GetFileName(filePaths[i]).Equals("distress.txt"))
                                {
                                    await OutprintAsync(fileAsArr, ChannelID.distressCallsID);

                                    await File.WriteAllTextAsync(filePaths[i], " "); //now clear it out
                                    await OutprintAsync(filePaths[i] + " cleared!", ChannelID.botUpdatesID);
                                    System.Console.WriteLine(filePaths[i] + " cleared!");
                                }
                                else if (Path.GetFileName(filePaths[i]).Equals("scoutReports.txt"))
                                {
                                    await OutprintAsync(fileAsArr, ChannelID.scoutReportsID);

                                    await File.WriteAllTextAsync(filePaths[i], " "); //now clear it out
                                    await OutprintAsync(filePaths[i] + " cleared!", ChannelID.botUpdatesID);
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