namespace DdhpCore.Workers.DefaultWorker
{
    /// <summary>
    /// Any queue handlers here must be agnostic as to whether they are running on
    /// Azure ServiceBus, Azure Queue Storage, or AWS SNS/SQS.
    /// We should be able to switch between those implementations with ease.
    /// </summary>
    public class Program
    {
        public static void Main(string[] args)
        {
        }
    }
}
