using System;
using System.Diagnostics;
using System.ServiceModel.Channels;

namespace RabbitMqBinding.RequestReply
{
    public class RabbitMqChannelListener : ChannelListenerBase<IReplyChannel>
    {
        private RabbitMqReplyChannel _ReplyChannel;
        private GenericAsyncResult _AcceptChannelAsyncResult;
        internal MessageEncoderFactory EncoderFactory;
        public long MaxReceivedMessageSize;
        public BufferManager BufferManager;

        public RabbitMqChannelListener(BindingContext context, RabbitMqTransportBindingElement transportElement)
        {
            Debug.Assert(transportElement.MaxReceivedMessageSize > 0);

            MaxReceivedMessageSize = transportElement.MaxReceivedMessageSize;
            var messageElement     = context.BindingParameters.Remove<MessageEncodingBindingElement>();
            BufferManager          = BufferManager.CreateBufferManager(transportElement.MaxBufferPoolSize, (int)MaxReceivedMessageSize);
            EncoderFactory         = messageElement.CreateMessageEncoderFactory();
            Uri                    = new Uri(context.ListenUriBaseAddress, context.ListenUriRelativeAddress);
        }

        protected override void OnAbort()
        {
            
        }

        protected override void OnClose(TimeSpan timeout)
        {
            if (_AcceptChannelAsyncResult == null)
            {
                return;
            }

            _AcceptChannelAsyncResult.Complete();

            _AcceptChannelAsyncResult.Callback?.Invoke(_AcceptChannelAsyncResult);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
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

        protected override bool OnWaitForChannel(TimeSpan timeout)
        {
            throw new NotImplementedException();
        }

        protected override IAsyncResult OnBeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        protected override bool OnEndWaitForChannel(IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public override Uri Uri { get; }
        protected override IReplyChannel OnAcceptChannel(TimeSpan timeout)
        {
            return _ReplyChannel ?? (_ReplyChannel = new RabbitMqReplyChannel(this));
        }

        protected override IAsyncResult OnBeginAcceptChannel(TimeSpan timeout, AsyncCallback callback, object state)
        {
            //first time thru (_reply channel is not initialized) so call the callback. 
            if (_ReplyChannel == null)
            {
                _AcceptChannelAsyncResult = new GenericAsyncResult(true, state, timeout, callback, true) { AsyncState = state, Timeout = timeout };
                callback?.Invoke(_AcceptChannelAsyncResult);
            }
            else
            {
                _AcceptChannelAsyncResult = new GenericAsyncResult(true, state, timeout, callback, false) { AsyncState = state, Timeout = timeout };
                
            }

            return _AcceptChannelAsyncResult;
        }

        protected override IReplyChannel OnEndAcceptChannel(IAsyncResult result)
        {
            var asyncResult = result as GenericAsyncResult;

            return asyncResult != null ? OnAcceptChannel(asyncResult.Timeout) : null;
        }

    }
}   