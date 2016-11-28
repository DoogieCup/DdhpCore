namespace DdhpCore.Workers.DefaultWorker.Extensions
{
    using System.IO;
    using Microsoft.ServiceBus.Messaging;
    using Newtonsoft.Json;

    public static class BrokeredMessageExtensions
    {
        public static DdhpMessage<T> TranslateToDdhpMessage<T>(this BrokeredMessage message) where T : class
        {
            var body = message.GetBody<Stream>();
            var stringWriter = new StringWriter();
            var streamReader = new StreamReader(body);
            var buffer = new char[body.Length];
            var text = streamReader.ReadToEnd();

            return new DdhpMessage<T>
            {
                Body = JsonConvert.DeserializeObject<T>(text)
            };
        }
    }
}