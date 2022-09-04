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
            Google.Apis.Auth.OAuth2.UserCredential credential;

            using (System.IO.FileStream stream =
                new System.IO.FileStream(System.IO.Directory.GetCurrentDirectory() + "/client_secret.json", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = System.IO.Directory.GetCurrentDirectory() + "/token.json";

                credential = Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(
                    Google.Apis.Auth.OAuth2.GoogleClientSecrets.FromStream(stream).Secrets,
                    scopes,
                    "user",
                    System.Threading.CancellationToken.None,
                    new Google.Apis.Util.Store.FileDataStore(credPath, true)).Result;
                System.Console.WriteLine("Credential file saved to: " + credPath);
            }

            _service = new Google.Apis.Calendar.v3.CalendarService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
            //was clearing out the wrong file names

            /*
            string[] files = System.IO.Directory.GetFiles("G:/My Drive/Personal Stuff/Starport/PlanetPictures/Enemy Planets", "*", System.IO.SearchOption.AllDirectories);
            foreach (string file in files)
            {
                string newName = System.IO.Path.GetFileName(file);
                newName = Algorithms.StringManipulation.RemoveDuplicates(newName);

                System.Console.WriteLine(System.IO.Path.GetFileName(file) + '\n' + newName);
                System.IO.DirectoryInfo parentDir = System.IO.Directory.GetParent(file);
                if (System.IO.File.Exists(parentDir.FullName + "/" + newName) && System.IO.Path.GetFileName(file) != newName)
                {
                    if (System.IO.File.Exists(newName))
                    {
                        System.IO.File.Delete(parentDir.FullName + "/" + newName);
                    }
                    System.IO.File.Move(file, parentDir.FullName + "/" + newName);
                    System.IO.File.Delete(file);
                    System.Console.WriteLine("Deleted " + System.IO.Path.GetFileName(file));
                }
            }*/

            new Program().RunBotAsync().GetAwaiter().GetResult();
        }

        private static string[] scopes = { Google.Apis.Calendar.v3.CalendarService.Scope.Calendar };
        private static readonly string ApplicationName = "DiscordBotUpdates";

        private static System.DateTime _dateTime;

        //nentwork variables
        private static DiscordSocketClient _client;

        private static CommandService _commands;
        private static Google.Apis.Calendar.v3.CalendarService _service;
        private System.IServiceProvider _services;

        public static CommandService commands { get => _commands; set => _commands = value; }
        public static DiscordSocketClient client { get => _client; set => _client = value; }
        public static System.DateTime dateTime { get => _dateTime; set => _dateTime = value; }
        public static Google.Apis.Calendar.v3.CalendarService service { get => _service; set => _service = value; }

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
            System.Console.WriteLine("The time is now: " + _dateTime);

            await Task.Delay(10000);
            Modules.Commands commands = new Modules.Commands();
            await commands.MessageUpdater("Message Updater", "Client");
            await commands.PictureUpdater(Objects.ChannelID.botUpdatesId, "Picture Updater", "Client");
            await commands.ChatLogListener(Objects.ChannelID.botUpdatesId, "Chat Log Listener", "Client");
            await Task.Run(() => commands.init.SetAllAsync(true));

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

            if (message.HasStringPrefix("$", ref argPos) && message.Channel.Id == Objects.ChannelID.botCommandsId)
            {
                IResult result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {
                    System.Console.WriteLine(result.ErrorReason + " | " + context.Message);
                    await Modules.DBUTask.OutprintAsync(result.ErrorReason + " | " + context.Message, Objects.ChannelID.botCommandsId);
                }
            }
        }
    }
}