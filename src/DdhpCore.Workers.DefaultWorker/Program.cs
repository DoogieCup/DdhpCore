namespace DdhpCore.Workers.DefaultWorker
{
    using DdhpCore.Workers.DefaultWorker.Models;
    using DdhpCore.Workers.DefaultWorker.Extensions;
    using Microsoft.ServiceBus.Messaging;
    using System;

    /// <summary>
    /// Any queue handlers here must be agnostic as to whether they are running on
    /// Azure ServiceBus, Azure Queue Storage, or AWS SNS/SQS.
    /// We should be able to switch between those implementations with ease.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
            var program = new Program();
            program.Run();
        }

        /*
         * A handler function should be agnostic.
         * That which receives the message from the queue * 
         */

        public void Run()
        {
             var handler = new AzureHandler();
             var statImportedHandler = new StatImportedHandler();
             handler.RegisterListener("statImported", statImportedHandler.StatImported);
        }
    }

    public class StatImportedHandler
    {        
        public void StatImported(DdhpMessage message)
        {
            // Fetch all PlayedTeams for the stat round
            // Search each played team for the player in the stat
            // If found, update the scores and played positions
            // Save back to the store
        }
    }

    public class AzureHandler
    {
        public void RegisterListener(string queueName, Action<DdhpMessage> listener)
        {
            var client = QueueClient.Create(queueName);
            client.OnMessage((message) => listener(message.TranslateToDdhpMessage()));
        }
    }

    public class DdhpMessage
    {
        string Message { get; set; }
    }
}
