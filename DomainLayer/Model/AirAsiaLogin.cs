using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace DomainLayer.Model
{
    public class airlineLogin
    {
        public _credentials credentials { get; set; }
        public string applicationName { get; set; } = "";
    }
    //public class AakashaLogin
    //{
    //    public _credentialsAkasha credentialsAakasha { get; set; }
        
    //}

    public class _credentials
    {

        public int supplierid { get; set; }
      //  public string organizationId { get; private set; }

        [Key]
        public string?   username  { get; set; }
        public string? alternateIdentifier { get; set; }
        public string? password { get; set; }

        public string? domain { get; set; }

        private string _organizationId;
        public string organizationId
        {
            get => _organizationId;
            set
            {
                _organizationId = value;
                domain = value; // Assigning directly inside setter
            }
        }

        public string? Image { get; set; }

        private string _img_name;
        public string img_name
        {
            get => _img_name;
            set
            {
                _img_name = value;
                Image = value; // Assigning directly inside setter
            }
        }


        public string? location { get; set; }
        public string? channelType { get; set; }
        public string? loginRole { get; set; }
       
        public int? FlightCode => supplierid;

        // New Status Property(1 = Active, 0 = Inactive)
        public int Status { get; set; } = 1;
        public string dealCodeName { get; set; } 

    }
    //public class _credentialsAkasha
    //{
    //    [Key]
    //    public string? username { get; set; }
    //    public string? password { get; set; }
    //    public string? domain { get; set; }

    //}




}
