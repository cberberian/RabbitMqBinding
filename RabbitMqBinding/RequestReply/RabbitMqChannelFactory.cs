using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace RabbitMqBinding.RequestReply
{
    public class RabbitMqChannelFactory : ChannelFactoryBase<IRequestChannel>
    {
        public MessageEncoderFactory EncoderFactory { get; }
        public BufferManager BufferManager { get; }

        public RabbitMqChannelFactory(BindingContext context, RabbitMqTransportBindingElement transportElement) : base(context.Binding)
        {
            var messageElement     = context.BindingParameters.Remove<MessageEncodingBindingElement>();
            EncoderFactory         = messageElement.CreateMessageEncoderFactory();
            MaxReceivedMessageSize = transportElement.MaxReceivedMessageSize;
            BufferManager          = BufferManager.CreateBufferManager(transportElement.MaxBufferPoolSize, (int)MaxReceivedMessageSize);
        }

        public long MaxReceivedMessageSize { get; set; }

        protected override IRequestChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            return new RabbitMqRequestChannel(this, address, via);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            throw new NotImplementedException();
        }
    }
}