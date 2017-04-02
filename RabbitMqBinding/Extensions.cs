using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Xml;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing;

namespace RabbitMqBinding
{
    public static class Extensions
    {
        public static bool IsNotQuit(this string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return true; 
            return!input.ToLower().Equals("q");
        }

        //publishes 
        public static void Publish<T>(this T target)
        {
            var factory = new ConnectionFactory();
            using (var connection = factory.CreateConnection())
            using (var model = connection.CreateModel())
            {
                var message = target.AsMessage();

                model.ExchangeDeclare(message.Topic, "topic");

                model.BasicPublish(message.Topic, message.RoutingKey, message.Properties, message.Body);
            }
        }

        public static RabbitMqMessage AsMessage<T>(this T target)
        {


            return new RabbitMqMessage
            {
                Body = target.AsBytes(),
                Topic = typeof(T).Name,
                RoutingKey = string.Empty,
                Properties = new BasicProperties
                {
                    
                }
                
            };
        }


        internal static RabbitMqMessage AsRabbitMqMessage(this BasicGetResult basicGet)
        {
            return GetRabbitMqMessage(basicGet.BasicProperties, basicGet.RoutingKey, basicGet.Body, basicGet.Exchange, basicGet.DeliveryTag);
        }

        internal static RabbitMqMessage AsRabbitMqMessage(this BasicDeliverEventArgs basicGet)
        {
            return GetRabbitMqMessage(basicGet.BasicProperties, basicGet.RoutingKey, basicGet.Body, basicGet.Exchange, basicGet.DeliveryTag);
        }

        private static RabbitMqMessage GetRabbitMqMessage(IBasicProperties properties, string routingkey, byte[] body, string topic, ulong deliveryTag)
        {
            return new RabbitMqMessage
            {
                Properties = properties,
                RoutingKey = routingkey,
                Body = body,
                Topic = topic,
                DeliveryTag = deliveryTag,
                ReplyTopic = properties.ReplyTo,
                CorrelationId = properties.CorrelationId
            };
        }


        internal static RabbitMqMessage AsRabbitMqMessage(this MessageBuffer bufferedCopy)
        {
            return bufferedCopy.CreateMessage().AsRabbitMqMessage();
        }

        internal static RabbitMqMessage AsRabbitMqMessage(this Message message)
        {
            RabbitMqMessage rabbitMsg;

            using (Stream stream = new MemoryStream())
            {

                var writer = XmlWriter.Create(stream);

                message.WriteMessage(writer);

                writer.Flush();

                stream.Position = 0;

                var bytes = new byte[stream.Length];

                stream.Read(bytes, 0, (int)stream.Length);

                var actionComponents = message.Headers.Action.Split('/');
                var serviceName = actionComponents[actionComponents.Length - 2];
                var correlationId = Guid.NewGuid().ToString();
                var replyTopic = $"{serviceName}{ServiceConfiguration.ReplyExchangeSuffix}";

                IBasicProperties props = new BasicProperties
                {
                    MessageId     = message.Headers.MessageId?.ToString() ?? Guid.NewGuid().ToString(),
                    CorrelationId = correlationId,
                    ReplyTo       = replyTopic,
                    Headers = new Dictionary<string, object>()
                };

                rabbitMsg = new RabbitMqMessage
                {
                    Body            = bytes,
                    Topic           = $"{serviceName}{ServiceConfiguration.DirectExchangeSuffix}",
                    ReplyTopic      = replyTopic,
                    ReplyRoutingKey = "",
                    RoutingKey      = ServiceConfiguration.ServiceProcessingRoute,
                    Properties      = props,
                    CorrelationId   = correlationId
                };
            }
            return rabbitMsg;
        }

        public static Message AsWcfMessage(this RabbitMqMessage currentRabbitMqMessage, long parentMaxReceivedMessageSize, BufferManager parentBufferManager, MessageEncoderFactory messageEncoderFactory)
        {
            Debug.Assert(currentRabbitMqMessage!=null);
            byte[] data;
            long bytesTotal;

            using (Stream stream = new MemoryStream())
            {
                stream.Write(currentRabbitMqMessage.Body, 0, currentRabbitMqMessage.Body.Length);

                stream.Position = 0;

                bytesTotal = stream.Length;

                if (bytesTotal > int.MaxValue)
                {
                    throw new CommunicationException(
                        $"Message of size {bytesTotal} bytes is too large to buffer. Use a streamed transfer instead.");
                }

                if (bytesTotal > parentMaxReceivedMessageSize)
                {
                    throw new CommunicationException(
                        $"Message exceeds maximum size: {bytesTotal} > {parentMaxReceivedMessageSize}.");
                }

                data = parentBufferManager.TakeBuffer((int)bytesTotal);

                var bytesRead = 0;

                while (bytesRead < bytesTotal)
                {
                    var count = stream.Read(data, bytesRead, (int)bytesTotal - bytesRead);

                    if (count == 0)
                    {
                        throw new CommunicationException($"Unexpected end of message after {bytesRead} of {bytesTotal} bytes.");
                    }

                    bytesRead += count;
                }
            }

            var buffer = new ArraySegment<byte>(data, 0, (int)bytesTotal);

            var message = messageEncoderFactory.Encoder.ReadMessage(buffer, parentBufferManager);

            parentBufferManager.ReturnBuffer(data);

            return message;
        }

        public static byte[] AsBytes<T>(this T target)
        {
            var serializer = new DataContractSerializer(typeof(T));
            using (var stream = new MemoryStream())
            {
                serializer.WriteObject(stream, target);
                return stream.ToArray();
            }
        }
    }
}