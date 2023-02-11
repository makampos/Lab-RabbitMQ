using System.Text;
using Common;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class Program
{
    private static ConnectionFactory _factory;
    private static IConnection _connection;
    private static EventingBasicConsumer _consumer;
    
    private const string ExchangeName = "PublishSubscribe_Exchange";
    
    public static void Main(string[] args)
    {
        _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
        using (_connection = _factory.CreateConnection())
        {
            using (var channel = _connection.CreateModel())
            {
                var queueName = DeclareAndBindQueueToExchange(channel);
                channel.BasicConsume(queue: queueName, autoAck: true, consumer: _consumer);

                _consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    var obj = JsonConvert.DeserializeObject<Payment>(message);
                    Console.WriteLine(" [-] Payment Message Received : {0} : {1} : {2}", obj.CardNumber, obj.AmountToPay, obj.Name);  
                };
            }
        }
    }
    
    private static string DeclareAndBindQueueToExchange(IModel channel)
    {
        channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Fanout);
        var queueName = channel.QueueDeclare().QueueName;
        channel.QueueBind(queue: queueName, exchange: ExchangeName, string.Empty);
        _consumer = new EventingBasicConsumer(channel);
        return queueName;
    }
    
}