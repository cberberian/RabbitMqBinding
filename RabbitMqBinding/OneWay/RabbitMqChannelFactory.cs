using System;
using System.ServiceModel;
using System.ServiceModel.Channels;

namespace RabbitMqBinding.OneWay
{
    class RabbitMqChannelFactory : ChannelFactoryBase<IInputChannel>
    {

        public RabbitMqChannelFactory(BindingContext context) : base(context.Binding)
        {
            
        }

        protected override IInputChannel OnCreateChannel(EndpointAddress address, Uri via)
        {
            return new RabbitMqInputChannel(this, address, via, address);
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