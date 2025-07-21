using DomainLayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Service.Interface
{
    public interface IBooking<T> where T : class
    {
        Task<IEnumerable<Booking>> GetAllAsync(string userEmail);
        Task<string> GetPNRAsync(string guid);
        // Task<bool> UpdateCancelStatusAsync(string recordLocator, int status);
        Task<bool> UpdateCancelStatusAsync( string recordLocator, int status,string userEmail,decimal balanceDue,decimal totalAmount);
        Task<FullBookingDetailsDto> GetBookingDetailsFromSPAsync(string recordLocator);

    }
}
