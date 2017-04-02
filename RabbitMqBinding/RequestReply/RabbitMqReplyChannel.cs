using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Threading;
using RabbitMqBinding.Base;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace RabbitMqBinding.RequestReply
{
    public class RabbitMqReplyChannel : RabbitMqChannelBase, IReplyChannel
    {
        internal readonly TimeSpan _DefaultTimeout = TimeSpan.MaxValue;
        private int _GetInterval = 1000;
        private RabbitMqMessage _CurrentRabbitMqMessage;
   
        internal RabbitMqChannelListener Parent { get; }


        public RabbitMqReplyChannel(RabbitMqChannelListener parent) : base(parent)
        {
            Parent = parent;
            LocalAddress =  new EndpointAddress(parent.Uri);

        }

        public RequestContext ReceiveRequest()
        {
            return ReceiveRequest(_DefaultTimeout);
        }

        public RequestContext ReceiveRequest(TimeSpan timeout)
        {
            var message = _CurrentRabbitMqMessage.AsWcfMessage(Parent.MaxReceivedMessageSize, Parent.BufferManager, Parent.EncoderFactory);

            var rabbitMqRequestContext = new RabbitMqRequestContext(message, this, _CurrentRabbitMqMessage);

            Model.BasicAck(_CurrentRabbitMqMessage.DeliveryTag, false);

            return rabbitMqRequestContext;

        }

        public IAsyncResult BeginReceiveRequest(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public RequestContext EndReceiveRequest(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public bool TryReceiveRequest(TimeSpan timeout, out RequestContext context)
        {
            context = null;
            var complete = WaitForRequest(timeout);

            if (!complete)
                return false;

            context = ReceiveRequest(DefaultReceiveTimeout);

            return true;
        }

        public IAsyncResult BeginTryReceiveRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return BeginWaitForRequest(timeout, callback, state);
        }

        public bool EndTryReceiveRequest(IAsyncResult result, out RequestContext context)
        {
            context = null;

            var complete = EndWaitForRequest(result);

            if (!complete)
            {
                return false;
            }

            context = ReceiveRequest(DefaultReceiveTimeout);

            return true;
        }

        public bool WaitForRequest(TimeSpan timeout)
        {
            ThrowIfDisposedOrNotOpen();

            var basicGet = Model.BasicGet(ServiceConfiguration.ServiceQueue, false);

            while (basicGet == null)
            {
                Thread.Sleep(_GetInterval);
                basicGet = Model.BasicGet(ServiceConfiguration.ServiceQueue, false);
            }

            _CurrentRabbitMqMessage = basicGet.AsRabbitMqMessage();

            return true;
        }

        public IAsyncResult BeginWaitForRequest(TimeSpan timeout, AsyncCallback callback, object state)
        {
            ThrowIfDisposedOrNotOpen();

            var asyncResult = new GenericAsyncResult(false, state, timeout, callback, false);

            var consumer = new EventingBasicConsumer(Model);

            consumer.Received += (a, b) =>
            {
                asyncResult.Complete();

                _CurrentRabbitMqMessage = b.AsRabbitMqMessage();

                callback?.Invoke(asyncResult);
            };

            Model.BasicConsume(ServiceConfiguration.ServiceQueue, false, consumer);
            
            return asyncResult;
        }

        public bool EndWaitForRequest(IAsyncResult asyncResult)
        {
            var genericAsyncResult = asyncResult as GenericAsyncResult;

            var isTimedOut = genericAsyncResult != null && genericAsyncResult.IsTimedOut;

            (asyncResult as IDisposable)?.Dispose();

            return !isTimedOut;
        }

        public EndpointAddress LocalAddress { get; }
    }
}