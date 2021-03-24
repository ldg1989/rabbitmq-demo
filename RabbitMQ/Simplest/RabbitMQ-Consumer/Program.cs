using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Collections.Generic;

namespace RabbitMQ_Consumer
{
  class Program
  {
    static void Main(string[] args)
    {
      var factory = new ConnectionFactory() { HostName = "localhost" };
      using (var connection = factory.CreateConnection())
      using (var channel = connection.CreateModel())
      {
        var arguments = new Dictionary<string, object>();
        arguments.Add("x-dead-letter-exchange", 5000);//过期时间是5秒
        channel.QueueDeclare(queue: "TTLQueue",
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: arguments);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
          var body = ea.Body.ToArray();
          var message = Encoding.UTF8.GetString(body);
          Console.WriteLine(" [x] Received {0}", message);
        };
        channel.BasicConsume(queue: "TTLQueue",
                             autoAck: true,
                             consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
      }



      Console.WriteLine("Hello World!");
    }
  }
}
