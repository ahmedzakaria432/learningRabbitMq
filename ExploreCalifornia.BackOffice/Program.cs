// See https://aka.ms/new-console-template for more information
#region Usings

using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

#endregion

#region Main

Console.WriteLine("Hello, World!");
(var connection, var channel) = prepareForConnecting();

//this to tell queue to send rejected message to dlx exhange (queue that we have defined to receive dead letterrd messages )
var dict = new Dictionary<string, object>
{
    { "x-dead-letter-exchange", "DLX" }
};
channel.QueueDeclare("backOfficeQueue", true, false, false,dict);
channel.QueueBind("backOfficeQueue", "webappExchange", "", new Dictionary<string, object>()
            {
                { "subject","tour"},

                { "x-match","any"}
            });
var consumer = new EventingBasicConsumer(channel);

consumer.Received += (sender, args) =>
{
    string message = System.Text.Encoding.UTF8.GetString(args.Body.ToArray());
    var content = JsonConvert.DeserializeObject<Obj>(message);
    var subject = System.Text.Encoding.UTF8.GetString((byte[])args.BasicProperties.Headers[key: "subject"]);
    var action = System.Text.Encoding.UTF8.GetString((byte[])args.BasicProperties.Headers[key: "action"]);

    print(obj: content, $"subject: {subject} \n action: {action}");
    channel.BasicReject(args.DeliveryTag, false);
};
channel.BasicConsume("backOfficeQueue", true, consumer);

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

void print(Obj obj, string additionalInfo)
{
    Console.WriteLine(additionalInfo);
    if (obj is not null)
    {
        Console.WriteLine(obj.ToString());
        return;
    }
    Console.WriteLine(" failed to deserialize content");

}
(IConnection, IModel) prepareForConnecting()
{
    var factory = new ConnectionFactory() { Uri = new Uri("amqp://guest:guest@localhost:5672") };
    var connection = factory.CreateConnection();
    var channel = connection.CreateModel();
    return (connection, channel);
}
#endregion

#region classes

public class Obj
{
    public string? tourname { get; set; }
    public string? name { get; set; }
    public string? email { get; set; }
    public string? cancelReason { get; set; }


    public override string ToString()
    {
        return $"tour Name :{tourname} \n  name:{name} \n email:{email} \n" + cancelReason ?? $"cancelReason: {cancelReason}";
    }

}
#endregion