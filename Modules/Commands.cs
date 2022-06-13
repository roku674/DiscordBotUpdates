﻿//Created by Alexander Fields https://github.com/roku674
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DiscordBotUpdates.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private string[] botNames = { "Allie", "Bitcoin", "Probation", "Towlie" };
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
                "    run Echo (BotName)" +
                '\n' +
                "    run ListenerChatLog" +
                '\n' +
                "    run ListenerDistress" +
                '\n' +
                "    run ListenerStarportServerReset" +
                '\n' +
                "    run StopAllTasks" +
                '\n' +
                "    run StopClientTasks" +
                '\n' +
                "    run StopListenerDistress" +
                '\n' +
                "    run StopListenerStarporServerReset" +
                '\n' +
                "    run StopTasks (BotName or client)"
                );
        }

        [Command("run AllTasks")]
        public async Task AllTasks()
        {
            await Task.Run(() => BotUpdaterPost());

            await Task.Run(() => ListenerDistressPost());
            await Task.Run(() => ListenerStarportServerResetPost());

            await Task.Run(() => ListenerChatLogPost());
        }

        [Command("run BotUpdater")]
        public async Task BotUpdaterPost()
        {
            await ReplyAsync("By Your Command! Listening for messages and pictures for " + TaskInitator.duration + " seconds!");

            uint messagesNum = DBUTask.dbuTaskNum++;
            Task messages = Task.Run(() => init.MessageBotUpdates(messagesNum, ChannelID.botUpdatesID));

            uint picturesNum = DBUTask.dbuTaskNum++;
            Task pictures = Task.Run(() => init.PictureBotUpdates(picturesNum, ChannelID.botUpdatesID));

            DBUTask.DBUTaskObj dBUMessages = new DBUTask.DBUTaskObj(messages, "Message Updater", "Client", messagesNum);
            DBUTask.DBUTaskObj dBUPicturess = new DBUTask.DBUTaskObj(pictures, "Picture Updater", "Client", picturesNum);

            DBUTask.runningTasks.Add(dBUMessages);
            DBUTask.runningTasks.Add(dBUPicturess);
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
            if (botNames.Any(s => text.Contains(s)))
            {
                List<string> botNamesList = botNames.ToList<string>();

                string bot = botNamesList.Find(s => text.Contains(s));
                string botEcho = text.Replace(bot + " ", "");

                if (File.Exists(Directory.GetCurrentDirectory() + "/Echo/" + bot + ".txt"))
                {
                    await File.AppendAllTextAsync(Directory.GetCurrentDirectory() + "/Echo/" + bot + ".txt", botEcho);
                }
                else
                {
                    File.Create(Directory.GetCurrentDirectory() + "/Echo/" + bot + ".txt");
                    await ReplyAsync("Created" + bot + ".txt ! Recommend ReRunning!");
                }

                await ReplyAsync(bot + " will say: " + botEcho);
            }
        }

        [Command("request Listeners")]
        public async Task ListenerActivityGet()
        {
            await ReplyAsync("Distress: " + TaskInitator.distress +
                '\n' +
                "Server Reset: " + TaskInitator.serverReset);
        }

        [Command("run ListenerChatLog")]
        public async Task ListenerChatLogPost()
        {
            await ReplyAsync("By Your Command! Listening for Chatlog changes!");

            uint listenerNum = DBUTask.dbuTaskNum++;
            Task listener = Task.Run(() => init.ChatLogListener(listenerNum, ChannelID.botUpdatesID));

            DBUTask.DBUTaskObj dbuListener = new DBUTask.DBUTaskObj(listener, "Chat Log Listener", "Client", listenerNum);
            DBUTask.runningTasks.Add(dbuListener);
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
            TimeSpan lifetime = DateTime.Now - Program.dateTime;
            await ReplyAsync("Initiated at " + Program.dateTime + " EST. Has been running for: " +
                '\n' +
                lifetime.Hours + " Hours " +
                '\n' +
                lifetime.Minutes + " Minutes " +
                '\n' +
                lifetime.Seconds + " Seconds"
                );
        }

        [Command("run ListenerDistress")]
        public async Task ListenerDistressPost()
        {
            await Task.Run(() => init.SetDistress(true));
            await ReplyAsync("By Your Command! Listening for Distress Signals");
        }

        [Command("run ListenerStarportServerReset")]
        public async Task ListenerStarportServerResetPost()
        {
            await Task.Run(() => init.SetServerReset(true));
            await ReplyAsync("By Your Command! Listening for Server Resets");
        }

        [Command("request RunningTasks")]
        public async Task RunningTasksGet()
        {
            if (DBUTask.runningTasks.Count > 0)
            {
                foreach (DBUTask.DBUTaskObj task in DBUTask.runningTasks)
                {
                    await ReplyAsync("TaskID: " + task.task.Id + " | " + "Task Purpose: " + task.purpose + " | Task Owner: " + task.owner + " | Initiated at " + task.timeStarted);
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

                    await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted
                        + '\n'
                        + "Was ended at " + DateTime.Now);

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

                    List<string> botNamesList = botNames.ToList<string>();
                    string bot = botNamesList.Find(s => text.Contains(s));

                    if (dbuTask.owner.Equals(bot))
                    {
                        await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted
                            + '\n'
                            + "Was ended at " + DateTime.Now);

                        dbuTask.Cancel();
                        DBUTask.runningTasks.Remove(dbuTask);
                    }
                    else if (dbuTask.owner.Equals("Client"))
                    {
                        await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted
                            + '\n'
                            + "Was ended at " + DateTime.Now);

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

        [Command("run StopListenerDistress")]
        public async Task StopListenerDistress()
        {
            await Task.Run(() => init.SetDistress(false));
            await ReplyAsync("By Your Command! Stopped Listening for Distress Signals");
        }

        [Command("run StopListenerStarportServerReset")]
        public async Task StopListenerServerReset()
        {
            await Task.Run(() => init.SetServerReset(false));
            await ReplyAsync("By Your Command! Stopped Listening for Server Resets");
        }
    }
}