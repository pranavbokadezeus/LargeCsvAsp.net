using CsvHelper.Configuration;
using LargeDatasetProject.Backend.Models;

namespace LargeDatasetProject.Backend.Mappers
{
    public sealed class EmployeeCsvMap : ClassMap<Employee>
    {
        public EmployeeCsvMap()
        {
            Map(m => m.ID).Index(0).Name("ID");
            Map(m => m.Email).Index(1).Name("Email");
            Map(m => m.Name).Index(2).Name("Name");
            Map(m => m.Country).Index(3).Name("Country");
            Map(m => m.State).Index(4).Name("State");
            Map(m => m.City).Index(5).Name("City");
            Map(m => m.Telephone).Index(6).Name("Telephone");
            Map(m => m.AddressLine1).Index(7).Name("AddressLine1");
            Map(m => m.AddressLine2).Index(8).Name("AddressLine2");
            Map(m => m.DOB).Index(9).Name("DOB").TypeConverterOption.Format("yyyy-MM-dd");
            Map(m => m.FY2019_20).Index(10).Name("FY2019_20");
            Map(m => m.FY2020_21).Index(11).Name("FY2020_21");
            Map(m => m.FY2021_22).Index(12).Name("FY2021_22");
            Map(m => m.FY2022_23).Index(13).Name("FY2022_23");
            Map(m => m.FY2023_24).Index(14).Name("FY2023_24");
        }
    }
}
