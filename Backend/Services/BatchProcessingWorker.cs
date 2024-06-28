using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using LargeDatasetProject.Backend.Models;
using MySql.Data.MySqlClient;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class BatchProcessingWorker
{
    private readonly string _hostname;
    private readonly string _batchQueueName;
    private readonly string _connectionString;

    public BatchProcessingWorker(string hostname, string batchQueueName, string connectionString)
    {
        _hostname = hostname;
        _batchQueueName = batchQueueName;
        _connectionString = connectionString;
    }

    public void StartWorking()
    {
        var factory = new ConnectionFactory { HostName = _hostname };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();
        channel.QueueDeclare(queue: _batchQueueName,
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
            var batch = JsonSerializer.Deserialize<List<Employee>>(message);
            Console.WriteLine($" [x] Received batch of size {batch.Count}");

            Stopwatch st = new Stopwatch();
            st.Start();
            await ProcessBatch(batch);
            Console.WriteLine(st.Elapsed);
            st.Stop();
        };
        channel.BasicConsume(queue: _batchQueueName,
                             autoAck: true,
                             consumer: consumer);

        Console.WriteLine(" [*] Waiting for batch messages.");
        Console.ReadLine();
    }

    private async Task ProcessBatch(List<Employee> batch)
    {
        StringBuilder sCommand = new StringBuilder("REPLACE INTO employees (Id,Email,Name,Country,State,City,Telephone,AddressLine1,AddressLine2,DOB,FY2019_20,FY2020_21,FY2021_22,FY2022_23,FY2023_24) VALUES ");
        List<string> Rows = new List<string>();
        for (int i = 0; i < batch.Count; i++)
        {
            Rows.Add(string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}')", 
                    batch[i].ID,
                    MySqlHelper.EscapeString(batch[i].Email), 
                    MySqlHelper.EscapeString(batch[i].Name), 
                    MySqlHelper.EscapeString(batch[i].Country), 
                    MySqlHelper.EscapeString(batch[i].State), 
                    MySqlHelper.EscapeString(batch[i].City), 
                    MySqlHelper.EscapeString(batch[i].Telephone), 
                    MySqlHelper.EscapeString(batch[i].AddressLine1), 
                    MySqlHelper.EscapeString(batch[i].AddressLine2), 
                    batch[i].DOB.ToString("yyyy-MM-dd"),
                    batch[i].FY2019_20,
                    batch[i].FY2020_21,
                    batch[i].FY2021_22,
                    batch[i].FY2022_23,
                    batch[i].FY2023_24
                ));
        }
        sCommand.Append(string.Join(",", Rows));
        sCommand.Append(";");

        using (MySqlConnection mConnection = new MySqlConnection(_connectionString))
        {
            await mConnection.OpenAsync();
            using var transactions = await mConnection.BeginTransactionAsync();
            using (MySqlCommand myCmd = new MySqlCommand(sCommand.ToString(), mConnection))
            {
                myCmd.Transaction = transactions;
                myCmd.CommandType = CommandType.Text;
                try {
                    await myCmd.ExecuteNonQueryAsync();
                }
                catch(Exception e) {
                    Console.WriteLine(e);
                    await transactions.RollbackAsync();
                }
                await transactions.CommitAsync();
            }
        }
        Console.WriteLine("Batch processed successfully");
    }
}
