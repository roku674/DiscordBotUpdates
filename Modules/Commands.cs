//Created by Alexander Fields https://github.com/roku674
using Discord.Commands;
using System;
using System.Threading.Tasks;

namespace DiscordBotUpdates.Modules
{
    public class Commands : ModuleBase<SocketCommandContext>
    {
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
                "    run StopAllTasks" +
                '\n' +
                "    run Stop(BotName)Tasks" +
                '\n' +
                "    run StopServerTasks"

                );
        }

        [Command("run BotUpdater")]
        public async Task BotUpdatterPost()
        {
            await ReplyAsync("By Your Command! Listening for messages and pictures for " + BotUpdater.duration + " seconds!");

            BotUpdater botUpdater = new BotUpdater();

            uint messagesNum = DBUTask.dbuTaskNum++;
            Task messages = Task.Run(() => botUpdater.MessageBotUpdates(messagesNum));

            uint picturesNum = DBUTask.dbuTaskNum++;
            Task pictures = Task.Run(() => botUpdater.PictureBotUpdates(picturesNum));

            DBUTask.DBUTaskObj dBUMessages = new DBUTask.DBUTaskObj(messages, "Message Updater", "Server", messagesNum);
            DBUTask.DBUTaskObj dBUPicturess = new DBUTask.DBUTaskObj(pictures, "Picture Updater", "Server", picturesNum);

            BotUpdater.runningTasks.Add(dBUMessages);
            BotUpdater.runningTasks.Add(dBUPicturess);
        }

        [Command("run Deactivate")]
        public async Task DeactivatePost()
        {
            await ReplyAsync("Client will be stopped now...");
            await Program.client.StopAsync();
        }

        [Command("run StopAllTasks")]
        public async Task StopAllTasks()
        {
            if (BotUpdater.runningTasks.Count > 0)
            {
                for (int i = BotUpdater.runningTasks.Count - 1; i >= 0; i--)
                {
                    DBUTask.DBUTaskObj dbuTask = BotUpdater.runningTasks[i];

                    await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted
                        + '\n'
                        + "Was ended at " + DateTime.Now);

                    dbuTask.Cancel();
                    BotUpdater.runningTasks.Remove(dbuTask);
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
            if (BotUpdater.runningTasks.Count > 0)
            {
                for (int i = BotUpdater.runningTasks.Count - 1; i >= 0; i--)
                {
                    DBUTask.DBUTaskObj dbuTask = BotUpdater.runningTasks[i];

                    if (dbuTask.owner.Equals("Allie"))
                    {
                        await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted
                            + '\n'
                            + "Was ended at " + DateTime.Now);

                        dbuTask.Cancel();
                        BotUpdater.runningTasks.Remove(dbuTask);
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
            if (BotUpdater.runningTasks.Count > 0)
            {
                for (int i = BotUpdater.runningTasks.Count - 1; i >= 0; i--)
                {
                    DBUTask.DBUTaskObj dbuTask = BotUpdater.runningTasks[i];

                    if (dbuTask.owner.Equals("Probation"))
                    {
                        await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted
                            + '\n'
                            + "Was ended at " + DateTime.Now);

                        dbuTask.Cancel();
                        BotUpdater.runningTasks.Remove(dbuTask);
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
            if (BotUpdater.runningTasks.Count > 0)
            {
                for (int i = BotUpdater.runningTasks.Count - 1; i >= 0; i--)
                {
                    DBUTask.DBUTaskObj dbuTask = BotUpdater.runningTasks[i];

                    if (dbuTask.owner.Equals("Server"))
                    {
                        await ReplyAsync("TaskID: " + dbuTask.task.Id + " | " + "Task Purpose: " + dbuTask.purpose + " | Task Owner: " + dbuTask.owner + " | Initiated at " + dbuTask.timeStarted
                            + '\n'
                            + "Was ended at " + DateTime.Now);

                        dbuTask.Cancel();
                        BotUpdater.runningTasks.Remove(dbuTask);
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
            if (BotUpdater.runningTasks.Count > 0)
            {
                foreach (DBUTask.DBUTaskObj task in BotUpdater.runningTasks)
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