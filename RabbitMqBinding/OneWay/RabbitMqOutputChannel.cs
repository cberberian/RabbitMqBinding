using System;
using System.ServiceModel;
using System.ServiceModel.Channels;
using RabbitMqBinding.Base;

namespace RabbitMqBinding.OneWay
{
    class RabbitMqOutputChannel : RabbitMqChannelBase, IOutputChannel
    {
        public RabbitMqOutputChannel(ChannelManagerBase parent, EndpointAddress remoteAddress, Uri via) : base(parent)
        {
            RemoteAddress = remoteAddress;
            Via = via;
        }

        public void Send(System.ServiceModel.Channels.Message message)
        {
            throw new NotImplementedException();
        }

        public void Send(System.ServiceModel.Channels.Message message, TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginSend(System.ServiceModel.Channels.Message message, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public IAsyncResult BeginSend(System.ServiceModel.Channels.Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public void EndSend(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public EndpointAddress RemoteAddress { get; }
        public Uri Via { get; }
    }
}