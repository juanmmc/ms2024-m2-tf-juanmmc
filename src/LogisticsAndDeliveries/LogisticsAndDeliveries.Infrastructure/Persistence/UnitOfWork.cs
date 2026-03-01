using LogisticsAndDeliveries.Core.Abstractions;
using LogisticsAndDeliveries.Domain.Packages.Events;
using LogisticsAndDeliveries.Infrastructure.Persistence.DomainModel;
using LogisticsAndDeliveries.Infrastructure.Outbox;
using System.Text.Json;
using System.Collections.Immutable;
using MediatR;

namespace LogisticsAndDeliveries.Infrastructure.Persistence
{
    internal class UnitOfWork : IUnitOfWork
    {
        private readonly DomainDbContext _dbContext;
        private readonly IMediator _mediator;
        private const string PackageDispatchStatusUpdatedEventName = "logistica.paquete.estado-actualizado";

        public UnitOfWork(DomainDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
        }

        public async Task CommitAsync(CancellationToken cancellationToken = default)
        {
            //Get domain events
            var domainEvents = _dbContext.ChangeTracker
                .Entries<Entity>()
                .Where(x => x.Entity.DomainEvents.Any())
                .Select(x =>
                {
                    var domainEvents = x.Entity.DomainEvents.ToImmutableArray();
                    x.Entity.ClearDomainEvents();

                    return domainEvents;
                })
                .SelectMany(domainEvents => domainEvents)
                .ToList();
            
            /*foreach (var domainEvent in domainEvents)
            {
                await _mediator.Publish(domainEvent, cancellationToken);
            }*/

            var outboxMessages = domainEvents
                .Select(MapToOutboxMessage)
                .Where(message => message is not null)
                .Cast<OutboxMessage>()
                .ToList();

            if (outboxMessages.Count > 0)
            {
                await _dbContext.OutboxMessage.AddRangeAsync(outboxMessages, cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        private OutboxMessage? MapToOutboxMessage(DomainEvent domainEvent)
        {
            if (domainEvent is PackageDeliveryStatusChangedDomainEvent packageEvent)
            {
                var payload = new
                {
                    packageId = packageEvent.PackageId,
                    driverId = packageEvent.DriverId,
                    number = packageEvent.Number,
                    deliveryStatus = packageEvent.DeliveryStatus,
                    incidentType = packageEvent.IncidentType,
                    incidentDescription = packageEvent.IncidentDescription,
                    deliveryEvidence = packageEvent.DeliveryEvidence,
                    occurredOn = packageEvent.OccurredOn,
                    updatedAt = packageEvent.UpdatedAt
                };

                return new OutboxMessage
                {
                    Id = domainEvent.Id,
                    EventName = PackageDispatchStatusUpdatedEventName,
                    Type = domainEvent.GetType().FullName ?? nameof(PackageDeliveryStatusChangedDomainEvent),
                    Content = JsonSerializer.Serialize(payload),
                    OccurredOnUtc = domainEvent.OccurredOn.ToUniversalTime()
                };
            }

            return null;
        }
    }
}
