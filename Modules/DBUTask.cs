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

        public static async Task CreateCalendarEvent(System.DateTime start, System.DateTime end, string summary, ulong channelId)
        {
            Discord.IMessageChannel channel = Program.client.GetChannel(channelId) as Discord.IMessageChannel;
            await Outprint("Calendar Updater Unimplemented!", channelId);
            //channel.
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

        public static async Task Outprint(string message, ulong channelId)
        {
            System.Console.WriteLine(message);

            Discord.IMessageChannel channel = Program.client.GetChannel(channelId) as Discord.IMessageChannel;
            await channel.SendMessageAsync(message);
        }

        public static async Task Say(string message, ulong channelId)
        {
            System.Console.WriteLine(message);

            Discord.IVoiceChannel channel = Program.client.GetChannel(channelId) as Discord.IVoiceChannel;
            await channel.SendMessageAsync(message, isTTS: true);
        }
    }
}