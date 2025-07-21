namespace DomainLayer.Model
{
    public class MealImageList
    {

        public string MealCode { get; set; }
        public string MealImage { get; set; }

        public static List<MealImageList> GetAllmeal()
        {
            List<MealImageList> mealdatalist = new List<MealImageList>();

            var mealDataCollection = new[]
            {
            new { MealCode = "NCBB", MealImage = "Awadhi Chicken Biryani with Mirch Salan" },
            new { MealCode = "NCBF", MealImage = "Awadhi Chicken Biryani with Mirch Salan" },
            new { MealCode = "VBKB", MealImage = "Bajra Khichdi with Matar Bhaji (Diabetic-friendly)" },
            new { MealCode = "VBKF", MealImage = "Bajra Khichdi with Matar Bhaji (Diabetic-friendly)" },
            new { MealCode = "NCJB", MealImage = "Chicken Junglee Sandwich" },
            new { MealCode = "NCJF", MealImage = "Chicken Junglee Sandwich(Free meal code).png" },
            new { MealCode = "NCNB", MealImage = "Chicken Nuggets with Fried Potatoes" },
            new { MealCode = "NCNF", MealImage = "Chicken Nuggets with Fried Potatoes" },
            new { MealCode = "NFFB", MealImage = "Herb Grilled Fish Fillet" },
            new { MealCode = "NFFF", MealImage = "Herb Grilled Fish Fillet" },
            new { MealCode = "VHRB", MealImage = "Herb Roast Vegetable Roll" },
            new { MealCode = "VHRF", MealImage = "Herb Roast Vegetable Roll" },
            new { MealCode = "NMBB", MealImage = "Hyderabadi Mutton Biryani with Mirch Salan" },
            new { MealCode = "NMBF", MealImage = "Hyderabadi Mutton Biryani with Mirch Salan " },
            new { MealCode = "NOSB", MealImage = "Masala-Omelette-with-Chicken-Sausages-_-Hash-Brown" },
            new { MealCode = "NOSF", MealImage = "Masala-Omelette-with-Chicken-Sausages-_-Hash-Brown" },
            new { MealCode = "VPBB", MealImage = "Matar-Paneer-Bhurji-with-Aloo-Paratha" },
            new { MealCode = "VPBF", MealImage = "Matar-Paneer-Bhurji-with-Aloo-Paratha" },
            new { MealCode = "VIVB", MealImage = "Mini Idlis, Medu Vada and Upma" },
            new { MealCode = "VIVF", MealImage = "Mini Idlis, Medu Vada and Upma" },
            new { MealCode = "NMTB", MealImage = "Murg Tikka Masala with Lachha Paratha" },
            new { MealCode = "NMTF", MealImage = "Murg Tikka Masala with Lachha Paratha" },
            new { MealCode = "VPMB", MealImage = "Paneer Makhani with Jeera Aloo _ Vegetable Pulao" },
            new { MealCode = "VPMF", MealImage = "Paneer Makhani with Jeera Aloo _ Vegetable Pulao " },
            new { MealCode = "VFPB", MealImage = "Seasonal Fresh Fruits Platter" },
            new { MealCode = "VFPF", MealImage = "Seasonal Fresh Fruits Platter" },
            new { MealCode = "VSDB", MealImage = "Shôndesh Tiramisù" },
            new { MealCode = "VSDF", MealImage = "Shôndesh Tiramisù" },
            new { MealCode = "VMCB", MealImage = "Vegan Moilee Curry with Coconut Rice." },
            new { MealCode = "VMCF", MealImage = "Vegan Moilee Curry with Coconut Rice (1)" },
            new { MealCode = "VMFB", MealImage = "Vegetable Manchurian with Fried Rice " },
            new { MealCode = "VMFF", MealImage = "Vegetable Manchurian with Fried Rice " },
            new { MealCode = "VMGB", MealImage = "Mushroom Ghee Roast with Sriracha Fried Rice" },
            new { MealCode = "VKDB", MealImage = "Kaju Katli white chocolate Mousse" },
            new { MealCode = "VMGF", MealImage = "Mushroom Ghee Roast with Sriracha Fried Rice" },
            new { MealCode = "UPMA", MealImage = "Rava Upma Combo" },
            new { MealCode = "WHSK", MealImage = "Whiskey" },

            new { MealCode = "MMFD", MealImage = "MOJO BAR ORANGE DARK CHOCOLATE + VITAMIN C" },
            new { MealCode = "LSCB", MealImage = "Lemon Samiya ( Vakulaa)" },
            new { MealCode = "CXCB", MealImage = "TOMATO CUCUMBER CHEESE SANDWICH" },
            new { MealCode = "CPMD", MealImage = "CHOWPATTY PHUDINA BHEL" },
            new { MealCode = "CCCB", MealImage = "Chicken Curry Rice" },
            new { MealCode = "LTML", MealImage = "Late Meal ₹0" },
            new { MealCode = "VRPB", MealImage = "Veg Red Sause Pasta" },
            new { MealCode = "VRPF", MealImage = "Veg Red Sause Pasta" },

            new { MealCode = "CAKE", MealImage = "Cake" },
            new { MealCode = "VFSB", MealImage = "Feta Vegetable Salad" },
            new { MealCode = "VFSF", MealImage = "Feta Vegetable Salad" },
            new { MealCode = "TMAI", MealImage = "Masala Chai" },
            new { MealCode = "TMAF", MealImage = "Masala Chai" },
            new { MealCode = "NKPB", MealImage = "Non-Veg Kabab" },
            new { MealCode = "NKPF", MealImage = "Non-Veg Kabab" },
            new { MealCode = "NCOB", MealImage = "Cheddar & Chives Omelette" },
            new { MealCode = "NCOF", MealImage = "Cheddar & Chives Omelette" },
            new { MealCode = "NCCB", MealImage = "Chicken Ghee Roast with Siracha Fried Rice" },
            new { MealCode = "NCCF", MealImage = "Chicken Ghee Roast with Siracha Fried Rice" },
            new { MealCode = "CFMR", MealImage = "BLACK COFFEE" },
            new { MealCode = "CFMF", MealImage = "BLACK COFFEE" },
            
            //Spicejet Meal
            new { MealCode = "VIGB", MealImage = "Dry Fruits Gujiya" },
            new { MealCode = "VGSW", MealImage = "Cucumber, tomato cheese in multigrain bread" },
            new { MealCode = "VGML", MealImage = "Masala Dosa with Tomato onion uttapam and kanjivaram mini idli in sambar along with Coconut Chutney" },
            new { MealCode = "VCC6", MealImage = "Vegetable Daliya" },
            new { MealCode = "VCC5", MealImage = "Vegetable Pasta in Neapolitan sauce" },
            new { MealCode = "VCC2", MealImage = "Vegetable in Red Thai Curry with Steamed Rice" },
            new { MealCode = "NVSW", MealImage = "Non Veg Sandwich" },
            new { MealCode = "NVML", MealImage = "Murg lababdar on bed of palak pulao Dal panchrattni" },
            new { MealCode = "NCC6", MealImage = "Chicken schezwan on bed of fried rice" },
            new { MealCode = "NCC5", MealImage = "Tawa Fish masala on bed of Steamed rice with tadka masoor dal" },
            new { MealCode = "NCC4", MealImage = "Tandoori Chicken tangri with chicken haryali tikka & vegetable shami kebab" },
            new { MealCode = "NCC1", MealImage = "Grilled Chicken Breast with Mushroom Sauce, Yellow Rice, Sautéed Carrots Beans Baton" },
            new { MealCode = "LCVS", MealImage = "Low cal salad Vegetarian" },
            new { MealCode = "LCNS", MealImage = "Low cal salad Non - Vegetarian" },
            new { MealCode = "JNSW", MealImage = "Jain Cold Sandwich (current Cucumber and Tomato sandwich)" },
            //new { MealCode = "JNML", MealImage = "Jain Hot Meal" },
            new { MealCode = "GFVG", MealImage = "Vegetarian Gluten-free Hot Meal" },
            //new { MealCode = "GFNV", MealImage = "Non - Vegetarian Gluten-free Hot Meal" },
            new { MealCode = "GFCM", MealImage = "Vegetarian Gluten-free Cold Meal (Dhokla)" },
           // new { MealCode = "FPML", MealImage = "Fruit Platter"},
            new { MealCode = "DNVL", MealImage = "Non - Vegetarian Diabetic Hot Meal"},
           // new { MealCode = "DBML", MealImage = "Vegetarian Diabetic Hot Meal"},
            new { MealCode = "NCC2", MealImage = "Chicken in Red Thai Curry with Steamed Rice"},
            new { MealCode = "CHML", MealImage = "Kid's meal"},

            //Indigo Meal..
            new { MealCode = "VLML", MealImage = "Veg Lactose Meal / Paneer Bhatti" },
            new { MealCode = "VGAN", MealImage = "Vegan Meal / 2 Dips with Baked Pita" },
            new { MealCode = "TCSW", MealImage = "Tomato Cucumber Cheese Lettuce Sandwich" },
            new { MealCode = "LCVG", MealImage = "Low Calorie Veg / Paneer Bhatti" },
            new { MealCode = "JNML", MealImage = "Jain Meal / Tomato Cucumber" },
            new { MealCode = "GFNV", MealImage = "Gluten Free / Chicken Supreme Salad" },
            new { MealCode = "DBVG", MealImage = "Diabetic Veg Meal / Paneer Bhatti" },
            new { MealCode = "DBNV", MealImage = "Diabetic Non-Veg / Chicken Supreme Salad" },
            new { MealCode = "CNWT", MealImage = "Cashew (Salted)" },
            new { MealCode = "CJSW", MealImage = "Chicken Junglee Sandwich" },
            new { MealCode = "CHVM", MealImage = "Kiddie Delight - Veg" },
            new { MealCode = "CHNM", MealImage = "Kiddie Delight - Non-Veg" },
            new { MealCode = "CHBR", MealImage = "Chicken Biryani" },

            //GDS Meal..
            new { MealCode = "AVML",MealImage = "Vegetarian Indian Meal"},
            new { MealCode = "BBML",MealImage = "Baby Meal"},
            new { MealCode = "BLML",MealImage = "Bland Meal"},
           // new { MealCode = "CHML",MealImage = "Child Meal"},
            new { MealCode = "DBML",MealImage = "Diabetic Meal"},
            new { MealCode = "FPML",MealImage = "Fruit Platter Meal"},
            new { MealCode = "GFML",MealImage = "Gluten Intolerant Meal"},
            new { MealCode = "HNML",MealImage = "Hindu Non-Vegetarian Meal"},
            new { MealCode = "KSML",MealImage = "Kosher Meal"},
            new { MealCode = "LCML",MealImage = "Low Calorie Meal"},
            new { MealCode = "LFML",MealImage = "Low Fat Meal"},
            new { MealCode = "LSML",MealImage = "Low Salt Meal"},
            new { MealCode = "MOML",MealImage = "Moslem Meal"},
            new { MealCode = "NBML",MealImage = "No Beef Meal"},
            new { MealCode = "NLML",MealImage = "Low Lactose Meal"},
            new { MealCode = "NOML",MealImage = "No Meal"},
            new { MealCode = "RVML",MealImage = "Vegetarian Raw Meal"},
            new { MealCode = "SFML",MealImage = "Sea Food Meal"},
            new { MealCode = "SPML",MealImage = "Special Meal"},
            //new { MealCode = "VGML",MealImage = "Vegetarian Vegan Meal"},
            new { MealCode = "VJML",MealImage = "Vegetarian Jain Meal"},
            //new { MealCode = "VLML",MealImage = "Vegetarian Lacto-ovo Meal"},
            new { MealCode = "VOML",MealImage = "Vegetarian Oriental Meal"},
            

            //Akasa Meal
           
            new { MealCode = "PVTT", MealImage = "Triple Treat Nutella Sandwich" },
            new { MealCode = "PVPP", MealImage = "Peppy Paneer Sandwich" },
            new { MealCode = "PVHB", MealImage = "Yu Hyderabadi Veg Biryani" },
            new { MealCode = "PVFF", MealImage = "Fruit & Feta Fiesta Salad" },
            new { MealCode = "PVCB", MealImage = "Chaat-buster Box" },
            new { MealCode = "PONM", MealImage = "Navroz Special (Non-veg)" },
            new { MealCode = "PNKW", MealImage = "Kosha Chicken Malabari Wrapper" },
            new { MealCode = "PNKS", MealImage = "WOW! Khow Suey Chicken" },
            new { MealCode = "PNKP", MealImage = "Kari Pan-tastic Chicken Pocket" },
            new { MealCode = "PDCP", MealImage = "Chocolate Pistachio Verrine" },
            new { MealCode = "PBBS", MealImage = "Borecha Basil Shikanji" },

            new { MealCode = "CWAF", MealImage = "Wafers" },
            new { MealCode = "CVTT", MealImage = "Triple Treat Nutella Sandwich" },
            new { MealCode = "CVPP", MealImage = "Peppy Paneer Sandwich" },
            new { MealCode = "CVNO", MealImage = "Veg Cup Noodles" },
            new { MealCode = "CVMC", MealImage = "Mushroom &amp; Brie Croissant" },
            new { MealCode = "CVJN", MealImage = "Three Bean Burrito Wrap (Jain Special)" },
            new { MealCode = "CVHB", MealImage = "Yu Hyderabadi Veg Biryani" },
            new { MealCode = "CVCB", MealImage = "Chaat-Buster Box" },
            new { MealCode = "CVAL", MealImage = "Valentine's Special" },
            new { MealCode = "CVFF", MealImage = "Fruit & Feta Fiesta Salad" },
            new { MealCode = "CRPX", MealImage = "Snack &amp; Beverage (choose on-board)" },
            new { MealCode = "CNKW", MealImage = "Kosha Chicken Malabari Wrapper" },
            new { MealCode = "CNKS", MealImage = "WOW! Khow Suey Chicken" },
            new { MealCode = "CNKP", MealImage = "Kari Pan-tastic Chicken Pocket" },
            new { MealCode = "CDCP", MealImage = "Chocolate Pistachio Verrine" },

            new { MealCode = "CYOG", MealImage = "Yoga Day Special Meal" },
            new { MealCode = "CVJN", MealImage = "Three Bean Burrito Wrap (Jain Special)" },
            new { MealCode = "CSAN", MealImage = "Sankranti Special" },
            new { MealCode = "CRAZ", MealImage = "Easter Special (Non-Veg)" },
            new { MealCode = "CONM", MealImage = "Navroz Special (Non-veg)" },
            new { MealCode = "CMOM", MealImage = "Mother’s Day Special Meal" },
            new { MealCode = "CHOL", MealImage = "Holi Special" },
            new { MealCode = "CGAN", MealImage = "Ganesh Chaturthi Special Meal" },
            new { MealCode = "CDUS", MealImage = "Dussehra Special" },
            new { MealCode = "CDIW", MealImage = "Diwali Special" },
            new { MealCode = "CCHS", MealImage = "Christmas Special" },
            



            //wheelchair
             new { MealCode = "WCHS", MealImage = "Wheelchair Unable to ascend and descend step" },
             new { MealCode = "WCHR", MealImage = "Wheelchair Unable to walk long distance"},
             new { MealCode = "WCHQ", MealImage = "Wheelchair Quadriplegic" },
             new { MealCode = "WCHC", MealImage = "Wheelchair Paraplegic" },
             new { MealCode = "WCHA", MealImage = "Arrival wheelchair request" },
             new { MealCode = "WCAS", MealImage = "Airport Unable to ascend and descend steps" },
             new { MealCode = "WCAR", MealImage = "Airport Unable to walk long distance" },
          //baggage
           new { MealCode = "PVIP", MealImage = "Xpress Ahead- Prebook" },
           new { MealCode = "PBCB", MealImage = "+ 5Kgs Xtra-Carry-On" },
           new { MealCode = "PBCA", MealImage = "+3Kgs Xtra-Carry-On" },


           new { MealCode = "PBA3", MealImage = "+3 kgs Xcess Baggage" },
           new { MealCode = "PBAB", MealImage = "+ 5 kg Xcess Baggage" },
           new { MealCode = "PBAC", MealImage = "+ 10 kg Xcess Baggage" },
           new { MealCode = "PBAD", MealImage = "+ 15 kg Xcess Baggage" },
           new { MealCode = "PBAF", MealImage = "+ 25 Kg Xcess Baggage" },
           
          
          //baggage AKASA

           new { MealCode = "XC30", MealImage = "+30 kgs Xcess Baggage" },
           new { MealCode = "XC25", MealImage = "+25 kg Xcess Baggage" },
           new { MealCode = "XC20", MealImage = "+20 kg Xcess Baggage" },
           new { MealCode = "XC15", MealImage = "+15 kg Xcess Baggage" },
           new { MealCode = "XC10", MealImage = "+10 Kg Xcess Baggage" },
           new { MealCode = "XC05", MealImage = "+5 Kg Xcess Baggage" },
           
           //Indigo Bag Code
           new { MealCode = "XBPD", MealImage = "+30 kgs Xcess Baggage" },
           new { MealCode = "XBPJ", MealImage = "+ 20 kg Xcess Baggage" },
           new { MealCode = "XBPC", MealImage = "+ 15 kg Xcess Baggage" },
           new { MealCode = "XBPB", MealImage = "+ 10 kg Xcess Baggage" },
           new { MealCode = "XBPA", MealImage = "+ 5 kg Xcess Baggage" },
           new { MealCode = "XBPE", MealImage = "+ 3 kg Xcess Baggage" },

           new { MealCode = "EB25", MealImage = "+25 kgs Xcess Baggage" },
           new { MealCode = "EB20", MealImage = "+ 20 kg Xcess Baggage" },
           new { MealCode = "EB15", MealImage = "+ 15 kg Xcess Baggage" },
           new { MealCode = "EB10", MealImage = "+ 10 kg Xcess Baggage" },
           new { MealCode = "EB05", MealImage = "+ 5 kg Xcess Baggage" },
           new { MealCode = "EB03", MealImage = "+ 3 kg Xcess Baggage" },
           new { MealCode = "EB30", MealImage = "+30 kgs Xcess Baggage" },

              
            // Add more data as needed...
        };

            foreach (var data in mealDataCollection)
            {
                MealImageList mealItem = new MealImageList
                {
                    MealCode = data.MealCode,
                    MealImage = data.MealImage
                };

                mealdatalist.Add(mealItem);
            }

            return mealdatalist;
        }

    }
}
