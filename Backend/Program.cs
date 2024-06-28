

using Microsoft.EntityFrameworkCore;
using LargeDatasetProject.Backend.Data;
using MySqlConnector;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;
using LargeDatasetProject.Models;

var builder = WebApplication.CreateBuilder(args);

// adding mongodb services
builder.Services.Configure<StatusStoreDatabaseSettings>(
    builder.Configuration.GetSection("StatusStoreDatabase"));



builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 37)))
);

var hostname = "localhost";
var queueName = "file_uploads";
var batchQueueName = "batch_uploads";
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddSingleton(new RabbitMQWorker(hostname, queueName, batchQueueName, connectionString));
builder.Services.AddSingleton(new BatchProcessingWorker(hostname, batchQueueName, connectionString));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (args.Length > 0)
{
    if (args[0].ToLower() == "rabbitworker")
    {
        StartRabbitWorker(connectionString);
    }
    else if (args[0].ToLower() == "batchworker")
    {
        StartBatchWorker(connectionString);
    }
    else
    {
        StartServer();
    }
}
else
{
    StartServer();
}

void StartServer()
{
    app.UseCors(builder => builder
        .AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader());
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
}

void StartRabbitWorker(string connectionString)
{
    var worker = new RabbitMQWorker(hostname, queueName, batchQueueName, connectionString);
    worker.StartWorking();
}

void StartBatchWorker(string connectionString)
{
    var worker = new BatchProcessingWorker(hostname, batchQueueName, connectionString);
    worker.StartWorking();
}

