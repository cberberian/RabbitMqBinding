using System.ServiceModel.Channels;

namespace RabbitMqBinding.RequestReply
{
    public class RabbitMqTransportBinding : Binding
    {
        readonly MessageEncodingBindingElement messageElement;
        readonly RabbitMqTransportBindingElement transportElement;

        public RabbitMqTransportBinding()
        {
            messageElement = new TextMessageEncodingBindingElement();
            transportElement = new RabbitMqTransportBindingElement();
        }

        public override BindingElementCollection CreateBindingElements()
        {
            return new BindingElementCollection(new BindingElement[] {
                messageElement,
                transportElement
            });
        }

        public override string Scheme => transportElement.Scheme;
    }
}
