namespace DiscordBotUpdates.Modules
{
    internal static class ChannelID
    {
        private static readonly ulong _botUpdatesID = 979100384037568582;
        private static readonly ulong _buildingID = 977770208183848990;
        private static readonly ulong _distressCallsID = 941795796523819048;
        private static readonly ulong _nuetrinoID = 943753103042289675;
        private static readonly ulong _slaversID = 966941055444467743;
        private static readonly ulong _slaversOnlyVoiceID = 941134782048395369;

        public static ulong botUpdatesID => _botUpdatesID;
        public static ulong buildingID => _buildingID;
        public static ulong distressCallsID => _distressCallsID;
        public static ulong nuetrinoID => _nuetrinoID;
        public static ulong slaversID => _slaversID;
        public static ulong slaversOnlyVoiceID => _slaversOnlyVoiceID;
    }
}