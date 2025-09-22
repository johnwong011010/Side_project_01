using Confluent.Kafka;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;
namespace OTP.Service
{
    public class KafkaService : BackgroundService
    {
        private readonly ILogger<KafkaService> _logger;
        private readonly IProducer<Null, string> _producer;
        private readonly IConsumer<Null, string> _consumer;
        private readonly IMongoCollection<Model.NoCardRequest> _collection;
        public KafkaService(ILogger<KafkaService> logger, IProducer<Null, string> producer, IConsumer<Null, string> consumer, IOptionsMonitor<Model.RequestDB> database)
        {
            _logger = logger;
            _producer = producer;
            _consumer = consumer;
            _consumer.Subscribe("pending");
            _consumer.Subscribe("reject");
            var client = new MongoClient(database.CurrentValue.ConnectionString);
            var db = client.GetDatabase(database.CurrentValue.DatabaseName);
            _collection = db.GetCollection<Model.NoCardRequest>(database.CurrentValue.CollectionName);
        }
        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Kafka Service is starting.Time: {time}",DateTime.Now);
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = _consumer.Consume(cancellationToken);
                    switch (consumeResult.Topic)
                    {
                        case "pending":
                            _logger.LogInformation("Pending message received: {message}", consumeResult.Message.Value);
                            //do checklist to confirm this request is safe and vaild.
                            //after check, put this request to approved or reject topic
                            await DoChecklist(consumeResult.Message.Value);
                            // Process pending message
                            break;
                        case "reject":
                            _logger.LogInformation("Rejected message received: {message}", consumeResult.Message.Value);
                            await SendRejectMessage();
                            break;
                        case "approved":
                            //log the approved message
                            _logger.LogInformation("Approved message received:{message}", consumeResult.Message.Value);
                            //save the approved message to mongoDB
                            var message = System.Text.Json.JsonSerializer.Deserialize<Model.NoCardRequest>(consumeResult.Message.Value);
                            _collection.InsertOne(message);
                            break;
                        default:
                            _logger.LogWarning("Unknown topic: {topic}", consumeResult.Topic);
                            break;
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("Kafka Service is stopping due to cancellation.Time: {time}", DateTime.Now);
                    // Handle cancellation
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming message");
                }
            }
            _logger.LogInformation("Kafka Service is stopping.Time: {time}", DateTime.Now);
        }
        private async Task DoChecklist(string message)
        {
            // Simulate checklist processing
            await Task.Delay(2000); // Simulate some processing time
            var random = new Random();
            bool isApproved = random.Next(2) == 0; // Randomly approve or reject for demo purposes
            var topic = isApproved ? "approved" : "reject";
            //change the status in message from pending to approved or reject
            message = message.Replace("pending", isApproved ? "approved" : "reject");
            var kafkaMessage = new Message<Null, string> { Value = message };
            try
            {
                await _producer.ProduceAsync(topic, kafkaMessage);
                _logger.LogInformation("Message sent to topic '{topic}': {message}", topic, message);
            }
            catch (ProduceException<string, string> e)
            {
                _logger.LogError(e, "Kafka produce error while sending to topic '{topic}': {message}", topic, message);
            }
        }
        private async Task SendRejectMessage() 
        {
             await Task.Delay(1000); // Simulate some processing time
        }
    }
}
