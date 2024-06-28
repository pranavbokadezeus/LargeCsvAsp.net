


using System.Text;
using System.Text.Json;
using Csvhandling.Mappers;
using LargeDatasetProject.Backend.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class RabbitMQWorker
{
    private readonly string _hostname;
    private readonly string _queueName;
    private readonly string _batchQueueName;
    private readonly string _connectionString;

    public RabbitMQWorker(string hostname, string queueName, string batchQueueName, string connectionString)
    {
        _hostname = hostname;
        _queueName = queueName;
        _batchQueueName = batchQueueName;
        _connectionString = connectionString;
    }

    public void StartWorking()
    {
        var factory = new ConnectionFactory { HostName = _hostname };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: _queueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($" [x] Received {message}");

            await ProcessFile(message);
        };
        channel.BasicConsume(queue: _queueName,
                             autoAck: true,
                             consumer: consumer);

        Console.WriteLine(" [*] Waiting for messages.");
        Console.ReadLine();
    }

    private async Task ProcessFile(string filePath)
    {
        var models = new List<Employee>();

        using (var stream = new FileStream(filePath, FileMode.Open))
        using (var reader = new StreamReader(stream))
        {
            string line;
            bool isHeader = true;

            while ((line = reader.ReadLine()) != null)
            {
                if (isHeader)
                {
                    isHeader = false;
                    continue;
                }
                try
                {
                    models.Add(line.ToCsvData());
                }
                catch (Exception ex)
                {
                    // Log the error and continue processing other lines
                    Console.WriteLine($"Error parsing line: {line}. Exception: {ex.Message}");
                }
            }
        }

        Console.WriteLine("file parsed successfully");
        var batchSize = 1000; // Define your batch size here
        var totalBatches = (int)Math.Ceiling((double)models.Count / batchSize);

        for (int i = 0; i < totalBatches; i++)
        {
            
                   
            // if(i == totalBatches-1) {
            //     batchSize = models.Count - (i*batchSize);
            // }
            var batch = models.Skip(i * batchSize).Take(batchSize).ToList();    
            await SendBatchToQueue(batch); 
        }


        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }

    private async Task SendBatchToQueue(List<Employee> batch)
    {
        var factory = new ConnectionFactory { HostName = _hostname };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: _batchQueueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var batchData = JsonSerializer.Serialize(batch);
        var message = Encoding.UTF8.GetBytes(batchData);
        channel.BasicPublish(exchange: string.Empty,
                             routingKey: _batchQueueName,
                             basicProperties: null,
                             body: message);
        Console.WriteLine($" [x] Sent batch of size {batch.Count} to {_batchQueueName}");
    }
}
