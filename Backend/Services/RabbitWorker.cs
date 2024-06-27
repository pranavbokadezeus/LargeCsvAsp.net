using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Csvhandling.Mappers;
using LargeDatasetProject.Backend.Models;
using MySql.Data.MySqlClient;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

public class RabbitMQWorker
{
    private readonly string _hostname;
    private readonly string _queueName;
    private readonly string _connectionString;

    public RabbitMQWorker(string hostname, string queueName, string connectionString)
    {
        _hostname = hostname;
        _queueName = queueName;
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
        Stopwatch st = new Stopwatch();
        st.Start();

        StringBuilder sCommand = new StringBuilder("REPLACE INTO employees (Id,Email,Name,Country,State,City,Telephone,AddressLine1,AddressLine2,DOB,FY2019_20,FY2020_21,FY2021_22,FY2022_23,FY2023_24) VALUES ");
        String sCommand2 = sCommand.ToString();

        using (MySqlConnection mConnection = new MySqlConnection(_connectionString))
        {
            Console.WriteLine("connection made successfully");
            List<string> Rows = new List<string>();
            for (int i = 0; i < models.Count; i++)
            {
                Rows.Add(string.Format("('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}')", 
                        models[i].ID,
                        MySqlHelper.EscapeString(models[i].Email), 
                        MySqlHelper.EscapeString(models[i].Name), 
                        MySqlHelper.EscapeString(models[i].Country), 
                        MySqlHelper.EscapeString(models[i].State), 
                        MySqlHelper.EscapeString(models[i].City), 
                        MySqlHelper.EscapeString(models[i].Telephone), 
                        MySqlHelper.EscapeString(models[i].AddressLine1), 
                        MySqlHelper.EscapeString(models[i].AddressLine2), 
                        models[i].DOB.ToString("yyyy-MM-dd"),
                        models[i].FY2019_20,
                        models[i].FY2020_21,
                        models[i].FY2021_22,
                        models[i].FY2022_23,
                        models[i].FY2023_24
                    ));
            }
            sCommand.Append(string.Join(",", Rows));
            sCommand.Append(";");
            mConnection.Open();
            using var transactions = await mConnection.BeginTransactionAsync();
            using (MySqlCommand myCmd = new MySqlCommand(sCommand.ToString(), mConnection))
            {
                myCmd.Transaction = transactions;
                myCmd.CommandType = CommandType.Text;
                try {
                    await myCmd.ExecuteNonQueryAsync();
                    sCommand.Clear();
                    sCommand.Append(sCommand2);
                }
                catch(Exception e) {
                    Console.WriteLine(e);
                    await transactions.RollbackAsync();
                }
            }
            await transactions.CommitAsync();
        }
        Console.WriteLine("data uploaded successfully");
        Console.WriteLine(st.Elapsed);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}
