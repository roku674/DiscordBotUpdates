//Created by Alexander Fields https://github.com/roku674
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace DiscordBotUpdates.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
        private TaskInitiater init = new TaskInitiater();

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
                "    request RunningTasks" +
                '\n' + '\n' +

                "  Posts: " +
                '\n' +
                "    run BotUpdater" +
                '\n' +
                "    run Deactivate" +
                '\n' +
                "    run DistressListener" +
                '\n' +
                "    run StarportResetListener" +
                '\n' +
                "    run StopAllTasks" +
                '\n' +
                "    run Stop(BotName)Tasks" +
                '\n' +
                "    run StopServerTasks"

                );
        }

        [Command("run BotUpdater")]
        public async Task BotUpdaterPost()
        {
            await ReplyAsync("By Your Command! Listening for messages and pictures for " + TaskInitiater.duration + " seconds!");

            uint messagesNum = DBUTask.dbuTaskNum++;
            Task messages = Task.Run(() => init.MessageBotUpdates(messagesNum, ChannelID.botUpdatesID));

            uint picturesNum = DBUTask.dbuTaskNum++;
            Task pictures = Task.Run(() => init.PictureBotUpdates(picturesNum, ChannelID.botUpdatesID));

            DBUTask.DBUTaskObj dBUMessages = new DBUTask.DBUTaskObj(messages, "Message Updater", "Server", messagesNum);
            DBUTask.DBUTaskObj dBUPicturess = new DBUTask.DBUTaskObj(pictures, "Picture Updater", "Server", picturesNum);

            DBUTask.runningTasks.Add(dBUMessages);
            DBUTask.runningTasks.Add(dBUPicturess);
        }

        [Command("run Deactivate")]
        public async Task DeactivateProgramPost()
        {
            await ReplyAsync("Client will be stopped now...");
            await Program.client.StopAsync();
        }

        [Command("run DistressListener")]
        public async Task DistressListenerPost()
        {
            await ReplyAsync("By Your Command! Listening for Distress Signals");

            init = new TaskInitiater();
        }

        [Command("run StarportResetListener")]
        public async Task StarportResetListenerPost()
        {
            await ReplyAsync("By Your Command! Listening for Server Resets");
        }

        [Command("run StopAllTasks")]
        public async Task StopAllTasks()
        {
            if (TaskInitiater.runningTasks.Count > 0)
            {
                for (int i = TaskInitiater.runningTasks.Count - 1; i >= 0; i--)
                {
                    DBUTask.DBUTaskObj dbuTask = TaskInitiater.runningTasks[i];

                    await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted
                        + '\n'
                        + "Was ended at " + DateTime.Now);

                    dbuTask.Cancel();
                    TaskInitiater.runningTasks.Remove(dbuTask);
                }
            }
            else
            {
                await ReplyAsync("There are no tasks to stop!");
            }
        }

        [Command("run StopAllieTasks")]
        public async Task StopAllieTasks()
        {
            if (TaskInitiater.runningTasks.Count > 0)
            {
                for (int i = TaskInitiater.runningTasks.Count - 1; i >= 0; i--)
                {
                    DBUTask.DBUTaskObj dbuTask = TaskInitiater.runningTasks[i];

                    if (dbuTask.owner.Equals("Allie"))
                    {
                        await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted
                            + '\n'
                            + "Was ended at " + DateTime.Now);

                        dbuTask.Cancel();
                        TaskInitiater.runningTasks.Remove(dbuTask);
                    }
                }
            }
            else
            {
                await ReplyAsync("There are no tasks to stop!");
            }
        }

        [Command("run StopProbationTasks")]
        public async Task StopProbationTasks()
        {
            if (TaskInitiater.runningTasks.Count > 0)
            {
                for (int i = TaskInitiater.runningTasks.Count - 1; i >= 0; i--)
                {
                    DBUTask.DBUTaskObj dbuTask = TaskInitiater.runningTasks[i];

                    if (dbuTask.owner.Equals("Probation"))
                    {
                        await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted
                            + '\n'
                            + "Was ended at " + DateTime.Now);

                        dbuTask.Cancel();
                        TaskInitiater.runningTasks.Remove(dbuTask);
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
            if (TaskInitiater.runningTasks.Count > 0)
            {
                for (int i = TaskInitiater.runningTasks.Count - 1; i >= 0; i--)
                {
                    DBUTask.DBUTaskObj dbuTask = TaskInitiater.runningTasks[i];

                    if (dbuTask.owner.Equals("Server"))
                    {
                        await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted
                            + '\n'
                            + "Was ended at " + DateTime.Now);

                        dbuTask.Cancel();
                        TaskInitiater.runningTasks.Remove(dbuTask);
                    }
                }
            }
            else
            {
                await ReplyAsync("There are no tasks to stop!");
            }
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

        [Command("request RunningTasks")]
        public async Task RunningTasksGet()
        {
            if (TaskInitiater.runningTasks.Count > 0)
            {
                foreach (DBUTask.DBUTaskObj task in TaskInitiater.runningTasks)
                {
                    await ReplyAsync("TaskID: " + task.task.Id + " | " + "Task Purpose: " + task.purpose + " | Task Owner: " + task.owner + " | Initiated at " + task.timeStarted);
                }
            }
            else
            {
                await ReplyAsync("No Tasks are currently Running!");
            }
        }
    }
}