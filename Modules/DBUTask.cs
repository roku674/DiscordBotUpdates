using Discord;
using Discord.WebSocket;
using System;
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
                timeStarted = DateTime.Now;

                id = _id;
                owner = _owner;
                purpose = _purpose;
                task = _task;
            }

            public uint id { get; set; }
            public string owner { get; set; }
            public string purpose { get; set; }
            public Task task { get; set; }
            public DateTime timeStarted { get; set; }

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

        public static async Task CalendarEvent(Schedule schedule)
        {
            var embed = new EmbedBuilder();
            embed.WithTitle(schedule.Title);
            embed.AddField("Event Description: ", schedule.Description);
            embed.AddField("Author: ", MentionUtils.MentionUser(schedule.Author), true);
            embed.AddField("Who can join: ", schedule.Role != 0 ? MentionUtils.MentionRole(schedule.Role) : "Everyone", true);
            embed.AddField("Start Date:", schedule.StartTime.ToString("dd/MM/yyyy H:mm"), true);
            embed.AddField("Maximum capacity: ", schedule.MaxMembers, true);
            embed.AddField($"Assistants ({schedule.Members.Count}/{schedule.MaxMembers}) : ", members.ToString());
            embed.AddField("Party ID: ", schedule.Id);
            embed.WithFooter($"React with {NewMemberEmoji} to join. Created on {schedule.CreatedOn:dd/MM/yyyy H:mm}");
            embed.WithColor(new Color(52, 152, 219));
            embed.WithCurrentTimestamp();
        }

        public static async Task Outprint(string message, ulong channelId)
        {
            IMessageChannel channel = Program.client.GetChannel(channelId) as IMessageChannel;
            await channel.SendMessageAsync(message);
        }

        public static async Task Say(string message, ulong channelId)
        {
            IVoiceChannel channel = Program.client.GetChannel(channelId) as IVoiceChannel;

            await channel.SendMessageAsync(message, true);
        }
    }
}