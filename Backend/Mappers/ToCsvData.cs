// using LargeDatasetProject.Backend.Models;
// using System;

// namespace Csvhandling.Mappers
// {
//     public static class CsvMapper
//     {
//         public static Employee ToCsvData(this string data)
//         {
//             var values = data.Split(',');
            
//             if (values.Length < 15)
//             {
//                 throw new ArgumentException("Invalid CSV format: Expected at least 15 columns.");
//             }

//             return new Employee
//             {
//                 ID = int.Parse(values[0]),
//                 Email = values[1],
//                 Name = values[2],
//                 Country = values[3],
//                 State = values[4],
//                 City = values[5],
//                 Telephone = values[6],
//                 AddressLine1 = values[7],
//                 AddressLine2 = values[8],
//                 DOB = DateTime.Parse(values[9]),
//                 FY2019_20 = long.Parse(values[10]),
//                 FY2020_21 = long.Parse(values[11]),
//                 FY2021_22 = long.Parse(values[12]),
//                 FY2022_23 = long.Parse(values[13]),
//                 FY2023_24 = long.Parse(values[14])
//             };
//         }
//     }
// }



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

            // Validate individual fields
            int id;
            if (!int.TryParse(values[0], out id))
            {
                throw new ArgumentException($"Invalid value for ID: {values[0]}");
            }

            if (string.IsNullOrEmpty(values[1]))
            {
                throw new ArgumentException("Email cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(values[2]))
            {
                throw new ArgumentException("Name cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(values[3]))
            {
                throw new ArgumentException("Country cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(values[4]))
            {
                throw new ArgumentException("State cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(values[5]))
            {
                throw new ArgumentException("City cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(values[6]))
            {
                throw new ArgumentException("Telephone cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(values[7]))
            {
                throw new ArgumentException("AddressLine1 cannot be null or empty.");
            }

            // if (string.IsNullOrEmpty(values[8]))
            // {
            //     throw new ArgumentException("AddressLine2 cannot be null or empty.");
            // }

            // AddressLine2 can be optional, so no validation for null or empty

            DateTime dob;
            if (!DateTime.TryParse(values[9], out dob))
            {
                throw new ArgumentException($"Invalid value for DOB: {values[9]}");
            }

            long fy2019_20;
            if (!long.TryParse(values[10], out fy2019_20))
            {
                throw new ArgumentException($"Invalid value for FY2019_20: {values[10]}");
            }

            long fy2020_21;
            if (!long.TryParse(values[11], out fy2020_21))
            {
                throw new ArgumentException($"Invalid value for FY2020_21: {values[11]}");
            }

            long fy2021_22;
            if (!long.TryParse(values[12], out fy2021_22))
            {
                throw new ArgumentException($"Invalid value for FY2021_22: {values[12]}");
            }

            long fy2022_23;
            if (!long.TryParse(values[13], out fy2022_23))
            {
                throw new ArgumentException($"Invalid value for FY2022_23: {values[13]}");
            }

            long fy2023_24;
            if (!long.TryParse(values[14], out fy2023_24))
            {
                throw new ArgumentException($"Invalid value for FY2023_24: {values[14]}");
            }

            // Parse values into Employee object
            return new Employee
            {
                ID = id,
                Email = values[1],
                Name = values[2],
                Country = values[3],
                State = values[4],
                City = values[5],
                Telephone = values[6],
                AddressLine1 = values[7],
                AddressLine2 = values[8], // Allow null or empty for AddressLine2
                DOB = dob,
                FY2019_20 = fy2019_20,
                FY2020_21 = fy2020_21,
                FY2021_22 = fy2021_22,
                FY2022_23 = fy2022_23,
                FY2023_24 = fy2023_24
            };
        }
    }
}
