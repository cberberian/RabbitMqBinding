﻿using System;
using System.ServiceModel.Channels;

namespace RabbitMqBinding.RequestReply
{
    public class RabbitMqTransportBindingElement : TransportBindingElement
    {

        public RabbitMqTransportBindingElement()
        {
        }

        public RabbitMqTransportBindingElement(RabbitMqTransportBindingElement rabbitMqTransportBindingElement)
        {
            
        }

        public override BindingElement Clone()
        {
            return new RabbitMqTransportBindingElement(this);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            return typeof(TChannel) == typeof(IRequestChannel);
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            return typeof(TChannel) == typeof(IReplyChannel);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (!CanBuildChannelFactory<TChannel>(context))
            {
                throw new ArgumentException($"Unsupported channel type: {typeof(TChannel).Name}.");
            }
            return (IChannelFactory<TChannel>) new RabbitMqChannelFactory(context, this);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }
            if (!CanBuildChannelListener<TChannel>(context))
            {
                throw new ArgumentException($"Unsupported channel type: {typeof(TChannel).Name}.");
            }
            return (IChannelListener<TChannel>)new RabbitMqChannelListener(context, this);
        }

        public override string Scheme => "rabbitmq";
    }
}