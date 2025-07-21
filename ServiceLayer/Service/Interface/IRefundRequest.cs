using DomainLayer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Service.Interface
{
    public interface IRefundRequest<T> where T : class
    {
        Task<IEnumerable<RefundRequest>> GetByRefundAsync(string bookingId, string recordLocator);
    }
}
