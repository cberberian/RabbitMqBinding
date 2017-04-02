using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;
using Contracts;
using RabbitMqBinding;
using RabbitMqBinding.RequestReply;

namespace testclient
{
    class Program
    {
        static void Main(string[] args)
        {
            while (true)
            {
                var input = Console.ReadLine();

                if (!input.IsNotQuit())
                    break;
                int count;

                if (int.TryParse(input, out count))
                    RunAsynch(count);
                else 
                    Console.WriteLine("bad input");
            }

        }


        private static void RunAsynch(int threadCount)
        {
            

            var startTime = DateTime.Now;
            var endTime = DateTime.Now.AddSeconds(30);
            var totalCalls = 0;

            var myBinding = new RabbitMqTransportBinding();
            var myEndpoint = new EndpointAddress("rabbitmq://localhost:8080/hello");
            var myChannelFactory = new ChannelFactory<IHelloWorldService>(myBinding, myEndpoint);

            var clients = new List<IHelloWorldService>();

            for (var a = 0; a < threadCount; a++)
            {
                clients.Add(myChannelFactory.CreateChannel());
            }
            var aClients = clients.ToArray();
            while (DateTime.Now < endTime)
            {
                try
                {
                    var threads = new List<Thread>();
                    for (var a = 0; a < threadCount; a++)
                    {
                        var a1 = a;
                        var thread = new Thread(() =>
                        {
                            aClients[a1].SayHello($"Hello {totalCalls++}");
                        });
                        threads.Add(thread);
                        thread.Start();
                    }
                    foreach (var thread in threads)
                    {
                        thread.Join();
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e}\r\n\r\n");
                }
            }

            var duration = DateTime.Now.Subtract(startTime);

            Console.WriteLine($"\r\nDuration: {duration.TotalSeconds}\r\nTotal Calls: {totalCalls}\r\nCalls Per sec: {totalCalls / duration.TotalSeconds}");


        }

    }
}
