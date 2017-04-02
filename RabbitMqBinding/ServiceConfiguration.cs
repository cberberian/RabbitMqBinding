using System;
using System.ServiceModel;
using RabbitMqBinding.RequestReply;
using RabbitMQ.Client;

namespace RabbitMqBinding
{
    public static class ServiceConfiguration
    {
        public static string ServiceTopicExchange { get; private set; }

        public static string ServiceProcessingRoute { get; set; } = "service.worker";

        public static string ServiceQueue { get; set; }

        public static string ServiceDirectExchange { get; set; }

        public static string ServiceReplyExchange { get; set; }

        public static string TopicExchangeSuffix { get; set; } = ".exchange.topic";

        public static string DirectExchangeSuffix { get; set; } = ".exchange.direct";
        public static string ReplyExchangeSuffix { get; set; } = ".reply";

        public static void RegisterContract<T>()
        {
            var type = typeof(T);

            if (!type.IsInterface)
            {
                throw new InvalidOperationException("RegisterContract expects an interface");
            }
            
            var factory    = new ConnectionFactory();
            var connection = factory.CreateConnection();
            var model      = connection.CreateModel();

            //Subscribe to this exchange for logging and other interested parties. 
            model.ExchangeDeclare(ServiceTopicExchange, "topic", true, false, null);

            //Service itself subscribes to this exchange. 
            model.ExchangeDeclare(ServiceDirectExchange, "direct", true, false, null);

            //Direct exchanges subscribes to topic exchange for all messages. 
            model.ExchangeBind(ServiceDirectExchange, ServiceTopicExchange, "*");

            //Reply exchange
            model.ExchangeDeclare(ServiceReplyExchange, "headers", true, false, null);

            model.QueueDeclare(ServiceQueue, true, false, false);

            model.QueueBind(ServiceQueue, ServiceDirectExchange, ServiceProcessingRoute);


        }

        public static ServiceHost BuildServiceHost<TInterface, TService>(string uri)
        {
            var baseAddress = new Uri(uri);
            
            var host = new ServiceHost(typeof(TService));

            var type = typeof(TInterface);

            //Topic exchange that all message will be published to.
            ServiceTopicExchange = $"{type.Name}{TopicExchangeSuffix}";

            //Direct exchange that server service will listen on 
            ServiceDirectExchange = $"{type.Name}{DirectExchangeSuffix}";

            //Prefix for queues subscribing to the direct exchange
            ServiceQueue = $"{type.Name}.queue";

            //Reply exchange to return values. This is a topic exchange. 
            ServiceReplyExchange = $"{type.Name}{ReplyExchangeSuffix}";

            RegisterContract<TInterface>();

            host.AddServiceEndpoint(typeof(TInterface), new RabbitMqTransportBinding(), baseAddress);

            host.Open();

            return host;
        }
    }
}