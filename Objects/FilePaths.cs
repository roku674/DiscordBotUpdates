//Created by Alexander Fields https://github.com/roku67

namespace DiscordBotUpdates.Objects
{
    public class FilePaths
    {
        public string chatLogsDir;
        public string csvLocal;
        public string csvPath;
        public string enemyPlanetsDir;
        public string excelPath;
        public string planetPicturesDir;
        public string starportDir;
        public string undomedDir;
        public string picturesAndInfo;

        public FilePaths()
        {
        }

        public FilePaths(string chatLogsDir, string csvLocal, string csvPath, string enemyPlanetsDir, string excelPath, string planetPicturesDir, string starportDir, string undomedDir, string picturesAndInfo)
        {
            this.chatLogsDir = chatLogsDir;
            this.csvLocal = csvLocal;
            this.csvPath = csvPath;
            this.enemyPlanetsDir = enemyPlanetsDir;
            this.excelPath = excelPath;
            this.planetPicturesDir = planetPicturesDir;
            this.starportDir = starportDir;
            this.undomedDir = undomedDir;
            this.picturesAndInfo = picturesAndInfo;
        }
    }
}