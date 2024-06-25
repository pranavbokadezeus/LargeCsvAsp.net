import csv
import random
import datetime
 
class CsvModel:
    def __init__(self):
        self.Id = 0
        self.EmailId = ""
        self.Name = ""
        self.Country = ""
        self.State = ""
        self.City = ""
        self.TelephoneNumber = ""
        self.AddressLine1 = ""
        self.AddressLine2 = ""
        self.DateOfBirth = datetime.datetime.now()
        self.FY2019_20 = 0
        self.FY2020_21 = 0
        self.FY2021_22 = 0
        self.FY2022_23 = 0
        self.FY2023_24 = 0
 
def generate_data(num_records):
    data = []
    for i in range(num_records):
        csv_model = CsvModel()
        csv_model.Id = i + 1
        csv_model.EmailId = f"user{i}@example.com"
        csv_model.Name = f"User {i}"
        csv_model.Country = random.choice(["USA", "Canada", "Mexico", "UK", "Australia"])
        csv_model.State = random.choice(["California", "New York", "Texas", "Florida", "Illinois"])
        csv_model.City = random.choice(["Los Angeles", "New York City", "Houston", "Miami", "Chicago"])
        csv_model.TelephoneNumber = f"({i}23) 456-0{i}01"
        csv_model.AddressLine1 = random.choice(["Main", "Elm", "Oak", "Maple", "Pine"])
        csv_model.AddressLine2 = ""
        csv_model.DateOfBirth = datetime.datetime.now() - datetime.timedelta(days=random.randint(0, 365*70))
        csv_model.FY2019_20 = random.randint(0, 100000)
        csv_model.FY2020_21 = random.randint(0, 100000)
        csv_model.FY2021_22 = random.randint(0, 100000)
        csv_model.FY2022_23 = random.randint(0, 100000)
        csv_model.FY2023_24 = random.randint(0, 100000)
        data.append(csv_model)
    return data
 
def write_csv_file(data, file_path):
    with open(file_path, "w", newline="", encoding="utf-8") as csvfile:
        fieldnames = ["Id", "EmailId", "Name", "Country", "State", "City", "TelephoneNumber", "AddressLine1", "AddressLine2", "DateOfBirth", "FY2019_20", "FY2020_21", "FY2021_22", "FY2022_23", "FY2023_24"]
        writer = csv.DictWriter(csvfile, fieldnames=fieldnames)
        writer.writeheader()
        for csv_model in data:
            writer.writerow(csv_model.__dict__)
 
if __name__ == "__main__":
    data = generate_data(10000)
    write_csv_file(data, "output1.csv")