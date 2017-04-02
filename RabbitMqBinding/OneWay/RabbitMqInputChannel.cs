using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using RabbitMqBinding.Base;

namespace RabbitMqBinding.OneWay
{
    class RabbitMqInputChannel : RabbitMqChannelBase, IInputChannel
    {
        private int _Timeout = 30;

        public RabbitMqInputChannel(ChannelManagerBase parent, EndpointAddress remoteAddress, Uri via, EndpointAddress localAddress) : base(parent)
        {
            RemoteAddress = remoteAddress;
            Via = via;
            LocalAddress = localAddress;
        }

        public EndpointAddress RemoteAddress { get; }
        public Uri Via { get; }
        public System.ServiceModel.Channels.Message Receive()
        {
            throw new NotImplementedException();
        }

        public System.ServiceModel.Channels.Message Receive(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginReceive(AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public System.ServiceModel.Channels.Message EndReceive(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public bool TryReceive(TimeSpan timeout, out System.ServiceModel.Channels.Message message)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginTryReceive(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public bool EndTryReceive(IAsyncResult result, out System.ServiceModel.Channels.Message message)
        {
            throw new NotImplementedException();
        }

        public bool WaitForMessage(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginWaitForMessage(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public bool EndWaitForMessage(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public EndpointAddress LocalAddress { get; }
    }
}