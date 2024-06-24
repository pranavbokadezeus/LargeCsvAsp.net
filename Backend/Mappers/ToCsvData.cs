using LargeDatasetProject.Backend.Models;
using System;

namespace Csvhandling.Mappers
{
    public static class CsvMapper
    {
        public static Employee ToCsvData(this string data)
        {
            var values = data.Split(',');
            
            if (values.Length < 15)
            {
                throw new ArgumentException("Invalid CSV format: Expected at least 15 columns.");
            }

            return new Employee
            {
                ID = int.Parse(values[0]),
                Email = values[1],
                Name = values[2],
                Country = values[3],
                State = values[4],
                City = values[5],
                Telephone = values[6],
                AddressLine1 = values[7],
                AddressLine2 = values[8],
                DOB = DateTime.Parse(values[9]),
                FY2019_20 = long.Parse(values[10]),
                FY2020_21 = long.Parse(values[11]),
                FY2021_22 = long.Parse(values[12]),
                FY2022_23 = long.Parse(values[13]),
                FY2023_24 = long.Parse(values[14])
            };
        }
    }
}
