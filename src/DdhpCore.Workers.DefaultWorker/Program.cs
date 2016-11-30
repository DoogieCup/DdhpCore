namespace DdhpCore.Workers.DefaultWorker
{
    using DdhpCore.Workers.DefaultWorker.Extensions;
    using System;
    using Nito.AsyncEx;
    using System.Collections.Generic;
    using Microsoft.WindowsAzure.Storage;
    using Microsoft.WindowsAzure.Storage.Queue;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Configuration;
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
            var factory = new LoggerFactory()
                .WithFilter(new FilterLoggerSettings
                {
                    { "Microsoft", LogLevel.Warning },
                    { "System", LogLevel.Warning },
                    { "SampleApp.Program", LogLevel.Debug }
                });

            var loggingConfiguration = new ConfigurationBuilder()
                .AddJsonFile("logging.json", optional: false, reloadOnChange: true)
                .Build();
            factory.AddConsole(loggingConfiguration);

            _logger = factory.CreateLogger<Program>();
            _logger.LogDebug("Logger Configured");

            using (var program = new Program())
            {
                program.Run();
            }
        }

        private CancellationTokenSource _cancel = new CancellationTokenSource();
        private static ILogger _logger;

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
             var handler = new QueueStorageHandler(_cancel.Token);
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
        public QueueStorageHandler(CancellationToken token)
        {
            _token = token;
        }
        public void RegisterListener<T>(string queueName, 
            Action<DdhpMessage<T>> listener) where T : class
        {
            // CloudStorageAccount storageAccount = CloudStorageAccount.Parse(
            //     string.Empty);
            // CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();
            // CloudQueue queue = queueClient.GetQueueReference("myqueue");

            _listeners.Add(new Func<Task>(async () => {
                    // CloudQueueMessage message = await queue.GetMessageAsync().ConfigureAwait(false);
                    // listener(message.TranslateToDdhpMessage<T>());
                    // await queue.DeleteMessageAsync(message).ConfigureAwait(false);
                    await Task.Delay(1000);
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
