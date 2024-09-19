using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

// Setting up the Connection Factory - Think SSL Connection Factory
var factory = new ConnectionFactory { HostName = "localhost" };
// Actually set up that connection - think port is open on 5671
using var connection = factory.CreateConnection();
// Create the channel to handle input recieved on the port that we just opened
using var channel = connection.CreateModel();

// Declare the queue - whole reason why I am learning RabbitMQ ;)
channel.QueueDeclare(queue: "task_queue",
                     durable: true,
                     exclusive: false,
                     autoDelete: false,
                     arguments: null);
// Tell RabbitMQ not to give more than 1 message (preFetchCount < 1) at a time
// Once the message has been consumed, prefetchCount is set back to 0 and the worker is available again
channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

// Inform the operator that no consumers have sent messages yet
Console.WriteLine(" [*] Waiting for messages.");


// Set up consumer object
var consumer = new EventingBasicConsumer(channel);

// Lambda function to iterate through each time the 'Received' event occurs for the consumer object
// 
consumer.Received += (model, ea) =>
{
  var body = ea.Body.ToArray();
  var message = Encoding.UTF8.GetString(body);
  Console.WriteLine($" [X] Received {message}");

  int dots = message.Split('.').Length - 1;
  Thread.Sleep(dots * 1000);

  Console.WriteLine(" [X] Done");

  // here channel could also be accessed as ((EventingBasicConsumer)sender).Model
  channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
};
channel.BasicConsume(queue: "task_queue",
                     autoAck: false,
                     consumer: consumer);

Console.WriteLine(" Press [enter] to exit.");



Console.ReadLine();
