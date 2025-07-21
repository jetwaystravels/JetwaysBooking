using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepositoryLayer.Migrations
{
    /// <inheritdoc />
    public partial class corporategst : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CreateDate",
                table: "TblEmployee",
                newName: "CreatedDate");

            migrationBuilder.CreateTable(
                name: "ContactDetail",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MobileNumber = table.Column<int>(type: "int", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifyBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContactDetail", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "GSTDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    bookingReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    airLinePNR = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GSTNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GSTName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    GSTEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GSTDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_admin",
                columns: table => new
                {
                    admin_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    admin_name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    admin_email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    admin_password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    admin_image = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_admin", x => x.admin_id);
                });

            migrationBuilder.CreateTable(
                name: "tb_AirCraft",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AirlineID = table.Column<int>(type: "int", nullable: false),
                    AirCraftName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AirCraftDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Createdby = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Modifieddate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifyby = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_AirCraft", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_Airlines",
                columns: table => new
                {
                    AirlineID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Id = table.Column<int>(type: "int", nullable: false),
                    AirlneName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AirlineDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Createdby = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Modifieddate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifyby = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Airlines", x => x.AirlineID);
                });

            migrationBuilder.CreateTable(
                name: "tb_Booking",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FlightID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AirLineID = table.Column<int>(type: "int", nullable: false),
                    RecordLocator = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CurrencyCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Origin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartureDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArrivalDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalAmount = table.Column<double>(type: "float", nullable: false),
                    SpecialServicesTotal = table.Column<double>(type: "float", nullable: false),
                    SpecialServicesTotal_Tax = table.Column<double>(type: "float", nullable: false),
                    SeatTotalAmount = table.Column<double>(type: "float", nullable: false),
                    SeatTotalAmount_Tax = table.Column<double>(type: "float", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Createdby = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifyBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookingDoc = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Booking", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_CP_GstDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GST_Number = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GST_CompanyName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GST_MobileNo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifyBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifyDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_CP_GstDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_DailyNumber",
                columns: table => new
                {
                    Autogenratednumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_DailyNumber", x => x.Autogenratednumber);
                });

            migrationBuilder.CreateTable(
                name: "tb_journeys",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JourneyKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    JourneyKeyCount = table.Column<int>(type: "int", nullable: false),
                    FlightType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Stops = table.Column<int>(type: "int", nullable: false),
                    Origin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartureDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ArrivalDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Createdby = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifyby = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_journeys", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tb_PassengerDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SegmentsKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PassengerKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TypeCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Dob = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Seatnumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Handbages = table.Column<int>(type: "int", nullable: true),
                    Carrybages = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MealsCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contact_Emailid = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contact_Mobileno = table.Column<int>(type: "int", nullable: true),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalAmount_tax = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TotalAmount_Meals = table.Column<double>(type: "float", nullable: false),
                    TotalAmount_Meals_tax = table.Column<double>(type: "float", nullable: true),
                    BaggageTotalAmount = table.Column<double>(type: "float", nullable: false),
                    TotalAmount_Seat = table.Column<double>(type: "float", nullable: false),
                    TotalAmount_Seat_tax = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Inf_Firstname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Inf_Middlename = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Inf_Lastname = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Inf_TypeCode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Inf_Gender = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Inf_Dob = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Createdby = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifyBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_PassengerDetails", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_PassengerTotal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TotalMealsAmount = table.Column<double>(type: "float", nullable: false),
                    TotalMealsAmount_Tax = table.Column<double>(type: "float", nullable: false),
                    TotalSeatAmount = table.Column<double>(type: "float", nullable: false),
                    TotalSeatAmount_Tax = table.Column<double>(type: "float", nullable: false),
                    TotalBookingAmount = table.Column<double>(type: "float", nullable: false),
                    totalBookingAmount_Tax = table.Column<double>(type: "float", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Createdby = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifyby = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_PassengerTotal", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tb_Segments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BookingID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    journeyKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SegmentKey = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SegmentCount = table.Column<int>(type: "int", nullable: false),
                    Origin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Destination = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DepartureDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArrivalDate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Identifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CarrierCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Seatnumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MealCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MealDiscription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ArrivalTerminal = table.Column<int>(type: "int", nullable: false),
                    DepartureTerminal = table.Column<int>(type: "int", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Createdby = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Modifyby = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Segments", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblCityMaster",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CityCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CityName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsCity = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCityMaster", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "tblflightlogin",
                columns: table => new
                {
                    username = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    alternateIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    domain = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    location = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    channelType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    loginRole = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Image = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FlightCode = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblflightlogin", x => x.username);
                });

            migrationBuilder.CreateTable(
                name: "TblLogin",
                columns: table => new
                {
                    username = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    alternateIdentifier = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    domain = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    location = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    channelType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    loginRole = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    applicationName = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TblLogin", x => x.username);
                });

            migrationBuilder.CreateTable(
                name: "TicketBooking",
                columns: table => new
                {
                    tripId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    passengerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    phoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bookingDateTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    emailId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    seatNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    airLines = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Class = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    guestBooking = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bookingStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bookingReferenceNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    airlinePNR = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    identifier = table.Column<int>(type: "int", nullable: false),
                    carrierCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    desination = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    origin = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    arrival = table.Column<DateTime>(type: "datetime2", nullable: false),
                    departure = table.Column<DateTime>(type: "datetime2", nullable: false),
                    desinationTerminal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    sourceTerminal = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    price = table.Column<int>(type: "int", nullable: false),
                    taxex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    response = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketBooking", x => x.tripId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContactDetail");

            migrationBuilder.DropTable(
                name: "GSTDetails");

            migrationBuilder.DropTable(
                name: "tb_admin");

            migrationBuilder.DropTable(
                name: "tb_AirCraft");

            migrationBuilder.DropTable(
                name: "tb_Airlines");

            migrationBuilder.DropTable(
                name: "tb_Booking");

            migrationBuilder.DropTable(
                name: "tb_CP_GstDetails");

            migrationBuilder.DropTable(
                name: "tb_DailyNumber");

            migrationBuilder.DropTable(
                name: "tb_journeys");

            migrationBuilder.DropTable(
                name: "tb_PassengerDetails");

            migrationBuilder.DropTable(
                name: "tb_PassengerTotal");

            migrationBuilder.DropTable(
                name: "tb_Segments");

            migrationBuilder.DropTable(
                name: "tblCityMaster");

            migrationBuilder.DropTable(
                name: "tblflightlogin");

            migrationBuilder.DropTable(
                name: "TblLogin");

            migrationBuilder.DropTable(
                name: "TicketBooking");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "TblEmployee",
                newName: "CreateDate");
        }
    }
}
