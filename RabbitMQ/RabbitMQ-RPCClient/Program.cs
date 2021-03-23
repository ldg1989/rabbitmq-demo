using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;

namespace RabbitMQ_RPCClient
{
  class Program
  {
    static void Main(string[] args)
    {
      var rpcClient = new RpcClient();

      for (int i = 5; i < 20; i++)
      {
        Console.WriteLine(" [x] Requesting fib(5)");
        var response = rpcClient.Call("5");
        Console.WriteLine(" [.] Got '{0}'", response);

        Thread.Sleep(2000); 
      } 
      rpcClient.Close();
    }
  } 

  public class RpcClient
  {
    private readonly IConnection connection;
    private readonly IModel channel;
    private readonly string replyQueueName;
    private readonly EventingBasicConsumer consumer;
    private readonly BlockingCollection<string> respQueue = new BlockingCollection<string>();
    private readonly IBasicProperties props;

    public RpcClient()
    {
      var factory = new ConnectionFactory() { HostName = "localhost" };

      connection = factory.CreateConnection();
      channel = connection.CreateModel();
      replyQueueName = channel.QueueDeclare().QueueName;
      consumer = new EventingBasicConsumer(channel);

      props = channel.CreateBasicProperties();
      var correlationId = Guid.NewGuid().ToString();
      props.CorrelationId = correlationId;
      props.ReplyTo = replyQueueName;

      consumer.Received += (model, ea) =>
      {
        var body = ea.Body.ToArray();
        var response = Encoding.UTF8.GetString(body);
        if (ea.BasicProperties.CorrelationId == correlationId)
        {
          respQueue.Add(response);
        }
      };
    }

    public string Call(string message)
    {
      var messageBytes = Encoding.UTF8.GetBytes(message);
      channel.BasicPublish(
          exchange: "",
          routingKey: "rpc_queue",
          basicProperties: props,
          body: messageBytes);

      channel.BasicConsume(
          consumer: consumer,
          queue: replyQueueName,
          autoAck: true);

      return respQueue.Take();
    }

    public void Close()
    {
      connection.Close();
    }
  } 
}
