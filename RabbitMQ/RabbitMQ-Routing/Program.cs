using RabbitMQ.Client;
using System;
using System.Linq;
using System.Text;
using System.Threading;

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
        string exchangeName = "driect_message_exchange";
        string routingKey = "driect";

        //声明一个交换机                       
        channel.ExchangeDeclare(exchange: exchangeName,// 交换机的名字 
                                    type: "direct", // 交换机的类型
                                    true);// 是否持久化
        //声明队列
        channel.QueueDeclare(queue: "queue5", true, exclusive: false, autoDelete: false, null);

        //绑定交换机与队列的关系
        channel.QueueBind("queue5", exchangeName, routingKey, null);



        var severity = (args.Length > 0) ? args[0] : routingKey;
        var message = (args.Length > 1)
                      ? string.Join(" ", args.Skip(1).ToArray())
                      : "Hello World!";
        var body = Encoding.UTF8.GetBytes(message);

        for (int i = 0; i < 100; i++)
        {
          channel.BasicPublish(exchange: exchangeName,
                            routingKey: severity,//路由key
                            basicProperties: null,
                            body: body);
          Console.WriteLine(" [x] Sent '{0}':'{1}'", severity, message);
          Thread.Sleep(2000);
        }
      }

      Console.WriteLine(" Press [enter] to exit.");
      Console.ReadLine();
    }
  }
}
