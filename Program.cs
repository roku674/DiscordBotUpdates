//Created by Alexander Fields https://github.com/roku674
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBotUpdates
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            new Program().RunBotAsync().GetAwaiter().GetResult();
        }

        private static System.DateTime _dateTime;

        //nentwork variables
        private static DiscordSocketClient _client;

        private static CommandService _commands;
        private System.IServiceProvider _services;

        public static CommandService commands { get => _commands; set => _commands = value; }
        public static DiscordSocketClient client { get => _client; set => _client = value; }
        public static System.DateTime dateTime { get => _dateTime; set => _dateTime = value; }

        public async Task RunBotAsync()
        {
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string token = "OTgzNDY2ODI0ODE5Njc1MTQ2.GBMlyr.WlqZ7ccfgpPHIJm7iTfo2GbggmLR8h2nOKhb-4";

            _client.Log += _client_Log;

            await RegisterCommandsAsync();
            await _client.LoginAsync(Discord.TokenType.Bot, token);
            await _client.StartAsync();

            _dateTime = System.DateTime.Now;

            await Task.Delay(System.Threading.Timeout.Infinite);
        }

        private Task _client_Log(Discord.LogMessage arg)
        {
            System.Console.WriteLine(arg);
            return Task.CompletedTask;
        }

        public async Task RegisterCommandsAsync()
        {
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task HandleCommandAsync(SocketMessage arg)
        {
            SocketUserMessage message = arg as SocketUserMessage;
            SocketCommandContext context = new SocketCommandContext(_client, message);

            if (message.Author.IsBot) return;

            int argPos = 0;

            if (message.HasStringPrefix("$", ref argPos) && message.Channel.Id == Modules.ChannelID.botCommandsID)
            {
                IResult result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {
                    System.Console.WriteLine(result.ErrorReason + " | " + context.Message);
                    await Modules.DBUTask.Outprint(result.ErrorReason + " | " + context.Message, Modules.ChannelID.botCommandsID);
                }
            }
        }
    }
}