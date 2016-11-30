namespace DdhpCore.Workers.DefaultWorker
{
    using DdhpCore.Workers.DefaultWorker.Extensions;
    using Nito.AsyncEx;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using LogLevel = Microsoft.Extensions.Logging.LogLevel;

    /// <summary>
    /// Any queue handlers here must be agnostic as to whether they are running on
    /// Azure ServiceBus, Azure Queue Storage, or AWS SNS/SQS.
    /// We should be able to switch between those implementations with ease.
    /// </summary>
    public class Program : IDisposable
    {
        public static void Main(string[] args)
        {
           BuildLogger();

           var config = new ConfigurationBuilder();

           if (IsDevelopment)
           {
               config.AddUserSecrets();
           }

            _logger.LogDebug("Logger Configured");

            using (var program = new Program())
            {
                program.Run();
            }
        }

        public static bool IsDevelopment
        {
            get
            {
                bool isDevelopment;
                bool.TryParse(Environment.GetEnvironmentVariable("IsDevelopment"), out isDevelopment);

                return isDevelopment;
            }
        }

        private static void BuildLogger()
        {
             _loggerFactory = new LoggerFactory()
                .WithFilter(new FilterLoggerSettings
                {
                    { "Microsoft", LogLevel.Warning },
                    { "System", LogLevel.Warning },
                    { "SampleApp.Program", LogLevel.Debug }
                });

            var loggingConfiguration = new ConfigurationBuilder()
                .AddJsonFile("logging.json", optional: false, reloadOnChange: true)
                .Build();
            _loggerFactory.AddConsole(loggingConfiguration);

            _logger = _loggerFactory.CreateLogger<Program>();
        }

        private CancellationTokenSource _cancel = new CancellationTokenSource();
        private static ILogger _logger;
        private static ILoggerFactory _loggerFactory;

        public void Dispose()
        {
            _logger.LogDebug("Disposing");
            _cancel.Cancel();
        }

        /*
         * A handler function should be agnostic.
         * That which receives the message from the queue * 
         */

        public void Run()
        {
             var handler = new QueueStorageHandler(_loggerFactory, _cancel.Token);
             var statImportedHandler = new StatImportedHandler();
             handler.RegisterListener<StatImportedMessage>("statImported", 
                statImportedHandler.StatImported);
            handler.Run();
        }
    }

    public class StatImportedHandler
    {        
        public void StatImported(DdhpMessage<StatImportedMessage> message)
        {
            // Fetch all PlayedTeams for the stat round
            // Search each played team for the player in the stat
            // If found, update the scores and played positions
            // Save back to the store

            var round = message.Body.Round;
        }
    }

    public class QueueStorageHandler
    {
        public QueueStorageHandler(ILoggerFactory loggerFactory, CancellationToken token)
        {
            _token = token;
            _logger = loggerFactory.CreateLogger(typeof(QueueStorageHandler));
        }

        private ILogger _logger;

        public void RegisterListener<T>(string queueName, 
            Action<DdhpMessage<T>> listener) where T : class
        {
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
                string.Empty);
            CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            CloudQueue queue = queueClient.GetQueueReference("statImported");

            _listeners.Add(new Func<Task>(async () => {
                    CloudQueueMessage message = await queue.GetMessageAsync().ConfigureAwait(false);
                    listener(message.TranslateToDdhpMessage<T>());
                    await queue.DeleteMessageAsync(message).ConfigureAwait(false);
            }));

        }

        private CancellationToken _token;
        private List<Func<Task>> _listeners = new List<Func<Task>>();
        public void Run()
        {
            AsyncContext.Run(
                async () => {
                    var tasks = new List<Task>();
                    foreach (var listener in _listeners)
                    {
                        tasks.Add(Task.Run(async () => {
                            while (!_token.IsCancellationRequested)
                            {
                                await listener();
                            }}));
                    }
                    await Task.WhenAll(tasks);
                }
            );
        }
    }

/*
* This is an abstraction so that we're not bound to Queue Storage, ServiceBus, or SQS for messaging.
*/
    public class DdhpMessage<T> where T : class
    {
        public T Body{get;set;}
    }

    public class StatImportedMessage
    {
        public int Round{get;set;}
    }
}
