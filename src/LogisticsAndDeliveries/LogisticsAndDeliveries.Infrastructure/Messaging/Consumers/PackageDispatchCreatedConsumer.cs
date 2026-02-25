using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using LogisticsAndDeliveries.Application.Drivers.Dto;
using LogisticsAndDeliveries.Application.Drivers.GetDrivers;
using LogisticsAndDeliveries.Application.Packages.CreatePackage;
using LogisticsAndDeliveries.Application.Packages.GetDriverDeliveryLoads;
using LogisticsAndDeliveries.Core.Results;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace LogisticsAndDeliveries.Infrastructure.Messaging.Consumers
{
    internal sealed class PackageDispatchCreatedConsumer : BackgroundService
    {
        private static readonly JsonSerializerOptions PayloadJsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PackageDispatchCreatedConsumer> _logger;
        private readonly RabbitMqOptions _options;

        private IConnection? _connection;
        private IModel? _channel;

        public PackageDispatchCreatedConsumer(
            IServiceScopeFactory scopeFactory,
            IOptions<RabbitMqOptions> options,
            ILogger<PackageDispatchCreatedConsumer> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _options = options.Value;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return RunConsumerLoopAsync(stoppingToken);
        }

        private async Task RunConsumerLoopAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    CreateConnection();

                    var consumer = new AsyncEventingBasicConsumer(_channel);
                    consumer.Received += async (_, eventArgs) =>
                    {
                        await HandleMessageAsync(eventArgs, stoppingToken);
                    };

                    _channel!.BasicConsume(
                        queue: _options.InputQueueName,
                        autoAck: false,
                        consumer: consumer);

                    _logger.LogInformation("Consumer de paquetes iniciado sobre la cola {Queue}", _options.InputQueueName);

                    await Task.Delay(Timeout.Infinite, stoppingToken);
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception exception)
                {
                    _logger.LogError(exception, "Error iniciando/ejecutando consumer RabbitMQ. Reintentando en {DelaySeconds}s", _options.ReconnectDelaySeconds);
                    DisposeConnection();
                    await Task.Delay(TimeSpan.FromSeconds(Math.Max(1, _options.ReconnectDelaySeconds)), stoppingToken);
                }
            }
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
                VirtualHost = _options.VirtualHost,
                DispatchConsumersAsync = true
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _channel.BasicQos(0, _options.PrefetchCount, false);

            if (_options.DeclareTopology)
            {
                _channel.ExchangeDeclare(_options.ExchangeName, ExchangeType.Topic, durable: true);
                _channel.QueueDeclare(_options.InputQueueName, durable: true, exclusive: false, autoDelete: false);
                _channel.QueueBind(_options.InputQueueName, _options.ExchangeName, _options.InputRoutingKey);
            }
        }

        private async Task HandleMessageAsync(BasicDeliverEventArgs eventArgs, CancellationToken cancellationToken)
        {
            if (_channel is null)
            {
                return;
            }

            try
            {
                var raw = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                // var payload = JsonSerializer.Deserialize<PackageDispatchCreatedPayload>(raw, PayloadJsonOptions);
                var payload = ExtractPayload(raw);

                if (payload is null)
                {
                    _logger.LogWarning("Mensaje inválido en {Queue}. Body: {Body}", _options.InputQueueName, raw);
                    _channel.BasicAck(eventArgs.DeliveryTag, false);
                    return;
                }

                if (!IsValidPayload(payload, out var validationMessage))
                {
                    _logger.LogWarning(
                        "Mensaje descartado en {Queue}. Motivo: {Reason}. Body: {Body}",
                        _options.InputQueueName,
                        validationMessage,
                        raw);

                    _channel.BasicAck(eventArgs.DeliveryTag, false);
                    return;
                }

                using var scope = _scopeFactory.CreateScope();
                var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

                var driversResult = await mediator.Send(new GetDriversQuery(), cancellationToken);
                if (driversResult.IsFailure || driversResult.Value.Count == 0)
                {
                    _logger.LogWarning("No existen drivers disponibles para asignar paquete {PackageId}.", payload.Id);
                    _channel.BasicNack(eventArgs.DeliveryTag, false, requeue: true);
                    return;
                }

                var selectedDriver = await SelectDriverAsync(
                    driversResult.Value,
                    payload,
                    mediator,
                    cancellationToken);

                if (selectedDriver is null)
                {
                    _logger.LogError(
                        "No fue posible seleccionar un driver para paquete {PackageId} usando la estrategia {Strategy}.",
                        payload.Id,
                        _options.DriverSelectionStrategy);
                    _channel.BasicNack(eventArgs.DeliveryTag, false, requeue: true);
                    return;
                }

                var command = new CreatePackageCommand
                {
                    Id = payload.Id,
                    Number = payload.Number,
                    PatientId = payload.PatientId,
                    PatientName = payload.PatientName,
                    PatientPhone = "N/A",
                    DeliveryAddress = payload.DeliveryAddress,
                    DeliveryLatitude = payload.DeliveryLatitude,
                    DeliveryLongitude = payload.DeliveryLongitude,
                    DeliveryDate = payload.DeliveryDate,
                    DriverId = selectedDriver.Id
                };

                var result = await mediator.Send(command, cancellationToken);

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Paquete {PackageId} procesado correctamente y asignado al driver {DriverId}.",
                        payload.Id,
                        selectedDriver.Id);
                    _channel.BasicAck(eventArgs.DeliveryTag, false);
                    return;
                }

                if (IsNonRetryableError(result.Error.Type))
                {
                    _logger.LogWarning(
                        "Mensaje descartado para paquete {PackageId}. Error no reintentable: {Code} - {Message}",
                        payload.Id,
                        result.Error.Code,
                        result.Error.Description);

                    _channel.BasicAck(eventArgs.DeliveryTag, false);
                    return;
                }

                _logger.LogError(
                    "Error reintentable procesando paquete {PackageId}. Error: {Code} - {Message}",
                    payload.Id,
                    result.Error.Code,
                    result.Error.Description);

                _channel.BasicNack(eventArgs.DeliveryTag, false, requeue: true);
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Error procesando mensaje de creación de paquete.");
                _channel.BasicNack(eventArgs.DeliveryTag, false, requeue: true);
            }
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

        private async Task<DriverDto?> SelectDriverAsync(
            ICollection<DriverDto> drivers,
            PackageDispatchCreatedPayload payload,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            return _options.DriverSelectionStrategy switch
            {
                DriverSelectionStrategy.LeastPackagesOnDate => await SelectDriverWithLeastPackagesAsync(
                    drivers,
                    payload.DeliveryDate,
                    mediator,
                    cancellationToken) ?? SelectDriverByDistance(drivers, payload),
                _ => SelectDriverByDistance(drivers, payload)
            };
        }

        private async Task<DriverDto?> SelectDriverWithLeastPackagesAsync(
            ICollection<DriverDto> drivers,
            DateOnly deliveryDate,
            IMediator mediator,
            CancellationToken cancellationToken)
        {
            var driverLoadsResult = await mediator.Send(new GetDriverDeliveryLoadsQuery(deliveryDate), cancellationToken);

            if (driverLoadsResult.IsFailure)
            {
                _logger.LogWarning(
                    "No fue posible obtener la carga de paquetes para la fecha {DeliveryDate}. Se usará la estrategia por proximidad. Codigo: {Code} - {Message}",
                    deliveryDate,
                    driverLoadsResult.Error.Code,
                    driverLoadsResult.Error.Description);

                return null;
            }

            var loadLookup = driverLoadsResult.Value.ToDictionary(load => load.DriverId, load => load.PackagesCount);

            return drivers
                .Select(driver => new
                {
                    Driver = driver,
                    Packages = loadLookup.TryGetValue(driver.Id, out var count) ? count : 0
                })
                .OrderBy(entry => entry.Packages)
                .ThenBy(entry => entry.Driver.Name)
                .ThenBy(entry => entry.Driver.Id)
                .Select(entry => entry.Driver)
                .FirstOrDefault();
        }

        private static DriverDto? SelectDriverByDistance(
            ICollection<DriverDto> drivers,
            PackageDispatchCreatedPayload payload)
        {
            return drivers
                .Where(driver => driver.Latitude.HasValue && driver.Longitude.HasValue)
                .OrderBy(driver => GetDistanceInKm(
                    payload.DeliveryLatitude,
                    payload.DeliveryLongitude,
                    driver.Latitude!.Value,
                    driver.Longitude!.Value))
                .ThenBy(driver => driver.Name)
                .FirstOrDefault()
                ?? drivers
                    .OrderBy(driver => driver.Name)
                    .FirstOrDefault();
        }

        private sealed class PackageDispatchCreatedPayload
        {
            public Guid Id { get; set; }
            public string Number { get; set; } = string.Empty;
            public Guid PatientId { get; set; }
            public string PatientName { get; set; } = string.Empty;
            public string DeliveryAddress { get; set; } = string.Empty;
            public double DeliveryLatitude { get; set; }
            public double DeliveryLongitude { get; set; }
            public DateOnly DeliveryDate { get; set; }
        }

        private static double GetDistanceInKm(double originLatitude, double originLongitude, double targetLatitude, double targetLongitude)
        {
            const double earthRadiusKm = 6371;

            var deltaLatitude = ToRadians(targetLatitude - originLatitude);
            var deltaLongitude = ToRadians(targetLongitude - originLongitude);

            var a = Math.Sin(deltaLatitude / 2) * Math.Sin(deltaLatitude / 2) +
                    Math.Cos(ToRadians(originLatitude)) * Math.Cos(ToRadians(targetLatitude)) *
                    Math.Sin(deltaLongitude / 2) * Math.Sin(deltaLongitude / 2);

            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
            return earthRadiusKm * c;
        }

        private static double ToRadians(double value)
        {
            return value * Math.PI / 180;
        }

        private static bool IsNonRetryableError(ErrorType errorType)
        {
            return errorType is ErrorType.Validation or ErrorType.NotFound or ErrorType.Conflict;
        }

        private static PackageDispatchCreatedPayload? ExtractPayload(string raw)
        {
            using var document = JsonDocument.Parse(raw);

            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                return null;
            }

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (!string.Equals(property.Name, "payload", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                return property.Value.ValueKind == JsonValueKind.Object
                    ? JsonSerializer.Deserialize<PackageDispatchCreatedPayload>(property.Value.GetRawText(), PayloadJsonOptions)
                    : null;
            }

            return JsonSerializer.Deserialize<PackageDispatchCreatedPayload>(document.RootElement.GetRawText(), PayloadJsonOptions);
        }

        private static bool IsValidPayload(PackageDispatchCreatedPayload payload, out string validationMessage)
        {
            if (payload.Id == Guid.Empty)
            {
                validationMessage = "id no puede ser Guid.Empty";
                return false;
            }

            if (string.IsNullOrWhiteSpace(payload.Number))
            {
                validationMessage = "number es requerido";
                return false;
            }

            if (payload.PatientId == Guid.Empty)
            {
                validationMessage = "patientId no puede ser Guid.Empty";
                return false;
            }

            if (string.IsNullOrWhiteSpace(payload.PatientName))
            {
                validationMessage = "patientName es requerido";
                return false;
            }

            if (string.IsNullOrWhiteSpace(payload.DeliveryAddress))
            {
                validationMessage = "deliveryAddress es requerido";
                return false;
            }

            if (payload.DeliveryLatitude < -90 || payload.DeliveryLatitude > 90)
            {
                validationMessage = "deliveryLatitude fuera de rango";
                return false;
            }

            if (payload.DeliveryLongitude < -180 || payload.DeliveryLongitude > 180)
            {
                validationMessage = "deliveryLongitude fuera de rango";
                return false;
            }

            if (payload.DeliveryDate < DateOnly.FromDateTime(DateTime.UtcNow))
            {
                validationMessage = "deliveryDate no puede ser pasada";
                return false;
            }

            validationMessage = string.Empty;
            return true;
        }
    }
}
