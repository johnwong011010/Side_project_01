using Confluent.Kafka;
namespace OTP.Model
{
    public class KafkaConsumer
    {
        public string BootstrapServers { get; set; } = null!;
        public string GroupId { get; set; }
        public AutoOffsetReset AutoOffsetReset { get; set; }
        public bool EnableAutoCommit { get; set; }
        public int MaxPollIntervalMs { get; set; }
        public int SessionTimeoutMs { get; set; }
    }
}
