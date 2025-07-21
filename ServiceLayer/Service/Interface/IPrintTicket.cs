using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Service.Interface
{
    public interface IPrintTicket<T> where T : class
    {
        Task<string?> GetBookingJsonByPNRAsync(string pnr);
    }
}
