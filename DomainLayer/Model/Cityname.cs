using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.IO;


namespace DomainLayer.Model
{
    public class Citynamelist
    {
        private static IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
        public string citycode { get; set; }
        public string cityname { get; set; }
        public string airportname { get; set; } // New property
       

        private static string CacheKey = "CityDataList";
        public static List<Citynamelist> GetCityData()
        {
            List<Citynamelist> cityDataList = new List<Citynamelist>();

            var cityDataCollection = new[]
            {
        new { citycode = "AYJ", cityname = "Ayodhya", airportname = "Maharishi Valmiki International Airport" },
        new { citycode = "DEL", cityname = "New Delhi", airportname = "Indira Gandhi International Airport" },
        new { citycode = "BOM", cityname = "Mumbai", airportname = "Chhatrapati Shivaji Maharaj International Airport" },
        new { citycode = "BLR", cityname = "Bangalore", airportname = "Kempegowda International Airport" },
        new { citycode = "CCU", cityname = "Kolkata", airportname = "Netaji Subhas Chandra Bose International Airport" },
        new { citycode = "HYD", cityname = "Hyderabad", airportname = "Rajiv Gandhi International Airport" },
        new { citycode = "COK", cityname = "Kochi", airportname = "Cochin International Airport Limited" },
        new { citycode = "BDQ", cityname = "Vadodara", airportname = "Vadodara Airport" },
        new { citycode = "NAG", cityname = "Nagpur", airportname = "Dr. Babasaheb Ambedkar International Airport" },
        new { citycode = "IXC", cityname = "Chandigarh", airportname = "Chandigarh Airport" },
        new { citycode = "SXR", cityname = "Srinagar", airportname = "Srinagar International Airport" },
        new { citycode = "DED", cityname = "Dehradun", airportname = "Jolly Grant Airport" },
        new { citycode = "MAA", cityname = "Chennai", airportname = "Chennai International Airport" },
        new { citycode = "PNQ", cityname = "Pune", airportname = "Pune International Airport" },
        new { citycode = "GOI", cityname = "Goa", airportname = "Manohar International Airport" },
        new { citycode = "AMD", cityname = "Ahmedabad", airportname = "Sardar Vallabhbhai Patel International Airport" },
        new { citycode = "LKO", cityname = "Lucknow", airportname = "Chaudhary Charan Singh International Airport" },
        new { citycode = "PAT", cityname = "Patna", airportname = "Jay Prakash Narayan International Airport" },
        new { citycode = "JAI", cityname = "Jaipur", airportname = "Jaipur International Airport" },
        new { citycode = "TRV", cityname = "Thiruvananthapuram", airportname = "Trivandrum International Airport" },
        new { citycode = "BBI", cityname = "Bhubaneswar", airportname = "Biju Patnaik International Airport" },
        new { citycode = "IXB", cityname = "Bagdogra", airportname = "Bagdogra International Airport" },
        new { citycode = "VGA", cityname = "Vijayawada", airportname = "Vijayawada International Airport" },
        new { citycode = "IXZ", cityname = "Port Blair", airportname = "Veer Savarkar International Airport" }
    };

            foreach (var data in cityDataCollection)
            {
                Citynamelist cityItem = new Citynamelist
                {
                    citycode = data.citycode,
                    cityname = data.cityname,
                    airportname = data.airportname
                };

                cityDataList.Add(cityItem);
            }

            return cityDataList;
        }


        public static List<Citynamelist> GetAllCityData()
        {

            if (!_cache.TryGetValue(CacheKey, out List<Citynamelist> cityDataList))
            {
                //string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                //string filePath = Path.Combine(baseDirectory, @"D:\JetwaysProject\DomainLayer\Files\Cityname.txt");
                // filePath = Path.GetFullPath(filePath);
                string rootPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.Parent.FullName;
                string filePath = Path.Combine(rootPath, "DomainLayer", "Files", "Cityname.txt");
                cityDataList = LoadDataFromTextFile(filePath);

				_cache.Set(CacheKey, cityDataList, TimeSpan.FromHours(1));
            }

            return cityDataList;
        }
        public static List<Citynamelist> LoadDataFromTextFile(string filePath)
        {
            List<Citynamelist> cityDataList = new List<Citynamelist>();

            if (File.Exists(filePath))
            {
                string jsonData = File.ReadAllText(filePath);

                cityDataList = JsonSerializer.Deserialize<List<Citynamelist>>(jsonData);
            }

            return cityDataList;
        }
    }
}
