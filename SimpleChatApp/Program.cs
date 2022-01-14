// See https://aka.ms/new-console-template for more information
using RabbitMQ.Client ;
using RabbitMQ.Client.Events ;
Console.WriteLine("Hello, World!");


var factory = new ConnectionFactory() { Uri=new Uri("amqp://guest:guest@localhost:5672")};
var connection = factory.CreateConnection();
var channel= connection.CreateModel();
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

    channel.BasicPublish("chatApp","", null, System.Text.Encoding.UTF8.GetBytes(message));
}