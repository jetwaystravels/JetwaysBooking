namespace DomainLayer.Model
{
    public class FareList
    {

        public string ProductCode { get; set; }
        public string Faredesc { get; set; }

        public static List<FareList> GetAllfare()
        {
            List<FareList> Faredatalist = new List<FareList>();

            var FareDataCollection = new[]
            {
                //AirAsia
            new { ProductCode = "LT", Faredesc = "Xpress Lite" },
            new { ProductCode = "EC", Faredesc = "Xpress Value" },
            new { ProductCode = "HF", Faredesc = "Vista Flex" },
            new { ProductCode = "EP", Faredesc = "Xpress Sale" },
            new { ProductCode = "BT", Faredesc = "Xpress Promo" },
            new { ProductCode = "FM", Faredesc = "Xpress Family" },
            new { ProductCode = "NT", Faredesc = "Xpress Return" },
            new { ProductCode = "DF", Faredesc = "Defence Fare" },
            new { ProductCode = "OF", Faredesc = "Xpress Flex" },
            new { ProductCode = "XE", Faredesc = "Fixed Fare" },
            new { ProductCode = "HF", Faredesc = "Herb Roast Vegetable Roll" },
            new { ProductCode = "STU", Faredesc = "Student Fare" },
            new { ProductCode = "SCT", Faredesc = "Senior Citizen Fare" },
            new { ProductCode = "W", Faredesc = "Xpress Bizz " },
            new { ProductCode = "SM", Faredesc = "Corporate fare" },
            new { ProductCode = "FS", Faredesc = "Corporate Flex" },
            new { ProductCode = "DNR", Faredesc = "Doc and Nurse Fare" },

            //Indigo
            new { ProductCode = "B", Faredesc = "Lite Fare" },
            new { ProductCode = "J", Faredesc = "Flexi Fare" },
            new { ProductCode = "O", Faredesc = "Super 6E" },
            new { ProductCode = "A", Faredesc = "Family Fare" },
            new { ProductCode = "R", Faredesc = "Retail Fare" },
            new { ProductCode = "S", Faredesc = "Sales Fare" },
            new { ProductCode = "N", Faredesc = "Special Round Trip " },
            new { ProductCode = "T", Faredesc = "Tactical Fare" },
            new { ProductCode = "M", Faredesc = "Corp Connect Fare" },
            new { ProductCode = "C", Faredesc = "Coupon Fare" },
            new { ProductCode = "F", Faredesc = "Corporate Fare" },
            new { ProductCode = "BR", Faredesc = "Stretch" },
            new { ProductCode = "SM", Faredesc = "SME Fare" },
            new { ProductCode = "BC", Faredesc = "StretchPlus " },


            new { ProductCode = "CP", Faredesc = "Corporate Fare " },
            new { ProductCode = "PC", Faredesc = "Corporate Fare" },
            new { ProductCode = "CM", Faredesc = "Corporate Max Fare" },
            new { ProductCode = "MC", Faredesc = "Corporate Max Fare" },

            //Akasha
            new { ProductCode = "EC", Faredesc = "Saver" },
            new { ProductCode = "AV", Faredesc = "Flexi" },

            //Spicejet
            new { ProductCode = "SS", Faredesc = "Saver Fare" },
            new { ProductCode = "RS", Faredesc = "Spice Plus" },
            new { ProductCode = "SC", Faredesc = "Spice Max" },
            new { ProductCode = "SF", Faredesc = "Spice Flex" },
            

              
            // Add more data as needed...
        };

            foreach (var data in FareDataCollection)
            {
                FareList fareItem = new FareList
                {
                    ProductCode = data.ProductCode,
                    Faredesc = data.Faredesc
                };

                Faredatalist.Add(fareItem);
            }

            return Faredatalist;
        }

    }
}
