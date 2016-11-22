namespace DdhpCore.Workers.DefaultWorker.Extensions
{
    using Microsoft.ServiceBus.Messaging;

    public static class BrokeredMessageExtensions
    {
        public static DdhpMessage TranslateToDdhpMessage(this BrokeredMessage message)
        {
            return new DdhpMessage();
        }
    }
}