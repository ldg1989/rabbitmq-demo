using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;
using System.Collections.Generic;



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
        var arguments = new Dictionary<string, object>() ;
        arguments.Add("x-dead-letter-exchange",5000);//过期时间是5秒

        string exchangeName = "TTL-driect_message_exchange";
       
        //声明一个交换机                       
        channel.ExchangeDeclare(exchange: exchangeName,// 交换机的名字 
                                    type: "direct", // 交换机的类型
                                    true);// 是否持久化
   

        channel.QueueDeclare(queue: "TTLQueue",// 通道
                             durable: false, //是否 持久化
                             exclusive: false,
                             autoDelete: false,
                             arguments: arguments);

        string message = "Hello World!";

        for (int i = 0; i < 10; i++)
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
