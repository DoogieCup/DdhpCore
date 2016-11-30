namespace DdhpCore.Workers.DefaultWorker.Extensions
{
    using System.IO;
    using Microsoft.WindowsAzure.Storage.Queue;
    using Newtonsoft.Json;

    public static class CloudQueueMessageExtensions
    {
        public static DdhpMessage<T> TranslateToDdhpMessage<T>(this CloudQueueMessage message) where T : class
        {
            var body = message.AsString;

            return new DdhpMessage<T>
            {
                Body = JsonConvert.DeserializeObject<T>(body)
            };
        }
    }
}