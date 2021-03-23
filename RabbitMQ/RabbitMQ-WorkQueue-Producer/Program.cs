using RabbitMQ.Client;
using System;
using System.Text;

namespace RabbitMQ_WorkQueue_Producer
{
  class Program
  {
    /// <summary>
    /// 轮训分发，
    /// </summary>
    /// <param name="args"></param>
    static void Main(string[] args)
    {

      var factory = new ConnectionFactory() { HostName = "localhost" };
      using (var connection = factory.CreateConnection())
      using (var channel = connection.CreateModel())
      {
        channel.QueueDeclare(queue: "task_queue",
                             durable: true,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        for (int i = 0; i < 50; i++)
        {
          var message = GetMessage(args);
          var body = Encoding.UTF8.GetBytes(message + "编号：" + i);

          var properties = channel.CreateBasicProperties();
          properties.Persistent = true;

          channel.BasicPublish(exchange: "",
                               routingKey: "task_queue",
                               basicProperties: properties,
                               body: body);
          Console.WriteLine(" [x] Sent {0}", message);
        }

      }

      Console.WriteLine(" Press [enter] to exit.");
      Console.ReadLine();
    }

    private static string GetMessage(string[] args)
    {
      return ((args.Length > 0) ? string.Join(" ", args) : "Hello World!");
    }
  }
}
