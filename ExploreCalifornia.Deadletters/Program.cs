// See https://aka.ms/new-console-template for more information
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


#region Main

Console.WriteLine("Hello, World!");
(var connection, var channel) = prepareForConnecting();
channel.ExchangeDeclare("DLX", ExchangeType.Direct, true);
channel.QueueDeclare("deadlettersQueue", true, false, false);

channel.QueueBind("deadlettersQueue", "webappExchange", "");
var consumer = new EventingBasicConsumer(channel);

consumer.Received += (sender, args) =>
{
    var message = System.Text.Encoding.UTF8.GetString(args.Body.ToArray());
    var deathReason = args.BasicProperties.Headers["x-first-death-reason"] as byte[];
    var stringDeathReason = deathReason is not null? System.Text.Encoding.UTF8.GetString(deathReason):"";
    Console.WriteLine($"DeadLetter:{ message} Reason:{stringDeathReason}");


};
channel.BasicConsume("deadlettersQueue", true, consumer);

Console.ReadLine();
exit(connection, channel);
#endregion




#region Utilities

void exit(IConnection connection, IModel channel)
{
    connection.Close();
    channel.Close();
    connection.Dispose();
    channel.Dispose();
}

(IConnection, IModel) prepareForConnecting()
{
    var factory = new ConnectionFactory() { Uri = new Uri("amqp://backoffice:backoffice@localhost:5672") };
    var connection = factory.CreateConnection();
    var channel = connection.CreateModel();
    
    return (connection, channel);
}
#endregion