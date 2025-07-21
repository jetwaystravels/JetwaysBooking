using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RepositoryLayer.Migrations
{
    /// <inheritdoc />
    public partial class tb_passengerTotal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdultCount",
                table: "tb_PassengerTotal",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ChildCount",
                table: "tb_PassengerTotal",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InfantCount",
                table: "tb_PassengerTotal",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalPax",
                table: "tb_PassengerTotal",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "contact_Mobileno",
                table: "tb_PassengerDetails",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FastForwardService",
                table: "tb_PassengerDetails",
                type: "nvarchar(1)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FrequentFlyerNumber",
                table: "tb_PassengerDetails",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "MobileNumber",
                table: "ContactDetail",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "ContactDetail",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdultCount",
                table: "tb_PassengerTotal");

            migrationBuilder.DropColumn(
                name: "ChildCount",
                table: "tb_PassengerTotal");

            migrationBuilder.DropColumn(
                name: "InfantCount",
                table: "tb_PassengerTotal");

            migrationBuilder.DropColumn(
                name: "TotalPax",
                table: "tb_PassengerTotal");

            migrationBuilder.DropColumn(
                name: "FastForwardService",
                table: "tb_PassengerDetails");

            migrationBuilder.DropColumn(
                name: "FrequentFlyerNumber",
                table: "tb_PassengerDetails");

            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "ContactDetail");

            migrationBuilder.AlterColumn<int>(
                name: "contact_Mobileno",
                table: "tb_PassengerDetails",
                type: "int",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MobileNumber",
                table: "ContactDetail",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
