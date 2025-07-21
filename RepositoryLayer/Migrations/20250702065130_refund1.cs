using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepositoryLayer.Migrations
{
    /// <inheritdoc />
    public partial class refund1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalMealsAmount_Tax",
                table: "tb_PassengerTotal",
                newName: "SpecialServicesAmount_Tax");

            migrationBuilder.RenameColumn(
                name: "TotalMealsAmount",
                table: "tb_PassengerTotal",
                newName: "SpecialServicesAmount");

            migrationBuilder.RenameColumn(
                name: "Status",
                table: "tb_Booking",
                newName: "CompanyName");

            migrationBuilder.AddColumn<double>(
                name: "InftAmount",
                table: "tb_PassengerDetails",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "InftAmount_Tax",
                table: "tb_PassengerDetails",
                type: "float",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "BookingStatus",
                table: "tb_Booking",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "RefundRequests",
                columns: table => new
                {
                    RefundID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrderID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CustomerID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BookingID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RecordLocator = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BookingAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    RefundAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DeductionAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    RefundReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RefundStatus = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RequestDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedBy = table.Column<int>(type: "int", nullable: true),
                    ApprovalDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentReversed = table.Column<bool>(type: "bit", nullable: true),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefundRequests", x => x.RefundID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RefundRequests");

            migrationBuilder.DropColumn(
                name: "InftAmount",
                table: "tb_PassengerDetails");

            migrationBuilder.DropColumn(
                name: "InftAmount_Tax",
                table: "tb_PassengerDetails");

            migrationBuilder.DropColumn(
                name: "BookingStatus",
                table: "tb_Booking");

            migrationBuilder.RenameColumn(
                name: "SpecialServicesAmount_Tax",
                table: "tb_PassengerTotal",
                newName: "TotalMealsAmount_Tax");

            migrationBuilder.RenameColumn(
                name: "SpecialServicesAmount",
                table: "tb_PassengerTotal",
                newName: "TotalMealsAmount");

            migrationBuilder.RenameColumn(
                name: "CompanyName",
                table: "tb_Booking",
                newName: "Status");
        }
    }
}
