namespace OTP.Model
{
    public class KafkaProducer
    {
        public string BootstrapServers { get; set; } = null!;
        public bool EnableIdempotence { get; set; }
        public int MessageSendMaxRetries { get; set; }
        public int RetryBackoffMs { get; set; }
        public int LingerMs { get; set; }
        public int BatchSize { get; set; }
    }
}
