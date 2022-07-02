//Created by Alexander Fields https://github.com/roku674
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
        private string[] cylons = { "Allie", "Bitcoin", "Probation", "Towlie" };
        private string[] listenerNames = { "building", "distress", "kombat", "serverResets", "All" };
        private TaskInitator init = new TaskInitator();

        [Command("Ping")]
        public async Task Ping()
        {
            await ReplyAsync("Pong");
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
                "    request Lifetime" +
                '\n' +
                "    request Listeners" +
                '\n' +
                "    request RunningTasks" +
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
                "    request planetTallies" +
                 '\n' +
                "    run planetsCaptured #" +
                 '\n' +
                "    run planetsLost #" +
                '\n' +
                "    run StopAllTasks" +
                '\n' +
                "    run StopClientTasks" +
                '\n' +
                "    run StopListener (Listener type)" +
                '\n' +
                "    run StopTasks (BotName or client)"
                );
        }

        [Command("run AllTasks")]
        public async Task AllTasks()
        {
            _ = Task.Run(() => BotUpdaterPost());
            _ = Task.Run(() => ListenerPost("All"));
            _ = Task.Run(() => ListenerChatLogPost("All"));

            await Task.Delay(33);
        }

        [Command("run BotUpdater")]
        public async Task BotUpdaterPost()
        {
            await ReplyAsync("By Your Command! Listening for messages and pictures for " + DBUTask.duration + " seconds!");

            await MessageUpdater(ChannelID.botUpdatesID, "Message Updater", "Client", "botUpdates");
            //await MessageUpdater(ChannelID.distressCallsID, "Distress Signal Updater", "Client", "distress");
            //await MessageUpdater(ChannelID.slaversID, "Warped In & Out Updater", "Client", "warpedInOut");

            await PictureUpdater(ChannelID.botUpdatesID, "Picture Updater", "Client");
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

        [Command("request Info")]
        public async Task InfoGet()
        {
            await DBUTask.OutprintAsync("We Lost: " + TaskInitator.planetsLost + '\n'
                        + "We Kaptured: " + TaskInitator.planetsKaptured + '\n'
                        + "Allies Slain: " + TaskInitator.alliesSlain + '\n'
                        + "Enemies Slain: " + TaskInitator.enemiesSlain + '\n'
                        + "landings: " + TaskInitator.landings + '\n'
                        + "Colonies Abanonded: " + TaskInitator.colsAbandoned, ChannelID.botCommandsID);
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

            await InfoGet();
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
                foreach (string botName in cylons)
                {
                    await ChatLogListener(ChannelID.botUpdatesID, "Chat Log Listener", botName);
                }
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

        [Command("run Deactivate")]
        public async Task DeactivateProgramPost()
        {
            await ReplyAsync("Client will be stopped now...");
            await Program.client.StopAsync();
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

        [Command("request planetTallies")]
        public async Task PlanetTalliesGet()
        {
            await DBUTask.OutprintAsync(
                "We Lauwst: " + TaskInitator.planetsLost + '\n'
                + "We Kaptured: " + TaskInitator.planetsKaptured, ChannelID.slaversID);
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

        [Command("run StopAllTasks")]
        public async Task StopAllTasks()
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
        public async Task StopBotTasks([Remainder] string text)
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

        [Command("run StopListener")]
        public async Task StopListener([Remainder] string text)
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

        private async Task ChatLogListener(ulong channelID, string purpose, string owner)
        {
            uint listenerNum = DBUTask.dbuTaskNum++;
            Task task = Task.Run(() => init.ChatLogListenerAsync(listenerNum, channelID, owner));
            DBUTask.runningTasks.Add(new DBUTask.DBUTaskObj(task, purpose, owner, listenerNum, null));
            await Task.Delay(500);
        }

        private async Task PictureUpdater(ulong channelID, string purpose, string owner)
        {
            uint picturesNum = DBUTask.dbuTaskNum++;
            Task task = Task.Run(() => init.PictureUpdaterAsync(picturesNum, channelID));
            DBUTask.runningTasks.Add(new DBUTask.DBUTaskObj(task, purpose, owner, picturesNum, null));
            await Task.Delay(500);
        }

        private async Task MessageUpdater(ulong channelID, string purpose, string owner, string type)
        {
            uint id = DBUTask.dbuTaskNum++;
            Task task = Task.Run(() => init.TextUpdaterAsync(id, channelID, type));
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