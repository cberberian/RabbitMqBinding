using System;
using System.ServiceModel;
using Contracts;
using RabbitMqBinding;
using ServiceConfiguration = RabbitMqBinding.ServiceConfiguration;

namespace testhost
{
    class Program
    {
        static void Main(string[] args)
        {

            using (var host = ServiceConfiguration.BuildServiceHost<IHelloWorldService, HelloWorldService>("rabbitmq://localhost:8080/hello"))
            {
                Console.WriteLine("The service is ready at {0}", ServiceConfiguration.ServiceTopicExchange);
                Console.WriteLine("Press <Enter> to stop the service.");
                Console.ReadLine();

                // Close the ServiceHost.
                host.Close();
            }
        }
    }
}
