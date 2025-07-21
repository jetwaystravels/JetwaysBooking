using DomainLayer.Model;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.DbContextLayer;
using ServiceLayer.Service.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServiceLayer.Service.Implementation
{
    public class PrintTicketService : IPrintTicket<Printticket>
    {
        private readonly AppDbContext _dbContext;
        public PrintTicketService(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public async Task<string?> GetBookingJsonByPNRAsync(string pnr)
        {
            return await _dbContext.tb_Booking
            .Where(b => b.BookingDoc.Contains($"\"recordLocator\":\"{pnr}\""))
        .Select(b => b.BookingDoc)
        .FirstOrDefaultAsync();
        }
    }
}
