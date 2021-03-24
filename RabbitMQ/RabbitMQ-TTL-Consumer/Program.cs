﻿using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RabbitMQ_Routing_Consumer
{
  class Program
  {
    static void Main(string[] args)
    {
      // 使用命令启动 cmd 到当前目录  dotnet run 【参数】
      var factory = new ConnectionFactory() { HostName = "localhost" };
      using (var connection = factory.CreateConnection())
      using (var channel = connection.CreateModel())
      {
        channel.ExchangeDeclare(exchange: "driect_message_exchange", type: "direct");
        var queueName = channel.QueueDeclare().QueueName;

        if (args.Length < 1)
        {
          Console.Error.WriteLine("Usage: {0} [info] [warning] [error]", Environment.GetCommandLineArgs()[0]);
          Console.WriteLine(" Press [enter] to exit.");
          Console.ReadLine();
          Environment.ExitCode = 1;
          return;
        }

        foreach (var severity in args)
        {
          channel.QueueBind(queue: queueName, exchange: "driect_message_exchange", routingKey: severity);
        }

        Console.WriteLine(" [*] Waiting for messages.");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
          var body = ea.Body.ToArray();
          var message = Encoding.UTF8.GetString(body);
          var routingKey = ea.RoutingKey;
          Console.WriteLine(" [x] Received '{0}':'{1}'", routingKey, message);
        };
        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
      } 
    }
  }
}

