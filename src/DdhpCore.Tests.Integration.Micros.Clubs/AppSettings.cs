using Microsoft.Extensions.Configuration;

namespace DdhpCore.Tests.Integration.Micros.Clubs
{
    public class AppSettings
    {
        private readonly IConfigurationRoot _config;

        public AppSettings(IConfigurationRoot config)
        {
            _config = config;
        }

        public string DynamoDbEndpoint => _config.GetValue<string>("dynamoEndpoint");
    }
}