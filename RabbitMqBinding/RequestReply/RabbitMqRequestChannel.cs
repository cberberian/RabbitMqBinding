using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using System.Xml;
using RabbitMqBinding.Base;
using RabbitMQ.Client;

namespace RabbitMqBinding.RequestReply
{
    public class RabbitMqRequestChannel : RabbitMqChannelBase, IRequestChannel
    {
        delegate Message WaitReplyDelegate(RabbitMqMessage originalPublishedMessage, string replyQueue, TimeSpan timeout);

        public EndpointAddress RemoteAddress { get; }

        public Uri Via { get; }

        internal RabbitMqChannelFactory Parent { get; }


        public RabbitMqRequestChannel(RabbitMqChannelFactory parent, EndpointAddress remoteAddress, Uri via) : base(parent)
        {
            RemoteAddress = remoteAddress;
            Via = via;
            Parent = parent;
        }

        public Message Request(Message message)
        {
            return Request(message, _Timeout);
        }

        public Message Request(Message message, TimeSpan timeout)
        {
            RemoteAddress.ApplyTo(message);

            //TODO: setup reply queue and wait for reply message. 
            var rabbitMsg = message.AsRabbitMqMessage();

            var queueResult = Model.QueueDeclare();

            var specs = new Dictionary<string, object> {{"x-match", "all"}, {"correlation-id", rabbitMsg.CorrelationId}};

            Model.QueueBind(queueResult.QueueName, rabbitMsg.ReplyTopic, string.Empty, specs);

            var waitReplyDelegate = new WaitReplyDelegate(WaitReply);

            var asyncresult = waitReplyDelegate.BeginInvoke(rabbitMsg, queueResult.QueueName, timeout, null, null);

            Model.BasicPublish(rabbitMsg.Topic, rabbitMsg.RoutingKey, rabbitMsg.Properties, rabbitMsg.Body);

            asyncresult.AsyncWaitHandle.WaitOne();

            return waitReplyDelegate.EndInvoke(asyncresult);
        }

        private Message WaitReply(RabbitMqMessage originalPublishedMessage, string replyQueue, TimeSpan timeout)
        {
            var timeoutAt = DateTime.Now.Add(timeout);

            while (true)
            {
                if (DateTime.Now > timeoutAt)
                {
                    throw new TimeoutException();
                }

                var reply = Model.BasicGet(replyQueue, false);

                if (reply == null)
                {
                    continue;
                }
                if (reply.BasicProperties.CorrelationId != originalPublishedMessage.CorrelationId)
                {
                    continue;
                }


                Model.BasicAck(reply.DeliveryTag, false);

                Model.QueueDelete(replyQueue);

                return reply.AsRabbitMqMessage().AsWcfMessage(
                    Parent.MaxReceivedMessageSize, 
                    Parent.BufferManager, 
                    Parent.EncoderFactory);
            }
        }

        //TODO: Temporary message
/*
        private static Message ConstructVoidReplyMessage(Message message)
        {
            var body = message.GetReaderAtBodyContents();

            var inputUri = new StringReader($"<{body.Name}Response xmlns='{body.NamespaceURI}' />");

            var xmlReader = XmlReader.Create(inputUri);

            var bodyReader = XmlDictionaryReader.CreateDictionaryReader(xmlReader);

            var reply = Message.CreateMessage(
                message.Version,
                message.Headers.Action + "Response",
                bodyReader);
            return reply;
        }
*/
        public IAsyncResult BeginRequest(Message message, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginRequest(System.ServiceModel.Channels.Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public Message EndRequest(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
    }
}