namespace DdhpCore.Workers.DefaultWorker
{
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
             handler.RegisterListener<StatImportedMessage>("statImported", statImportedHandler.StatImported);
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

    public class AzureHandler
    {
        public void RegisterListener<T>(string queueName, Action<DdhpMessage<T>> listener) where T : class
        {
            var client = QueueClient.Create(queueName);
            client.OnMessage((message) => listener(message.TranslateToDdhpMessage<T>()));
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
