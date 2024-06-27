// using LargeDatasetProject.Backend.Data;
// using LargeDatasetProject.Backend.Models;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;

// using System.Globalization;
// using CsvHelper;

// using CsvHelper.Configuration;




// namespace LargeDatasetProject.Controllers;






// [Route("api/[Controller]")]
// [ApiController]

// public class LargeCsvController : Controller {
//     private readonly ApplicationDbContext _applicationDbContext;
//     public LargeCsvController(ApplicationDbContext applicationDbContext) {
//         _applicationDbContext = applicationDbContext;
//     }

//     [HttpGet]
//     public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
//     {
//         var existUser = await _applicationDbContext.Employees.ToListAsync();
//         if(existUser.Count() == 0) {
//             return NotFound();
//         }
//         return Ok(existUser);
//         // if(_applicationDbContext.Employees.Count() == 0)
//         // {
//         //     return NotFound();
//         // }
//         // return await _applicationDbContext.Employees.ToListAsync();
//     }

//     [HttpGet("{id}")]
//     public async Task<ActionResult<Employee>> GetEmployee(int id)
//     {
//         if(_applicationDbContext.Employees == null)
//         {
//             return NotFound();
//         }
//         var employee = await _applicationDbContext.Employees.FindAsync(id);
//         if(employee == null) 
//         {
//             return NotFound();
//         }
//         return employee;
//     }

//     [HttpPost]
//     public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
//     {
//         _applicationDbContext.Employees.Add(employee);
//         Console.WriteLine("server data is updated");
//         await _applicationDbContext.SaveChangesAsync();
//         Console.WriteLine("db data is updated");

//         return CreatedAtAction(nameof(GetEmployee), new { id = employee.ID}, employee);
//     }

//     [HttpPut("{id}")]
//     public async Task<ActionResult> PutEmployee(int id, Employee employee)
//     {
//         if(id != employee.ID)
//         {
//             return BadRequest();
//         }

//         _applicationDbContext.Entry(employee).State = EntityState.Modified;
//         try{
//             await _applicationDbContext.SaveChangesAsync();
//         }
//         catch(DbUpdateConcurrencyException)
//         {
//             throw;
//         }
//         return Ok();
//     }

//     [HttpDelete("{id}")]
//     public async Task<ActionResult> DeleteEmployee(int id) 
//     {
//         if(_applicationDbContext.Employees == null)
//         {
//             return NotFound();
//         }
//         var employee = await _applicationDbContext.Employees.FindAsync(id);
//         if(employee == null) {
//             return NotFound();
//         }
//         _applicationDbContext.Employees.Remove(employee);
//         await _applicationDbContext.SaveChangesAsync();
//         return Ok();
//     }

// // -----------------------------------------------------------------------------

   
// }


using CsvHelper;

using Microsoft.EntityFrameworkCore;



using LargeDatasetProject.Backend.Data;
using LargeDatasetProject.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using System.Collections.Concurrent;
using Microsoft.AspNetCore.OutputCaching;
using Csvhandling.Mappers;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using System.Text;
using MySqlConnector;
using System.Data;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using RabbitMQ.Client;
namespace LargeDatasetProject.Controllers
{
    [Route("api/LargeCsv")]
    [ApiController]
    public class LargeCsvController : ControllerBase
    {
        private readonly ApplicationDbContext _applicationDbContext;
        private readonly ILogger<LargeCsvController> _logger;

        public LargeCsvController(ApplicationDbContext applicationDbContext, ILogger<LargeCsvController> logger)
        {
            _applicationDbContext = applicationDbContext;
            _logger = logger;
        }

        private void SendMessageToQueue(string filePath)
        {
            var factory = new ConnectionFactory { HostName = _hostname };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            channel.QueueDeclare(queue: _queueName,
                                durable: false,
                                exclusive: false,
                                autoDelete: false,
                                arguments: null);

            var message = Encoding.UTF8.GetBytes(filePath);
            channel.BasicPublish(exchange: string.Empty,
                                routingKey: _queueName,
                                basicProperties: null,
                                body: message);
            Console.WriteLine($" [x] Sent {filePath}");
        }
        
      

        private readonly string _hostname = "localhost";
        private readonly string _queueName = "file_uploads";

        [HttpPost]
        [Route("upload-csv/")]
        public async Task<ActionResult> UploadCsvAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            var filePath = Path.GetTempFileName();
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            SendMessageToQueue(filePath);

            return Ok(new { file.ContentType, file.Length, file.FileName });
        }

        

        // Other controller actions for CRUD operations on employees

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> GetEmployees()
        {
            var existUser = await _applicationDbContext.Employees.Take(15).ToListAsync();

        if(existUser.Count() == 0) {

            return NotFound();

        }

        return Ok(existUser);
            
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Employee>> GetEmployee(int id)
        {
            if(_applicationDbContext.Employees == null)
            {
                return NotFound();
            }
            var employee = await _applicationDbContext.Employees.FindAsync(id);
            if(employee == null) 
            {
                return NotFound();
            }
            return employee;
        }

    [HttpPost]
    public async Task<ActionResult<Employee>> PostEmployee(Employee employee)
    {
        _applicationDbContext.Employees.Add(employee);
        Console.WriteLine("server data is updated");
        await _applicationDbContext.SaveChangesAsync();
        Console.WriteLine("db data is updated");

        return CreatedAtAction(nameof(GetEmployee), new { id = employee.ID}, employee);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> PutEmployee(int id, Employee employee)
    {
        if(id != employee.ID)
        {
            return BadRequest();
        }

        _applicationDbContext.Entry(employee).State = EntityState.Modified;
        try{
            await _applicationDbContext.SaveChangesAsync();
        }
        catch(DbUpdateConcurrencyException)
        {
            throw;
        }
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteEmployee(int id) 
    {
        if(_applicationDbContext.Employees == null)
        {
            return NotFound();
        }
        var employee = await _applicationDbContext.Employees.FindAsync(id);
        if(employee == null) {
            return NotFound();
        }
        _applicationDbContext.Employees.Remove(employee);
        await _applicationDbContext.SaveChangesAsync();
        return Ok();
    }
    }
}