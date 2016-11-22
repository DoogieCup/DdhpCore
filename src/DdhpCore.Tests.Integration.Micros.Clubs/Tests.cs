using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.Model;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;

namespace DdhpCore.Tests.Integration.Micros.Clubs
{
    public class Tests
    {
        public readonly HttpClient Client;
        private static readonly Regex AppRootRegex = new Regex("FrontEnd$", RegexOptions.Compiled);
        private readonly ITestOutputHelper _output;

        public Tests(ITestOutputHelper output)
        {
            _output = output;
            var testServer = new TestServer(new WebHostBuilder()
                .UseEnvironment("Development")
                .UseContentRoot(ApplicationRoot().FullName)
                .ConfigureServices(ConfigureServices)
                .UseStartup<TestStartup>());
            Client = testServer.CreateClient();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IAmazonDynamoDB>(AmazonDynamoDbClient);
        }

        private static AmazonDynamoDBClient AmazonDynamoDbClient
        {
            get
            {
                Amazon.Util.ProfileManager.RegisterProfile("fake", "fake", "alsofake");
                var amazonDynamoDbConfig = new AmazonDynamoDBConfig
                {
                    ServiceURL = "http://localhost:8000"
                };

                AmazonDynamoDBClient dynamoClient = new AmazonDynamoDBClient(amazonDynamoDbConfig);
                return dynamoClient;
            }
        }

        private DirectoryInfo ApplicationRoot()
        {
            var root = Directory.GetCurrentDirectory();
            var rootInfo = new DirectoryInfo(root);
            var src = rootInfo.Parent.Parent.EnumerateDirectories();
            var appRoot = src.Single(q => AppRootRegex.IsMatch(q.Name));
            return appRoot;
        }

        [Fact]
        public async Task Test1() 
        {
            try
            {
                const string tableName = "clubs";
                var result = await AmazonDynamoDbClient.DescribeTableAsync(tableName);
            }
            catch (ResourceNotFoundException)
            {
                //Create the table here
            }

            var response = await Client.GetAsync("/api/clubs");
            response.EnsureSuccessStatusCode();
            _output.WriteLine(await response.Content.ReadAsStringAsync());
            Assert.True(true);
        }
    }
}
