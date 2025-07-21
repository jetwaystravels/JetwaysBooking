using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepositoryLayer.Migrations
{
    /// <inheritdoc />
    public partial class Trip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "SeatAdjustment",
                table: "tb_PassengerTotal",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "BaggageTotalAmountTax",
                table: "tb_PassengerDetails",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "BaggageTotalAmountTax_discount",
                table: "tb_PassengerDetails",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Gender",
                table: "tb_PassengerDetails",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "TotalAmount_Meals_discount",
                table: "tb_PassengerDetails",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalAmount_Seat_tax_discount",
                table: "tb_PassengerDetails",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BookingType",
                table: "tb_Booking",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "PaidStatus",
                table: "tb_Booking",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "SeatAdjustment",
                table: "tb_Booking",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "TripType",
                table: "tb_Booking",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "tb_Trips",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OutboundFlightID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReturnFlightID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TripType = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TripStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BookingDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tb_Trips", x => x.ID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tb_Trips");

            migrationBuilder.DropColumn(
                name: "SeatAdjustment",
                table: "tb_PassengerTotal");

            migrationBuilder.DropColumn(
                name: "BaggageTotalAmountTax",
                table: "tb_PassengerDetails");

            migrationBuilder.DropColumn(
                name: "BaggageTotalAmountTax_discount",
                table: "tb_PassengerDetails");

            migrationBuilder.DropColumn(
                name: "Gender",
                table: "tb_PassengerDetails");

            migrationBuilder.DropColumn(
                name: "TotalAmount_Meals_discount",
                table: "tb_PassengerDetails");

            migrationBuilder.DropColumn(
                name: "TotalAmount_Seat_tax_discount",
                table: "tb_PassengerDetails");

            migrationBuilder.DropColumn(
                name: "BookingType",
                table: "tb_Booking");

            migrationBuilder.DropColumn(
                name: "PaidStatus",
                table: "tb_Booking");

            migrationBuilder.DropColumn(
                name: "SeatAdjustment",
                table: "tb_Booking");

            migrationBuilder.DropColumn(
                name: "TripType",
                table: "tb_Booking");
        }
    }
}
