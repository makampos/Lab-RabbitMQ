using System.Text;
using Common;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class Program
{
    private static ConnectionFactory _factory;
    private static IConnection _connection;

    private const string ExchangeName = "DirectRouting_Exchange";
    private const string PurchaseOrderQueueName = "PurchaseOrderDirectRouting_Queue";

    static void Main()
    {
        _factory = new ConnectionFactory { HostName = "localhost", UserName = "guest", Password = "guest" };
        _connection = _factory.CreateConnection();

        var model = _connection.CreateModel();

        model.ExchangeDeclare(ExchangeName, "direct");
        model.QueueDeclare(PurchaseOrderQueueName, true, false, false, null);
        model.QueueBind(PurchaseOrderQueueName, ExchangeName, "CardPayment");

        var consumer = new EventingBasicConsumer(model);
        model.BasicConsume(PurchaseOrderQueueName, true, consumer);


        consumer.Received += (ob, ea) =>
        {
            var body = ea.Body.ToArray();
            var obj = JsonConvert.DeserializeObject<Payment>(Encoding.UTF8.GetString(body));
            var routingKey = ea.RoutingKey;
            Console.WriteLine(" [-] Purchase Order - Routing Key <{0}> : {1} : {2} : {3}", routingKey, obj.CardNumber, obj.AmountToPay, obj.Name);
        };
    }
}