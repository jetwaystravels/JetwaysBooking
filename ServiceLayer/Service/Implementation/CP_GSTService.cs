using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Model;
using Microsoft.EntityFrameworkCore;
using RepositoryLayer.DbContextLayer;
using ServiceLayer.Service.Interface;

namespace ServiceLayer.Service.Implementation
{
    public class CP_GSTService : ICP_GstDetail<CP_GSTModel>
    {
        private readonly AppDbContext _context;
        public CP_GSTService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<IEnumerable<CP_GSTModel>> GetAllAsync()
        {

            return await _context.tb_CP_GstDetails.ToListAsync();
        }
    }
}
