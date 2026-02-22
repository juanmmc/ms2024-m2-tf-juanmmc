using System.Text;
using LogisticsAndDeliveries.Infrastructure.Outbox;
using LogisticsAndDeliveries.Infrastructure.Persistence.PersistenceModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace LogisticsAndDeliveries.Infrastructure.Messaging.Outbox
{
    internal sealed class OutboxPublisherService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<OutboxPublisherService> _logger;
        private readonly RabbitMqOptions _options;

        private IConnection? _connection;
        private IModel? _channel;

        public OutboxPublisherService(
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<OutboxPublisherService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    EnsureConnection();
                    await PublishPendingMessagesAsync(stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Error publicando mensajes outbox.");
                    DisposeConnection();
                }

                await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, _options.OutboxPublishIntervalSeconds)), stoppingToken);
            }
        }

        private void EnsureConnection()
        {
            if (_connection is { IsOpen: true } && _channel is { IsOpen: true })
            {
                return;
            }

            CreateConnection();
        }

        private void CreateConnection()
        {
            DisposeConnection();

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            if (_options.DeclareTopology)
            {
                _channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true);
            }
        }

        private async Task PublishPendingMessagesAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PersistenceDbContext>();

            var pendingMessages = await dbContext.OutboxMessage
                .Where(message => message.ProcessedOnUtc == null)
                .OrderBy(message => message.OccurredOnUtc)
                .Take(_options.OutboxBatchSize)
                .ToListAsync(cancellationToken);

            if (pendingMessages.Count == 0)
            {
                return;
            }

            foreach (var message in pendingMessages)
            {
                try
                {
                    PublishMessage(message);
                    message.ProcessedOnUtc = DateTime.UtcNow;
                    message.Error = null;
                }
                catch (Exception exception)
                {
                    message.Error = exception.Message;
                    _logger.LogError(exception, "Error publicando outbox message {OutboxMessageId}", message.Id);
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        private void PublishMessage(OutboxMessage message)
        {
            if (_channel is null)
            {
                throw new InvalidOperationException("RabbitMQ channel no inicializado.");
            }

            var body = Encoding.UTF8.GetBytes(message.Content);
            var properties = _channel.CreateBasicProperties();
            properties.Persistent = true;
            properties.ContentType = "application/json";
            properties.MessageId = message.Id.ToString();
            properties.Type = message.Type;
            properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: string.IsNullOrWhiteSpace(message.EventName) ? _options.OutputRoutingKey : message.EventName,
                basicProperties: properties,
                body: body);
        }

        public override void Dispose()
        {
            DisposeConnection();
            base.Dispose();
        }

        private void DisposeConnection()
        {
            try
            {
                _channel?.Dispose();
                _connection?.Dispose();
            }
            finally
            {
                _channel = null;
                _connection = null;
            }
        }
    }
}
