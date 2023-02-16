
//Created by Alexander Fields 

using Microsoft.Extensions.Configuration;

namespace Optimization.Objects
{
    public static class Settings
    {
        /// <summary>
        /// For setting the static settings
        /// </summary>
        /// <param name="configuration"></param>
        public static void SetSettings(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        /// <summary>
        /// Creates a configuration object
        /// </summary>
        /// <param name="configJsonPath">Path to the config/app settings file </param>
        /// <returns>IConfig</returns>
        public static IConfiguration BuildConfig(string configJsonPath)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile(configJsonPath);
            return builder.Build();
        }

        /// <summary>
        /// Creates and sets the configuration
        /// </summary>
        /// <param name="configJsonPath">Path to the config/app settings file </param>
        /// <returns>IConfig</returns>
        public static void BuildAndSetConfig(string configJsonPath)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder().AddJsonFile(configJsonPath);
            Configuration = builder.Build();
        }

        /// <summary>
        /// Config Interface Object
        /// </summary>
        public static IConfiguration Configuration
        {
            get; private set;
        }
    }
}