using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightBooking.Persistence.Migrations
{
    public partial class AddSeatSelection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SeatNumber",
                table: "Passengers",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SeatType",
                table: "Passengers",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "SeatsPerRow",
                table: "Flights",
                type: "int",
                nullable: false,
                defaultValue: 6);

            migrationBuilder.AddColumn<int>(
                name: "TotalRows",
                table: "Flights",
                type: "int",
                nullable: false,
                defaultValue: 30);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SeatNumber",
                table: "Passengers");

            migrationBuilder.DropColumn(
                name: "SeatType",
                table: "Passengers");

            migrationBuilder.DropColumn(
                name: "SeatsPerRow",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "TotalRows",
                table: "Flights");
        }
    }
}
