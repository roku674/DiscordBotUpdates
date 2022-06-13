//Created by Alexander Fields https://github.com/roku674
using Discord;
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace DiscordBotUpdates.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
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
                "    run Deactivate" +
                '\n' +
                "    run ListenerChatLog" +
                '\n' +
                "    run ListenerDistress" +
                '\n' +
                "    run ListenerStarportServerReset" +
                '\n' +
                "    run StopAllTasks" +
                '\n' +
                "    run StopListenerDistress" +
                '\n' +
                "    run StopListenerStarporServerReset" +
                '\n' +
                "    run Stop(BotName)Tasks" +
                '\n' +
                "    run StopServerTasks"

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

            DBUTask.DBUTaskObj dBUMessages = new DBUTask.DBUTaskObj(messages, "Message Updater", "Server", messagesNum);
            DBUTask.DBUTaskObj dBUPicturess = new DBUTask.DBUTaskObj(pictures, "Picture Updater", "Server", picturesNum);

            DBUTask.runningTasks.Add(dBUMessages);
            DBUTask.runningTasks.Add(dBUPicturess);
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

            DBUTask.DBUTaskObj dbuListener = new DBUTask.DBUTaskObj(listener, "Chat Log Listener", "Server", listenerNum);
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
            if (TaskInitator.runningTasks.Count > 0)
            {
                foreach (DBUTask.DBUTaskObj task in TaskInitator.runningTasks)
                {
                    await ReplyAsync("TaskID: " + task.task.Id + " | " + "Task Purpose: " + task.purpose + " | Task Owner: " + task.owner + " | Initiated at " + task.timeStarted);
                }
            }
            else
            {
                await ReplyAsync("No Tasks are currently Running!");
            }
        }

        [Command("run StopAllieTasks")]
        public async Task StopAllieTasks()
        {
            if (TaskInitator.runningTasks.Count > 0)
            {
                for (int i = TaskInitator.runningTasks.Count - 1; i >= 0; i--)
                {
                    DBUTask.DBUTaskObj dbuTask = TaskInitator.runningTasks[i];

                    if (dbuTask.owner.Equals("Allie"))
                    {
                        await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted
                            + '\n'
                            + "Was ended at " + DateTime.Now);

                        dbuTask.Cancel();
                        TaskInitator.runningTasks.Remove(dbuTask);
                    }
                }
            }
            else
            {
                await ReplyAsync("There are no tasks to stop!");
            }
        }

        [Command("run StopAllTasks")]
        public async Task StopAllTasks()
        {
            if (TaskInitator.runningTasks.Count > 0)
            {
                for (int i = TaskInitator.runningTasks.Count - 1; i >= 0; i--)
                {
                    DBUTask.DBUTaskObj dbuTask = TaskInitator.runningTasks[i];

                    await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted
                        + '\n'
                        + "Was ended at " + DateTime.Now);

                    dbuTask.Cancel();
                    TaskInitator.runningTasks.Remove(dbuTask);
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

        [Command("run StopProbationTasks")]
        public async Task StopProbationTasks()
        {
            if (TaskInitator.runningTasks.Count > 0)
            {
                for (int i = TaskInitator.runningTasks.Count - 1; i >= 0; i--)
                {
                    DBUTask.DBUTaskObj dbuTask = TaskInitator.runningTasks[i];

                    if (dbuTask.owner.Equals("Probation"))
                    {
                        await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted
                            + '\n'
                            + "Was ended at " + DateTime.Now);

                        dbuTask.Cancel();
                        TaskInitator.runningTasks.Remove(dbuTask);
                    }
                }
            }
            else
            {
                await ReplyAsync("There are no tasks to stop!");
            }
        }

        [Command("run StopServerTasks")]
        public async Task StopServerTasks()
        {
            if (TaskInitator.runningTasks.Count > 0)
            {
                for (int i = TaskInitator.runningTasks.Count - 1; i >= 0; i--)
                {
                    DBUTask.DBUTaskObj dbuTask = TaskInitator.runningTasks[i];

                    if (dbuTask.owner.Equals("Server"))
                    {
                        await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted
                            + '\n'
                            + "Was ended at " + DateTime.Now);

                        dbuTask.Cancel();
                        TaskInitator.runningTasks.Remove(dbuTask);
                    }
                }
            }
            else
            {
                await ReplyAsync("There are no tasks to stop!");
            }
        }
    }
}