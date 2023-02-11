using System.Text;
using Common;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

internal class Program
{
    private static ConnectionFactory _factory;
    private static IConnection _connection;
    private static IModel _model;
    
    private const string QueueName = "StandardQueue";
    
    public static void Main(string[] args)
    {
        var payment1 = new Payment { AmountToPay = 25.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
        var payment2 = new Payment { AmountToPay = 5.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
        var payment3 = new Payment { AmountToPay = 2.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
        var payment4 = new Payment { AmountToPay = 17.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
        var payment5 = new Payment { AmountToPay = 300.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
        var payment6 = new Payment { AmountToPay = 350.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
        var payment7 = new Payment { AmountToPay = 295.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
        var payment8 = new Payment { AmountToPay = 5625.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
        var payment9 = new Payment { AmountToPay = 5.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
        var payment10 = new Payment { AmountToPay = 12.0m, CardNumber = "1234123412341234", Name = "Mr S Haunts" };
        
        CreateQueue();

        SendMessage(payment1);
        SendMessage(payment2);
        SendMessage(payment3);
        SendMessage(payment4);
        SendMessage(payment5);
        SendMessage(payment6);
        SendMessage(payment7);
        SendMessage(payment8);
        SendMessage(payment9);
        SendMessage(payment10);
        
        Receive();

        Console.ReadLine();
    }
    
    private static void CreateQueue()
    {
        _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest"};
        _connection = _factory.CreateConnection();
        _model = _connection.CreateModel(); 
                     
        _model.QueueDeclare(QueueName, true, false, false, null);            
    }
    
    private static void SendMessage(Payment message)
    {            
        _model.BasicPublish("", QueueName, null, message.Serialize());
        Console.WriteLine(" [V] Payment Message Sent : {0} : {1} : {2}", message.CardNumber, message.AmountToPay, message.Name);            
    }

    public static void Receive()
    {
        var consumer = new EventingBasicConsumer(_model);
        var msgCount = GetMessageCount(_model, QueueName);
        _model.BasicConsume(QueueName, true, consumer);
        
        Console.WriteLine(" [*] Waiting for messages.  Message Count : {0}", msgCount);
        
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var obj = JsonConvert.DeserializeObject<Payment>(message);
            Console.WriteLine(" [-] Payment Message Received : {0} : {1} : {2}", obj.CardNumber, obj.AmountToPay, obj.Name);  
        };
    }

    private static uint GetMessageCount(IModel channel, string queueName) => channel.QueueDeclare(queueName, true, false, false, null).MessageCount;
}