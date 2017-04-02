using RabbitMQ.Client;

namespace RabbitMqBinding
{
    public class RabbitMqMessage
    {

        public string Topic { get; set; }
        public string RoutingKey { get; set; }
        public IBasicProperties Properties { get; set; }
        public byte[] Body { get; set; }
        public ulong DeliveryTag { get; set; }
        public string ReplyTopic { get; set; }
        public string CorrelationId { get; set; }
        public string ReplyRoutingKey { get; set; }
    }
}