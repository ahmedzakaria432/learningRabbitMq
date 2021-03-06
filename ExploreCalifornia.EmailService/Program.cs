// See https://aka.ms/new-console-template for more information
#region Usings

using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
#endregion

#region Main

Console.WriteLine("Hello, World!");

(var connection, var channel) = prepareForConnecting();

channel.QueueDeclare("EmailSeviceQueue",true,false,false);
        channel.QueueBind("EmailSeviceQueue", "webappExchange", "", new Dictionary<string, object>()
            {
                { "subject","tour"},
                { "action","booked"},
                { "x-match","all"}
            });
        var consumer=new EventingBasicConsumer(channel);
        consumer.Received += (sender, args) =>
          {
              var objString=System.Text.Encoding.UTF8.GetString(args.Body.ToArray());
              var jsonObj = JsonConvert.DeserializeObject<Obj>(objString);
              var subject = System.Text.Encoding.UTF8.GetString((byte[])args.BasicProperties.Headers[key: "subject"]);
              var action = System.Text.Encoding.UTF8.GetString((byte[])args.BasicProperties.Headers[key: "action"]);
              Console.WriteLine($"subject: {subject} \n action: {action}");
              print(jsonObj);

          };
        channel.BasicConsume("EmailSeviceQueue", true, consumer);
        
    
    Console.ReadLine();
    exit(connection, channel);
#endregion


#region Utilities

void exit( IConnection connection, IModel channel) {
    connection.Close();
    channel.Close();
    connection.Dispose();
    channel.Dispose();
}

void print(Obj obj ) 
{
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

#region Classes

public class Obj 
{
    public string? tourname { get; set; }
    public string? name { get; set; }
    public string? email { get; set; }
    public string? cancelReason { get; set; }


    public override string ToString()
    {
        return $"tour Name :{tourname} \n  name:{name} \n email:{email} \n"+ cancelReason?? $"cancelReason: {cancelReason}";
    }

}
#endregion