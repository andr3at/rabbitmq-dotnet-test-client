using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RabbitMQ.TestClient
{
    class Program
    {
        static string exchangeName = "my-new-exchanage";
        static string routingKey = "my-new-queue";
        static string queueName = "my-new-queue";
        static bool publish = true;

        static void Main(string[] args)
        {
            IConnection conn = null;

            if (args.Length != 1)
            {
                Console.WriteLine("Usage:");
                Console.WriteLine("RabbitMQ.TestClient.exe publish");
                Console.WriteLine("RabbitMQ.TestClient.exe consume");
                return;
            }

            if (args[0] == "consume")
                publish = false;

            try
            {
                // To connect to a RabbitMQ, it is necessary to instantiate a ConnectionFactory and configure it
                ConnectionFactory factory = new ConnectionFactory();
                factory.UserName = "testmq";
                // "gue
                factory.Password = "testmq";
                factory.VirtualHost = "/";
                factory.HostName = "192.168.1.28";
                //port: 5672 for regular connections, 5671 for connections that use TLS
                factory.Port = 5672;
                //factory.Uri = "amqp://user:pass@hostName:port/vhost";
                //factory.Uri = "amqp://testmq:testmq@192.168.1.28:5672/";

                conn = factory.CreateConnection();

                if (publish)
                    Publish(factory, conn);
                else
                    Consume(factory, conn);

                Console.WriteLine("Done.");
            }
            finally
            {
                conn.Close();
            }
        }

        private static void Consume(ConnectionFactory factory, IConnection conn)
        {
            //The IConnection interface can then be used to open a channel:
            IModel channel = conn.CreateModel();

            // To retrieve individual messages, use IModel.BasicGet. The returned value 
            // is an instance of BasicGetResult, from which the header information (properties) and 
            // message body can be extracted:
            bool noAck = false;
            BasicGetResult result = channel.BasicGet(queueName, noAck);
            if (result == null)
            {
                // No message available at this time.
                Console.WriteLine("No message available at this time.");
            }
            else
            {
                IBasicProperties props = result.BasicProperties;
                byte[] body = result.Body;
                string msg = System.Text.Encoding.UTF8.GetString(body);

                Console.WriteLine("Message.Length:" + body.Length);
                Console.WriteLine("Message:" + msg);

                //Since noAck = false above, you must also call IModel.BasicAck to 
                //acknowledge that you have successfully received and processed the message:
                channel.BasicAck(result.DeliveryTag, false);
            }
        }

        private static void Publish(ConnectionFactory factory, IConnection conn)
        {
            //The IConnection interface can then be used to open a channel:
            IModel channel = conn.CreateModel();

            byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes("Hello, world from .NET client...");

            channel.BasicPublish(exchangeName, routingKey, null, messageBodyBytes);
        }
    }
}
