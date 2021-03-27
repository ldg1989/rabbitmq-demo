using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;
using System.ServiceModel.Channels;
using RabbitMQ.Client.Events;

namespace RabbitMQ_Routing
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
        var arguments = new Dictionary<string, object>();
        arguments.Add("x-message-ttl", 5000);//过期时间是5秒
        arguments.Add("x-max-length", 8);//设置队列的最大数据长度
        arguments.Add("x-dead-letter-exchange", "dead-exchange");// 死信队列的参数
        arguments.Add("x-dead-letter-routing-key", "dead");//direct 路由设置 需要设置  fanout 不需要设置

        string exchangeName = "TTL-driect_message_exchange01";
        string routingKey = "ttl01";
        string queue = "normalQueue01";

        //创建死信队列
        CreateDead(channel, arguments);

        //声明一个交换机                       
        channel.ExchangeDeclare(exchangeName, "direct", durable: true, autoDelete: true, null);
        //声明队列
        channel.QueueDeclare(queue, durable: true, exclusive: false, autoDelete: true, arguments);

        //绑定交换机与队列的关系
        channel.QueueBind(queue, exchangeName, routingKey: routingKey);

        var severity = (args.Length > 0) ? args[0] : routingKey;
        var message = (args.Length > 1)
                      ? string.Join(" ", args.Skip(1).ToArray())
                      : "Hello World!";
        var body = Encoding.UTF8.GetBytes(message);
        ////未开启下次确认 
        //for (int i = 0; i < 10; i++)
        //{
        //  channel.BasicPublish(exchange: exchangeName, routingKey: routingKey,
        //                    basicProperties:null,
        //                    body: body); 


        //  Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);
        //  Thread.Sleep(2000);
        //}

        ////事务模式 
        //channel.TxSelect();//开启事务
        //try
        //{
        //  for (int i = 0; i < 10; i++)
        //  {
        //    channel.BasicPublish(exchange: exchangeName, routingKey: routingKey,
        //                      basicProperties: null,
        //                      body: body);

        //    Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);
        //    Thread.Sleep(2000);
        //  }
        //  channel.TxCommit();//提交事务
        //}
        //catch (Exception e)
        //{
        //  var erryMsg = e.StackTrace;
        //  channel.TxRollback();//事务回滚
        //}

        //开启消息确认 Confirm模式
        channel.ConfirmSelect();
        for (int i = 0; i < 10; i++)
        {
          channel.BasicPublish(exchange: exchangeName, routingKey: routingKey,
                            basicProperties: null,
                            body: body);
          if (!channel.WaitForConfirms())// 逐条 确认
          {
            Console.WriteLine(" 消息投递失败'{0}':'{1}'", severity, message);
          };
          Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);
          Thread.Sleep(2000);
        }
        //if (!channel.WaitForConfirms())// 多条一起 确认
        //{
        //  Console.WriteLine(" 消息投递失败'{0}':'{1}'", severity, message);
        //};





      }

      Console.WriteLine(" Press [enter] to exit.");
      Console.ReadLine();
    }

    /// <summary>
    /// 死信队列
    /// </summary>
    /// <param name="channel"></param>
    /// <param name="arguments"></param>
    /// <returns></returns>
    public static IModel CreateDead(IModel channel, Dictionary<string, object> arguments)
    {

      //声明一个交换机                       
      channel.ExchangeDeclare("dead-exchange", "direct", durable: true, autoDelete: true, null);
      //声明队列
      channel.QueueDeclare("dead-exchange-queue", durable: true, exclusive: false, autoDelete: true, null);

      //绑定交换机与队列的关系
      channel.QueueBind("dead-exchange-queue", "dead-exchange", routingKey: "dead");

      return channel;
    }
  }


  /// <summary>
  /// 消费端
  /// </summary>
  class Consumer
  {

    static void Main(string[] args)
    {
      ulong tag = 1;

      // 使用命令启动 cmd 到当前目录  dotnet run 【参数】
      var factory = new ConnectionFactory() { HostName = "localhost" };
      using (var connection = factory.CreateConnection())
      using (var channel = connection.CreateModel())
      {
        channel.ExchangeDeclare(exchange: "driect_message_exchange", type: "ttl");
        var queueName = channel.QueueDeclare().QueueName;

        string routingKey = "ttl01";
        channel.QueueBind(queue: queueName, exchange: "driect_message_exchange", routingKey: routingKey);

        Console.WriteLine(" [*] Waiting for messages.");

        var consumer = new EventingBasicConsumer(channel);
        try
        {
          consumer.Received += (model, ea) =>
          {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var routingKey = ea.RoutingKey;
            // 应答机制
            channel.BasicAck(deliveryTag: tag, multiple: false);

            Console.WriteLine(" [x] Received '{0}':'{1}'", routingKey, message);
          };
          channel.BasicConsume(queue: queueName, autoAck: false, consumer: consumer);
        }
        catch (Exception ex)
        {
          // 参数        消息标识  单挑  不重发 
          channel.BasicNack(deliveryTag: tag, multiple: false, requeue: false);
        }

        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
      }
    }
  }
}
