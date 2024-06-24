using LargeDatasetProject.Backend.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace LargeDatasetProject.Backend.Data{

    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options) 
    {
        public DbSet<Employee> Employees { get; set; }

    }

}