using System;

namespace LargeDatasetProject.Backend.Models
{
    public class Employee
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public string State { get; set; }
        public string City { get; set; }
        public string Telephone { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public DateTime DOB { get; set; }
        public long FY2019_20 { get; set; }
        public long FY2020_21 { get; set; }
        public long FY2021_22 { get; set; }
        public long FY2022_23 { get; set; }
        public long FY2023_24 { get; set; }
    }
}
