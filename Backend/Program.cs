using Microsoft.EntityFrameworkCore;      
using LargeDatasetProject.Backend.Data;
using MySqlConnector;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors();

builder.Services.AddMySqlDataSource(builder.Configuration.GetConnectionString("DefaultConnection")!);


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 37)))
);




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
// ----------------------------------------------

if (args.Length > 0 && args[0].ToLower() == "worker")
        {
            StartWorker();
        }
        else
        {
            CreateHostBuilder(args).Build().Run();
        }


static void StartWorker()
    {
        var worker = new RabbitMQWorker("localhost", "file_uploads", "Server=localhost;Database=largedatasetdb;User=root;Password=root;");
        worker.StartWorking();
    }

static IHostBuilder CreateHostBuilder(string[] args)
{
    return Host.CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
}


app.UseCors(builder => builder
.AllowAnyOrigin()
.AllowAnyMethod()
.AllowAnyHeader());
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
app.Run();
