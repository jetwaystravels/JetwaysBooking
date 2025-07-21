using DomainLayer.Model;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RepositoryLayer.DbContextLayer;
using ServiceLayer.Service.Interface;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Service.Implementation
{
    public class BookingService : IBooking<Booking>
    {
        private readonly AppDbContext _dbContext;
        public BookingService(AppDbContext dbContext)
        {
            this._dbContext = dbContext;
        }


        public async Task<IEnumerable<Booking>> GetAllAsync(string userEmail)
        {
            if (string.IsNullOrWhiteSpace(userEmail))
                throw new ArgumentException("userEmail is required", nameof(userEmail));

            var userEmailParam = new SqlParameter("@UserEmail", userEmail);

            return await _dbContext.GetBookingDetails
                .FromSqlRaw("EXEC sp_GetFlightBookingDetails @UserEmail", userEmailParam)
                .ToListAsync();
        }

        public async Task<string> GetPNRAsync(string guid)
        {

            string result = "";

            using var conn = _dbContext.Database.GetDbConnection(); // Fixed 'dbContext' to '_dbContext'  
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "sp_GetPNR";
            cmd.CommandType = CommandType.StoredProcedure;

            var param = cmd.CreateParameter();
            param.ParameterName = "@guid";
            param.Value = guid;
            cmd.Parameters.Add(param);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if(result.Length > 0)
                    result += ","; // Add a comma to separate multiple PNRs
                result += reader["RecordLocator"].ToString() + "-" + reader["AirLineID"].ToString() ; // Assuming RecordLocator is the column name in the result set
            }
            return result;
        }



        public async Task<bool> UpdateCancelStatusAsync( string recordLocator, int status,string userEmail,decimal balanceDue, decimal totalAmount)
        {
            var recordLocatorParam = new SqlParameter("@RecordLocator", recordLocator);
            var statusParam = new SqlParameter("@CancelStatus", status);
            var userEmailParam = new SqlParameter("@UserEmail", userEmail);
            var balanceDueParam = new SqlParameter("@BalanceDue", balanceDue);
            var totalAmountParam = new SqlParameter("@TotalAmount", totalAmount);

            int rows = await _dbContext.Database.ExecuteSqlRawAsync(
                "EXEC dbo.sp_UpdateBookingCancelStatus @RecordLocator,@CancelStatus,@UserEmail,@BalanceDue,@TotalAmount",
                recordLocatorParam, statusParam, userEmailParam, balanceDueParam, totalAmountParam);
            return rows > 0;
        }

        public async Task<FullBookingDetailsDto> GetBookingDetailsFromSPAsync(string recordLocator)
        {
            var result = new FullBookingDetailsDto();

            using var conn = _dbContext.Database.GetDbConnection(); // Fixed 'dbContext' to '_dbContext'  
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "sp_GetBookingDetailsByRecordLocator";
            cmd.CommandType = CommandType.StoredProcedure;

            var param = cmd.CreateParameter();
            param.ParameterName = "@RecordLocator";
            param.Value = recordLocator;
            cmd.Parameters.Add(param);

            await conn.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            // First Result: Booking Info  
            if (await reader.ReadAsync())
            {
                result.Booking = new BookingResultDto
                {
                    BookingID = reader["BookingID"].ToString(),
                    RecordLocator = reader["RecordLocator"].ToString(),
                    BookedDate = Convert.ToDateTime(reader["BookedDate"]),
                    Origin = reader["Origin"].ToString(),
                    Destination = reader["Destination"].ToString(),
                    DepartureDate = Convert.ToDateTime(reader["DepartureDate"]),
                    ArrivalDate = Convert.ToDateTime(reader["ArrivalDate"]),
                    TotalAmount = Convert.ToInt32(reader["TotalAmount"]),
                    BookingStatus = Convert.ToInt32(reader["BookingStatus"]),
                    BookingType = reader["BookingType"].ToString(),
                    TripType = reader["TripType"].ToString()
                };
            }

            // Move to next result set (segments)  
            await reader.NextResultAsync();
            result.Segments = new List<SegmentResultDto>();
            while (await reader.ReadAsync())
            {
                result.Segments.Add(new SegmentResultDto
                {
                    BookingID = reader["BookingID"].ToString(),
                    Origin = reader["Origin"].ToString(),
                    Destination = reader["Destination"].ToString(),
                    DepartureDate = Convert.ToDateTime(reader["DepartureDate"]),
                    ArrivalDate = Convert.ToDateTime(reader["ArrivalDate"]),
                    CarrierCode = reader["CarrierCode"].ToString(),
                    Identifier = Convert.ToInt32(reader["Identifier"]),
                    ArrivalTerminal = reader["ArrivalTerminal"].ToString(),
                    DepartureTerminal = reader["DepartureTerminal"].ToString()
                });
            }

            // Move to final result set (passengers)  
            await reader.NextResultAsync();
            result.Passengers = new List<PassengerResultDto>();
            while (await reader.ReadAsync())
            {
                result.Passengers.Add(new PassengerResultDto
                {
                    BookingID = reader["BookingID"].ToString(),
                    Title = reader["Title"].ToString(),
                    FirstName = reader["FirstName"].ToString(),
                    LastName = reader["LastName"].ToString(),
                    MobileNumber = reader["contact_Mobileno"].ToString(),
                    SeatNumber = reader["Seatnumber"].ToString(),
                    CarryBages = reader["Carrybages"].ToString(),
                    TotalAmount = Convert.ToInt32(reader["TotalAmount"]),
                    TotalAmount_Tax = Convert.ToInt32(reader["TotalAmount_tax"])
                });
            }

            return result;
        }



    }
}
