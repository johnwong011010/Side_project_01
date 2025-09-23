using Confluent.Kafka;
using Microsoft.Extensions.Options;
using OTP.Model;
using OTP.Service;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
// using C# console logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
// using Serilog for logging
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
builder.Services.Configure<UserDB>(builder.Configuration.GetSection("User"));
builder.Services.Configure<MachineDB>(builder.Configuration.GetSection("Machine"));
builder.Services.Configure<RequestDB>(builder.Configuration.GetSection("Request"));
builder.Services.Configure<KafkaProducer>(builder.Configuration.GetSection("Producer"));
builder.Services.Configure<KafkaConsumer>(builder.Configuration.GetSection("Consumer"));
builder.Services.AddSingleton<IProducer<Null, String>>(sp =>
{
    var config = sp.GetRequiredService<IOptionsMonitor<KafkaProducer>>().CurrentValue;
    var producerConfig = new ProducerConfig
    {
        BootstrapServers = config.BootstrapServers,
        MessageSendMaxRetries = config.MessageSendMaxRetries,
        RetryBackoffMs = config.RetryBackoffMs,
        LingerMs = config.LingerMs,
        BatchSize = config.BatchSize
    };
    return new ProducerBuilder<Null, string>(producerConfig).Build();
});
builder.Services.AddSingleton<IConsumer<Null, string>>(sc =>
{
    var config = sc.GetRequiredService<IOptionsMonitor<KafkaConsumer>>().CurrentValue;
    var consumerConfig = new ConsumerConfig
    {
        BootstrapServers = config.BootstrapServers,
        GroupId = config.GroupId,
        AutoOffsetReset = (AutoOffsetReset)config.AutoOffsetReset,
        EnableAutoCommit = config.EnableAutoCommit,
        MaxPollIntervalMs = config.MaxPollIntervalMs,
        SessionTimeoutMs = config.SessionTimeoutMs
    };
    return new ConsumerBuilder<Null, string>(consumerConfig).Build();
});
//adding a background service to handle kafka message
builder.Services.AddHostedService<KafkaService>();
builder.Services.AddSingleton<LoginService>();
builder.Services.AddSingleton<MachineService>();
builder.Services.AddSingleton<RequestService>();


builder.Services.AddCors(option =>
{
    option.AddPolicy("policy", builder => builder.AllowAnyMethod().AllowAnyMethod().AllowAnyOrigin().AllowAnyHeader());
});
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("policy");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
