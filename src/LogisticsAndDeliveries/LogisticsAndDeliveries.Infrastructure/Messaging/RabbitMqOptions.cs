namespace LogisticsAndDeliveries.Infrastructure.Messaging
{
    public class RabbitMqOptions
    {
        public const string SectionName = "RabbitMq";

        public string HostName { get; set; } = "154.38.180.80";
        public int Port { get; set; } = 5672;
        public string UserName { get; set; } = "admin";
        public string Password { get; set; } = "rabbit_mq";
        public string VirtualHost { get; set; } = "/";
        public string ExchangeName { get; set; } = "outbox.events";
        public string InputQueueName { get; set; } = "produccion.paquete-despacho-creado ";
        public string InputRoutingKey { get; set; } = "produccion.paquete-despacho-creado";
        public string OutputRoutingKey { get; set; } = "logistica.paquete.estado-actualizado";
        public bool DeclareTopology { get; set; } = false;
        public int ReconnectDelaySeconds { get; set; } = 10;
        public ushort PrefetchCount { get; set; } = 10;
        public int OutboxBatchSize { get; set; } = 50;
        public int OutboxPublishIntervalSeconds { get; set; } = 5;
    }
}
