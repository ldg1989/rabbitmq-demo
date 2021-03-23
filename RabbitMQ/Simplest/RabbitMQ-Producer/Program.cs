using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;

namespace RabbitMQ_Producer
{
  class Program
  {
    static void Main(string[] args)
    {
      var factory = new ConnectionFactory() { HostName = "localhost" };
      using (var connection = factory.CreateConnection())
      using (var channel = connection.CreateModel())
      {
        channel.QueueDeclare(queue: "hello",// 通道
                             durable: false, //是否 持久化
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        string message = "Hello World!";

        for (int i = 0; i < 50; i++)
        {
          Thread.Sleep(2000);
          message = message + DateTime.Now;
          var body = Encoding.UTF8.GetBytes(message);

          channel.BasicPublish(exchange: "",
                               routingKey: "hello",
                               basicProperties: null,
                               body: body);
          Console.WriteLine(" [x] Sent {0}", message);

        }
      }

      Console.WriteLine(" Press [enter] to exit.");
      Console.ReadLine();


      Console.WriteLine("Hello World!");
    }
  }
}
