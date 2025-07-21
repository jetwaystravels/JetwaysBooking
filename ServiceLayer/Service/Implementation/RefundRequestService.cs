using DomainLayer.Model;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.DbContextLayer;
using ServiceLayer.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Service.Implementation
{
    public class RefundRequestService : IRefundRequest<RefundRequest>
    {
        private readonly AppDbContext _dbContext;

        public RefundRequestService(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }
        public async Task<IEnumerable<RefundRequest>> GetByRefundAsync(string bookingId, string recordLocator)
        {
            var bookingIdParam = new SqlParameter("@BookingID", bookingId ?? (object)DBNull.Value);
            var recordLocatorParam = new SqlParameter("@RecordLocator", recordLocator ?? (object)DBNull.Value);

            return await _dbContext.RefundRequests
                .FromSqlRaw("EXEC sp_GetRefundRequestsByBooking @BookingID, @RecordLocator",
                            bookingIdParam, recordLocatorParam)
                .ToListAsync();
        }
    }
}
