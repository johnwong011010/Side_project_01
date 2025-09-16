using Confluent.Kafka;
namespace OTP.Service
{
    public class KafkaService : BackgroundService
    {
        private readonly ILogger<KafkaService> _logger;
        private readonly IProducer<Null, string> _producer;
        private readonly IConsumer<Null, string> _consumer;
        public KafkaService(ILogger<KafkaService> logger, IProducer<Null, string> producer, IConsumer<Null, string> consumer)
        {
            _logger = logger;
            _producer = producer;
            _consumer = consumer;
            _consumer.Subscribe("pending");
            _consumer.Subscribe("approved");
            _consumer.Subscribe("reject");
        }
    }
}
