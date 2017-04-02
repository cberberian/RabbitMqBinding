using System;
using System.ServiceModel;

namespace Contracts
{
    [ServiceContract]
    public interface IHelloWorldService
    {
        [OperationContract]
        void SayHello(string name);

        [OperationContract]
        string ReturnHello(string name);
    }

    [ServiceBehavior(IncludeExceptionDetailInFaults = true)]
    public class HelloWorldService : IHelloWorldService
    {
        public void SayHello(string name)
        {
            Console.WriteLine($"Hello, {name}");
        }

        public string ReturnHello(string input)
        {
            return $"return hello says: {input}";
        }
    }
}
