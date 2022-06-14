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

            public DBUTaskObj(Task _task, string _purpose, string _owner, uint _id)
            {
                timeStarted = System.DateTime.Now;

                id = _id;
                owner = _owner;
                purpose = _purpose;
                task = _task;
            }

            public uint id { get; set; }
            public string owner { get; set; }
            public string purpose { get; set; }
            public Task task { get; set; }
            public System.DateTime timeStarted { get; set; }

            public void Cancel()
            {
                isCancelled = true;
            }
        }

        private static readonly int _duration = 604800;

        private static uint _dbuTaskNum;
        private static List<DBUTaskObj> _runningTasks = new List<DBUTaskObj>();

        private string _dbuString = "";

        public static int duration => _duration;

        public static uint dbuTaskNum { get => _dbuTaskNum; set => _dbuTaskNum = value; }
        public static List<DBUTaskObj> runningTasks { get => _runningTasks; set => _runningTasks = value; }

        public string dbuString { get => _dbuString; set => _dbuString = value; }

        public static async Task CreateCalendarEvent(System.DateTime start, System.DateTime end, string summary, ulong channelId)
        {
            Discord.IMessageChannel channel = Program.client.GetChannel(channelId) as Discord.IMessageChannel;
            await Outprint("Calendar Updater Unimplemented!", channelId);
            //channel.
        }

        public static async Task CelebrateUser(string title, string message, ulong channelId)
        {
            Discord.IMessageChannel channel = Program.client.GetChannel(channelId) as Discord.IMessageChannel;

            Discord.EmbedBuilder embed = new Discord.EmbedBuilder()
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
            await channel.SendMessageAsync("", embed: embed.Build());
        }

        public static async Task Outprint(string message, ulong channelId)
        {
            Discord.IMessageChannel channel = Program.client.GetChannel(channelId) as Discord.IMessageChannel;
            await channel.SendMessageAsync(message);
        }

        public static async Task Say(string message, ulong channelId)
        {
            Discord.IVoiceChannel channel = Program.client.GetChannel(channelId) as Discord.IVoiceChannel;

            await channel.SendMessageAsync(message, true);
        }
    }
}