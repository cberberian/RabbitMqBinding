using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using RabbitMqBinding.RequestReply;
using RabbitMQ.Client;
using RabbitMQ.Client.Framing;

namespace RabbitMqBinding.Base
{
    class RabbitMqRequestContext : RequestContext
    {
        private readonly RabbitMqReplyChannel _Parent;
        private readonly RabbitMqMessage _CurrentMqMessage;
        private bool _Aborted;
        private CommunicationState _State;
        private readonly object _ThisLock;
        private IModel _Model;

        public RabbitMqRequestContext(Message message, RabbitMqReplyChannel parent, RabbitMqMessage currentMqMessage)
        {
            
            RequestMessage = message;
            _Parent        = parent;
            _CurrentMqMessage = currentMqMessage;
            _Aborted       = false;
            _State         = CommunicationState.Opened;
            _ThisLock      = new object();
            _Model = _Parent.Connection.CreateModel();
        }

        public override Message RequestMessage { get; }

        public override void Abort()
        {
            lock (_ThisLock)
            {
                if (_Aborted)
                {
                    return;
                }
                _Aborted = true;
                _State = CommunicationState.Faulted;
            }
        }

        public override void Close()
        {
            Close(_Parent._DefaultTimeout);
        }

        public override void Close(TimeSpan timeout)
        {
            lock (_ThisLock)
            {
                _State = CommunicationState.Closed;
            }
        }

        public override void Reply(Message message)
        {
            Reply(message, _Parent._DefaultTimeout);
        }

        public override void Reply(Message message, TimeSpan timeout)
        {
            lock (_ThisLock)
            {
                if (_Aborted)
                {
                    throw new CommunicationObjectAbortedException();
                }
                if (_State == CommunicationState.Faulted)
                {
                    throw new CommunicationObjectFaultedException();
                }
                if (_State == CommunicationState.Closed)
                {
                    throw new ObjectDisposedException("this");
                }
            }
            
            var rabbitMsg = message.AsRabbitMqMessage();

            rabbitMsg.Properties.CorrelationId = _CurrentMqMessage.CorrelationId;
            rabbitMsg.Properties.Headers.Add("correlation-id", _CurrentMqMessage.CorrelationId);

            _Model.BasicPublish(rabbitMsg.ReplyTopic, rabbitMsg.ReplyRoutingKey, rabbitMsg.Properties, rabbitMsg.Body);

        }

        public override IAsyncResult BeginReply(Message message, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public override IAsyncResult BeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public override void EndReply(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

    }
}