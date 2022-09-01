//Created by Alexander Fields https://github.com/roku674
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiscordBotUpdates.Modules
{
    internal class DBUTask
    {
        public class DBUTaskObj
        {
            internal bool isCancelled = false;

            public DBUTaskObj(Task _task, string _purpose, string _owner, uint _id, System.DateTime? _timeStarted)
            {
                if (_timeStarted != null)
                {
                    timeStarted = (System.DateTime)_timeStarted;
                }
                else
                {
                    timeStarted = System.DateTime.Now;
                }

                id = _id;
                owner = _owner;
                purpose = _purpose;
                task = _task;
            }

            public uint id { get; set; }
            public string owner { get; set; }
            public string purpose { get; set; }
            public Task task { get; set; }
            public uint ticker { get; set; }
            public System.DateTime timeStarted { get; set; }

            public void Cancel()
            {
                isCancelled = true;
            }
        }

        //var
        private static readonly int _duration = 604800;

        private static uint _dbuTaskNum;
        private static List<DBUTaskObj> _runningTasks = new List<DBUTaskObj>();

        //getters/setters
        public static int duration => _duration;

        public static uint dbuTaskNum { get => _dbuTaskNum; set => _dbuTaskNum = value; }
        public static List<DBUTaskObj> runningTasks { get => _runningTasks; set => _runningTasks = value; }

        public static async Task CreateCalendarEventAsync(System.DateTime startTime, string title, string description, ulong channelId)
        {
            Google.Apis.Calendar.v3.EventsResource.ListRequest request = Program.service.Events.List("primary");
            request.TimeMin = System.DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.OrderBy = Google.Apis.Calendar.v3.EventsResource.ListRequest.OrderByEnum.StartTime;

            Google.Apis.Calendar.v3.Data.Event ev = new Google.Apis.Calendar.v3.Data.Event();
            Google.Apis.Calendar.v3.Data.EventDateTime start = new Google.Apis.Calendar.v3.Data.EventDateTime();
            Google.Apis.Calendar.v3.Data.EventDateTime end = new Google.Apis.Calendar.v3.Data.EventDateTime();

            start.DateTime = startTime;
            end.DateTime = startTime + new System.TimeSpan(0, 5, 0);

            ev.Start = start;
            ev.End = end;
            ev.Summary = title;
            ev.Description = description;

            string calendarId = "primary";
            _ = Program.service.Events.Insert(ev, calendarId).Execute();

            System.Random random = new System.Random();
            int num = random.Next(1, 4);  // creates a number between 1 and 12

            string message = "";
            switch (num)
            {
                case 1:
                    message = "Redome Calendar Event Posted Better show up to redome it before Niggas with Oranges!";
                    System.Console.WriteLine(message);
                    break;

                case 2:
                    message = "Redome Calendar Event Posted!";
                    System.Console.WriteLine(message);
                    break;

                case 3:
                    message = "I added a redome to your calendar!";
                    System.Console.WriteLine(message);
                    break;

                default:
                    message = "Redome Calendar Event Posted!";
                    System.Console.WriteLine(message);
                    break;
            }
            await OutprintAsync(message + '\n'
                + title + '\n'
                + "Redome Time: " + startTime.ToString() + " EST", Objects.ChannelID.redomeID);
        }

        public static async Task CelebrateUser(string title, string message, ulong channelId)
        {
            Discord.IMessageChannel channel = Program.client.GetChannel(channelId) as Discord.IMessageChannel;

            Discord.EmbedBuilder embedBuilder = new Discord.EmbedBuilder()
            {
                Title = title,
                Color = Discord.Color.Green,
                ThumbnailUrl = "https://tenor.com/view/cat-shooting-mouth-open-gif-15017033",
                Footer =
                {
                IconUrl = "https://icons8.com/icon/YV_2mLwXMWyM/futurama-bender",
                Text = message
                },
                Timestamp = System.DateTimeOffset.Now
            };
            Discord.Embed embeded = embedBuilder.Build();
            await channel.SendMessageAsync("", embed: embeded);
        }

        public static async Task OutprintAsync(string message, ulong channelId)
        {
            System.Console.WriteLine(message);
            if (message.Contains("shouts") || message.Contains("radios") || message.Contains("tells"))
            {
                Discord.IMessageChannel channel = Program.client.GetChannel(Objects.ChannelID.botUpdatesID) as Discord.IMessageChannel;
                await channel.SendMessageAsync(message);
            }
            else
            {
                Discord.IMessageChannel channel = Program.client.GetChannel(channelId) as Discord.IMessageChannel;
                await channel.SendMessageAsync(message);
            }
        }

        public static async Task OutprintAsync(string[] messages, ulong channelId)
        {
            for (int i = 0; i < messages.Length; i++)
            {
                System.Console.WriteLine(messages[i]);
                if (messages[i].Contains("shouts") || messages[i].Contains("radios") || messages[i].Contains("tells"))
                {
                    Discord.IMessageChannel channel = Program.client.GetChannel(Objects.ChannelID.botUpdatesID) as Discord.IMessageChannel;
                    await channel.SendMessageAsync(messages[i]);
                }
                else
                {
                    Discord.IMessageChannel channel = Program.client.GetChannel(channelId) as Discord.IMessageChannel;
                    await channel.SendMessageAsync(messages[i]);
                }
            }
        }

        public static async Task OutprintAsync(List<string> messages, ulong channelId)
        {
            for (int i = 0; i < messages.Count; i++)
            {
                System.Console.WriteLine(messages[i]);
                if (messages[i].Contains("shouts") || messages[i].Contains("radios") || messages[i].Contains("tells"))
                {
                    Discord.IMessageChannel channel = Program.client.GetChannel(Objects.ChannelID.botUpdatesID) as Discord.IMessageChannel;
                    await channel.SendMessageAsync(messages[i]);
                }
                else
                {
                    Discord.IMessageChannel channel = Program.client.GetChannel(channelId) as Discord.IMessageChannel;
                    await channel.SendMessageAsync(messages[i]);
                }
            }
        }

        public static async Task OutprintFileAsync(string path, ulong channelId)
        {
            Discord.IMessageChannel channel = Program.client.GetChannel(channelId) as Discord.IMessageChannel;
            await channel.SendFileAsync(path, System.IO.Path.GetFileName(path));
        }

        public static async Task SayAsync(string message, ulong channelId)
        {
            System.Console.WriteLine(message);

            Discord.IVoiceChannel channel = Program.client.GetChannel(channelId) as Discord.IVoiceChannel;
            await channel.SendMessageAsync(message, isTTS: true);
        }
    }
}