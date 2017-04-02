using System;
using System.ServiceModel.Channels;
using RabbitMQ.Client;


namespace RabbitMqBinding.Base
{
    public abstract class RabbitMqChannelBase : ChannelBase, IDisposable
    {
        internal IConnection Connection { get; }
        protected TimeSpan _Timeout = TimeSpan.MaxValue;
        public IModel Model { get; }

        protected RabbitMqChannelBase(ChannelManagerBase parent) : base(parent)
        {
            Connection = new ConnectionFactory().CreateConnection();
            Model = Connection.CreateModel();
        }

        protected override void OnAbort()
        {
            
        }

        protected override void OnClose(TimeSpan timeout)
        {

        }

        protected override void OnEndClose(IAsyncResult result)
        {
            
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            
            _Timeout = timeout;
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            
            Connection?.Close();
            Connection?.Dispose();          
        }
    }
}