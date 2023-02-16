//Created by Alexander Fields https://github.com/roku674
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using Google.Apis.Calendar.v3.Data;
using Microsoft.Extensions.DependencyInjection;

namespace DiscordBotUpdates
{
    internal class Program
    {
        private static string directory;
        //objects

        public static Objects.Bots bots;
        public static Objects.ChannelId channelId;
        public static Objects.FilePaths filePaths;

        //discord && google

        public static CommandService commands { get => _commands; set => _commands = value; }
        public static DiscordSocketClient client { get => _client; set => _client = value; }
        public static System.DateTime dateTime { get => _dateTime; set => _dateTime = value; }
        public static Google.Apis.Calendar.v3.CalendarService service { get => _service; set => _service = value; }
        private static string[] scopes = { Google.Apis.Calendar.v3.CalendarService.Scope.Calendar };
        private static readonly string ApplicationName = "DiscordBotUpdates";

        private static System.DateTime _dateTime;

        //nentwork variables

        private static DiscordSocketClient _client;
        private static CommandService _commands;
        private static Google.Apis.Calendar.v3.CalendarService _service;
        private System.IServiceProvider _services;

        private static void Main(string[] args)
        {
            string configContents = System.IO.File.ReadAllText(Directory.GetCurrentDirectory() + "/config.json");
            Optimization.Objects.Settings.BuildAndSetConfig(Directory.GetCurrentDirectory() + "/config.json");
            directory = System.IO.Directory.GetCurrentDirectory();
            System.Console.WriteLine("Last Update: " + System.IO.File.GetLastWriteTime(System.Reflection.Assembly.GetEntryAssembly().Location));
            Google.Apis.Auth.OAuth2.UserCredential credential;

            if (!System.IO.Directory.Exists(directory + "/JSON"))
            {
                System.IO.Directory.CreateDirectory(directory + "/JSON");
            }
            //Update diplomacy lists from internet
            Algorithms.FileManipulation.GetFileFromInternet("https://raw.githubusercontent.com/roku674/StarportObjects/main/JSON/allies.json", directory + "/JSON/allies.json");
            Algorithms.FileManipulation.GetFileFromInternet("https://raw.githubusercontent.com/roku674/StarportObjects/main/JSON/enemies.json", directory + "/JSON/enemies.json");
            Algorithms.FileManipulation.GetFileFromInternet("https://raw.githubusercontent.com/roku674/StarportObjects/main/JSON/nap.json", directory + "/JSON/nap.json");

            //store them in diplomacy
            StarportObjects.Diplomacy.allies = System.Text.Json.JsonSerializer.Deserialize<string[]>(System.IO.File.ReadAllText(directory + "/JSON/allies.json"));
            StarportObjects.Diplomacy.enemies = System.Text.Json.JsonSerializer.Deserialize<string[]>(System.IO.File.ReadAllText(directory + "/JSON/enemies.json"));
            StarportObjects.Diplomacy.nap = System.Text.Json.JsonSerializer.Deserialize<string[]>(System.IO.File.ReadAllText(directory + "/JSON/nap.json"));

            //store filepaths/challenids and bots
            filePaths = Newtonsoft.Json.JsonConvert.DeserializeObject<Objects.FilePaths>(System.IO.File.ReadAllText(directory + "/JSON/filepaths.json"));
            channelId = Newtonsoft.Json.JsonConvert.DeserializeObject<Objects.ChannelId>(System.IO.File.ReadAllText(directory + "/JSON/channelid.json"));
            bots = Newtonsoft.Json.JsonConvert.DeserializeObject<Objects.Bots>(System.IO.File.ReadAllText(directory + "/JSON/bots.json"));

            using (System.IO.FileStream stream =
                new System.IO.FileStream(directory + "/JSON/client_secret.json", System.IO.FileMode.Open, System.IO.FileAccess.Read))
            {
                // The file token.json stores the user's access and refresh tokens, and is created
                // automatically when the authorization flow completes for the first time.
                string credPath = directory + "/JSON";

                credential = Google.Apis.Auth.OAuth2.GoogleWebAuthorizationBroker.AuthorizeAsync(
                    Google.Apis.Auth.OAuth2.GoogleClientSecrets.FromStream(stream).Secrets,
                    scopes,
                    "user",
                    System.Threading.CancellationToken.None,
                    new Google.Apis.Util.Store.FileDataStore(credPath, true)).Result;
                //System.Console.WriteLine("Credential file saved to: " + credPath + " folder.");
            }

            _service = new Google.Apis.Calendar.v3.CalendarService(new Google.Apis.Services.BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            new Program().RunBotAsync().GetAwaiter().GetResult();
        }

        public async Task RunBotAsync()
        {
            System.Console.WriteLine("The time is now: " + _dateTime);
            _client = new DiscordSocketClient();
            _commands = new CommandService();

            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .BuildServiceProvider();

            string jsonPath = directory + "/JSON/tokendiscord.json";
            string jsonFileContents = System.IO.File.ReadAllText(jsonPath);
            string token = System.Text.Json.JsonSerializer.Deserialize<string>(jsonFileContents);

            _client.Log += _client_Log;

            await RegisterCommandsAsync();
            await _client.LoginAsync(Discord.TokenType.Bot, token);
            await _client.StartAsync();

            _dateTime = System.DateTime.Now;

            await Task.Delay(10000);
            Modules.Commands commands = new Modules.Commands();
            await commands.MessageUpdater("Message Updater", "Client");
            await commands.PictureUpdater(Program.channelId.botUpdatesId, "Picture Updater", "Client");
            await commands.ChatLogListener(Program.channelId.botUpdatesId, "Chat Log Listener", "Client");
            await Task.Run(() => commands.init.SetAllAsync(true));

            await Task.Run(() => Modules.TaskInitiator.PingAPI());
            await Task.Run(() => Modules.TaskInitiator.LoadExcelHoldingsAsync());

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

            if (message.HasStringPrefix("$", ref argPos) && message.Channel.Id == Program.channelId.botCommandsId)
            {
                IResult result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess)
                {
                    System.Console.WriteLine(result.ErrorReason + " | " + context.Message);
                    await Modules.DBUTask.OutprintAsync(result.ErrorReason + " | " + context.Message, Program.channelId.botCommandsId);
                }
            }
        }
    }
}