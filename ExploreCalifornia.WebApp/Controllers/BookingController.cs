using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace ExploreCalifornia.WebApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        #region Utilites
        public void sendMessage(string routingKey, object message)
        {
            var factory = new ConnectionFactory() { HostName = "localhost", Uri = new Uri("amqp://guest:guest@localhost:5672") };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("webappExchange", ExchangeType.Headers, true);
                 
                    var json = JsonConvert.SerializeObject(message);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                    channel.BasicPublish("webappExchange", routingKey, null, bytes);
                    channel.Close();
                    connection.Close();

                }
            }
        }

        public void sendMessageWithHeaderExxhange(IDictionary<string,object> headers, object message)
        {
            var factory = new ConnectionFactory() { HostName = "localhost", Uri = new Uri("amqp://guest:guest@localhost:5672") };
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare("webappExchange", ExchangeType.Headers, true);

                    var json = JsonConvert.SerializeObject(message);
                    var bytes = System.Text.Encoding.UTF8.GetBytes(json);
                    var props = channel.CreateBasicProperties();
                    props.Headers=headers;

                    channel.BasicPublish("webappExchange", "", props, bytes);
                    channel.Close();
                    connection.Close();

                }
            }
        }
        #endregion

        [HttpPost]
        [Route("Book")]
        public IActionResult Book()
        {
            var tourname = Request.Form["tourname"];
            var name = Request.Form["name"];
            var email= Request.Form["email"];
            var needsTransport = Request.Form["transport"] == "on";

            // Send messages here...

            //sendMessage("tour.booked", new
            //{
            //    tourname = tourname.ToString(),
            //    name = name.ToString(),
            //    email = email.ToString()
            //});

            sendMessageWithHeaderExxhange(new Dictionary<string, object>()
            {
                { "subject","tour"},
                { "action","booked"}
            }, new
            {
                tourname = tourname.ToString(),
                name = name.ToString(),
                email = email.ToString()
            });

            return Redirect($"/BookingConfirmed?tourname={tourname}&name={name}&email={email}");
        }

        [HttpPost]
        [Route("Cancel")]
        public IActionResult Cancel()
        {
            var tourname = Request.Form["tourname"];
            var name = Request.Form["name"];
            var email = Request.Form["email"];
            var cancelReason = Request.Form["reason"];

            //sendMessage("tour.canceled", new
            //{
            //    tourname = tourname.ToString(),
            //    name = name.ToString(),
            //    email = email.ToString(),
            //    cancelReason = cancelReason.ToString()
            //});
            sendMessageWithHeaderExxhange(new Dictionary<string, object>()
            {
                { "subject","tour"},
                { "action","canceled"}
            },new 
            {
                tourname = tourname.ToString(),
                name = name.ToString(),
                email = email.ToString(),
                cancelReason = cancelReason.ToString()
            });

            // Send cancel message here

            return Redirect($"/BookingCanceled?tourname={tourname}&name={name}");
        }
    }
}