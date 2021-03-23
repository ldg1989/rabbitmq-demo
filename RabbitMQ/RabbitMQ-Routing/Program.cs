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
        channel.ExchangeDeclare(exchange: "direct_logs",
                                type: "direct");

        var severity = (args.Length > 0) ? args[0] : "info";
        var message = (args.Length > 1)
                      ? string.Join(" ", args.Skip(1).ToArray())
                      : "Hello World!";
        var body = Encoding.UTF8.GetBytes(message);

        for (int i = 0; i < 100; i++)
        {
          channel.BasicPublish(exchange: "direct_logs",
                            routingKey: severity,
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
