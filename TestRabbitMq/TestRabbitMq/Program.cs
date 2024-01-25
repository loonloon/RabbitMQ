using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace TestRabbitMq
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var rabbitMQClient = new RabbitMQClient();

            for(var i = 0; i < 100000; i++)
            {
                string message = $"Hello World! {i}";
                rabbitMQClient.SendRequest(message);
            }

            Console.WriteLine("Press [enter] to exit.");
            Console.ReadLine();

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }

    public class RabbitMQClient
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string requestQueueName;
        private readonly string replyQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly string correlationId;

        public RabbitMQClient()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            requestQueueName = "request_queue";
            channel.QueueDeclare(queue: requestQueueName, durable: false, exclusive: false, autoDelete: true, arguments: null);

            replyQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);
            correlationId = Guid.NewGuid().ToString();

            consumer.Received += (model, ea) =>
            {
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    var response = Encoding.UTF8.GetString(ea.Body.ToArray());
                    Console.WriteLine("Response: " + response);

                    //channel.Close();
                    //connection.Close();
                }
            };

            channel.BasicConsume(consumer: consumer, queue: replyQueueName, autoAck: true);
        }

        public void SendRequest(string message)
        {
            var props = channel.CreateBasicProperties();
            props.CorrelationId = correlationId;
            props.ReplyTo = replyQueueName;

            var messageBytes = Encoding.UTF8.GetBytes(message);
            channel.BasicPublish(exchange: "", routingKey: requestQueueName, basicProperties: props, body: messageBytes);
            Console.WriteLine("Sent: " + message);
        }
    }
}