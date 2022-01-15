// See https://aka.ms/new-console-template for more information
#region Usings

using RabbitMQ.Client ;
using RabbitMQ.Client.Events ;
#endregion
#region Main

Console.WriteLine("Hello, World!");


(var connection, var channel) = prepareForConnecting();
channel.ExchangeDeclare("chatApp",ExchangeType.Fanout,true);
var queueName = $"queue{ new Random().Next()}";
channel.QueueDeclare(queueName,true,false);
channel.QueueBind(queueName, "chatApp","");
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, e) =>
 { 
     Console.WriteLine(System.Text.Encoding.UTF8.GetString( e.Body.ToArray()));
 }; 
channel.BasicConsume(queueName,true,consumer);

while (true) 
{
   var message= Console.ReadLine();

    if (string.IsNullOrEmpty(message))
    {
        exit(connection,channel);
      
        break;
        
    }

    channel.BasicPublish("chatApp","", null, System.Text.Encoding.UTF8.GetBytes(message));
    
}

#endregion

#region Utilities

(IConnection, IModel) prepareForConnecting()
{
    var factory = new ConnectionFactory() { Uri = new Uri("amqp://guest:guest@localhost:5672") };
    var connection = factory.CreateConnection();
    var channel = connection.CreateModel();
    return (connection, channel);
}
void exit(IConnection connection, IModel channel)
{
    connection.Close();
    channel.Close();
    connection.Dispose();
    channel.Dispose();
}
#endregion