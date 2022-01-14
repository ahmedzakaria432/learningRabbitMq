﻿// See https://aka.ms/new-console-template for more information
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

Console.WriteLine("Hello, World!");
var factory = new ConnectionFactory() { Uri = new Uri("amqp://guest:guest@localhost:5672") };
var connection = factory.CreateConnection();
var channel = connection.CreateModel();
channel.ExchangeDeclare("chatApp", ExchangeType.Direct, true);
var queueName = $"queue{ new Random().Next()}";
channel.QueueDeclare(queueName, true, false);

Console.WriteLine("enter room name");
var roomName= Console.ReadLine();


channel.QueueBind(queueName, "chatApp", roomName);
var consumer = new EventingBasicConsumer(channel);
consumer.Received += (sender, e) =>
{
    Console.WriteLine(System.Text.Encoding.UTF8.GetString(e.Body.ToArray()));
};
channel.BasicConsume(queueName, true, consumer);

Console.Write("message: ");
var message = Console.ReadLine();
while (!string.IsNullOrEmpty(message))
{
    Console.Write("message: ");
    message = Console.ReadLine();

    channel.BasicPublish("chatApp", roomName, null, System.Text.Encoding.UTF8.GetBytes(message));
}