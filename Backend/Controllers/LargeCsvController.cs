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

    

        [HttpPost]
        [Route("upload-csv/")]
        public async Task<ActionResult> UploadCsvAsync(IFormFile file)
        {
            Console.WriteLine("I'm post file");
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }
 
            var filePath = Path.GetTempFileName();
 
            try
            {
                var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                stream.Position = 0; // Reset the stream position to the beginning
 
 
                var models = new List<Employee>();
 
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
                // await _applicationDbContext.BulkInsertAsync(models);
                // await _applicationDbContext.SaveChangesAsync();
                Stopwatch st = new Stopwatch();
                st.Start();

                string ConnectionString = "Server=localhost;Database=largedatasetdb;User=root;Password=root;";
                StringBuilder sCommand = new StringBuilder("REPLACE INTO employees (Id,Email,Name,Country,State,City,Telephone,AddressLine1,AddressLine2,DOB,FY2019_20,FY2020_21,FY2021_22,FY2022_23,FY2023_24) VALUES ");           
                using (MySqlConnection mConnection = new MySqlConnection(ConnectionString))
                {
                    Console.WriteLine("connection made successfully");
                    List<string> Rows = new List<string>();
                    for (int i = 0; i < 100000; i++)
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
                    using (MySqlCommand myCmd = new MySqlCommand(sCommand.ToString(), mConnection))
                    {
                        myCmd.CommandType = CommandType.Text;
                        try {
                            await myCmd.ExecuteNonQueryAsync();
                        }
                        catch(Exception e) {
                            Console.WriteLine(e);
                        }
                        // myCmd.ExecuteNonQuery();
                    }
                }
                Console.WriteLine("data uploaded successfully");
                Console.WriteLine(st.Elapsed);
 
                return Ok(new { file.ContentType, file.Length, file.FileName });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.StackTrace);
                return StatusCode(StatusCodes.Status500InternalServerError, "Internal server error");
            }
            finally
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
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