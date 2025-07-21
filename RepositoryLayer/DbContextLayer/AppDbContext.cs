using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DomainLayer.Model;
using Microsoft.EntityFrameworkCore;

namespace RepositoryLayer.DbContextLayer
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions con) : base(con)
        {

        }
  //      public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
		//{

		//}
		public DbSet<User> TblUsers { get; set; }  
        public DbSet<Employee> TblEmployee { get;set; }
        public DbSet<Login> TblLogin { get; set; }

        public DbSet<City> tblCityMaster { get; set; }

        public DbSet<_credentials> tblflightlogin { get; set; }
        public DbSet<TicketBooking> TicketBooking { get; set; }
        public DbSet<GSTDetails> GSTDetails { get; set; }
        public DbSet<tb_Booking> tb_Booking { get; set; }
        public DbSet<tb_Airlines> tb_Airlines { get; set; }
        public DbSet<tb_AirCraft> tb_AirCraft { get; set; }
        public DbSet<tb_Segments> tb_Segments { get; set; }
        public DbSet<tb_journeys> tb_journeys { get; set; }
        public DbSet<tb_PassengerTotal> tb_PassengerTotal { get; set; }
        public DbSet<tb_PassengerDetails> tb_PassengerDetails { get; set; }

        public DbSet<tb_DailyNumber> tb_DailyNumber { get; set; }
        public DbSet<ContactDetail> ContactDetail { get; set; }

        public DbSet<Admin> tb_admin { get; set; }
        public DbSet<CP_GSTModel> tb_CP_GstDetails { get; set; }
        public DbSet<Trips> tb_Trips { get; set; }

        public DbSet<Booking> GetBookingDetails { get; set; }
        public DbSet<RefundRequest> RefundRequests { get; set; }

        public DbSet<Printticket> ticketprint { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Booking>().HasNoKey().ToView(null);

            //modelBuilder.Entity<GetbookingDB>()
            //.HasMany(b => b.FlightSegments)
            //.WithOne(f => f.Booking)
            //.HasForeignKey(f => f.BookingID);

            //modelBuilder.Entity<GetbookingDB>()
            //    .HasMany(b => b.Passengers)
            //    .WithOne(p => p.Booking)
            //    .HasForeignKey(p => p.BookingID);
        }
    }
}
