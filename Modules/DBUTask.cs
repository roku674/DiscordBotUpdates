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

        //one month
        private static readonly uint _duration = 2419200;

        private static uint _dbuTaskNum;
        private static List<DBUTaskObj> _runningTasks = new List<DBUTaskObj>();

        //getters/setters
        public static uint duration => _duration;

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
            await OutprintAsync(AtUser(title) + message + '\n'
                + title + '\n'
                + "Redome Time: " + startTime.ToString() + " EST", Objects.ChannelID.redomeId);
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
                Discord.IMessageChannel channel = Program.client.GetChannel(Objects.ChannelID.botUpdatesId) as Discord.IMessageChannel;
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
                    Discord.IMessageChannel channel = Program.client.GetChannel(Objects.ChannelID.botUpdatesId) as Discord.IMessageChannel;
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
                    Discord.IMessageChannel channel = Program.client.GetChannel(Objects.ChannelID.botUpdatesId) as Discord.IMessageChannel;
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

        public static string AtUser(string line)
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
            else if (line.Contains("Dev") || line.Contains("DEV") || line.Contains("276593195767431168") || line.Contains("Devila") || line.Contains("devila"))
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
    }
}