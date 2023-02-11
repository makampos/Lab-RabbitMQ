using System.Text;
using Common;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class Program
{
    private static ConnectionFactory _factory;
    private static IConnection _connection;
        
    private const string QueueName = "WorkerQueue_Queue";
    
    public static void Main(string[] args)
    {
        Receive();

        Console.ReadLine();
    }
    public static void Receive()
    {
         _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
         using (_connection = _factory.CreateConnection())
         {
             using (var channel = _connection.CreateModel())
             {
                 channel.QueueDeclare(QueueName, true, false, false, null);
                 channel.BasicQos(0, 1, false);

                 var consumer = new EventingBasicConsumer(channel);
                 channel.BasicConsume(QueueName, true, consumer);

                 consumer.Received += (model, ea) =>
                 {
                     var body = ea.Body.ToArray();
                     var message = Encoding.UTF8.GetString(body);
                     var obj = JsonConvert.DeserializeObject<Payment>(message);
                     channel.BasicAck(ea.DeliveryTag, false);
                     Console.WriteLine(" [-] Payment Message Received : {0} : {1} : {2}", obj.CardNumber, obj.AmountToPay, obj.Name);  
                 };
             };
         };
    }
}