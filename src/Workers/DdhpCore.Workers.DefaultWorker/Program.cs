namespace DdhpCore.Workers.DefaultWorker
{
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
             handler.RegisterListener("statImported", (message) => {});
        }
    }

    public class AzureHandler
    {
        public void RegisterListener(string queueName, Action<DdhpMessage> listener)
        {
            var client = QueueClient.Create(queueName);
            client.OnMessage((message) => listener(TranslateToDdhpMessage(message)));
        }

        private DdhpMessage TranslateToDdhpMessage(BrokeredMessage message)
        {
            return new DdhpMessage();
        }
    }

    public class DdhpMessage
    {
        string Message { get; set; }
    }
}
