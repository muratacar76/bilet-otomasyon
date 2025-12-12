using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlightBooking.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateBookingStatusToTurkish : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Mevcut kayıtları Türkçe'ye çevir
            migrationBuilder.Sql("UPDATE Bookings SET Status = 'Onaylandı' WHERE Status = 'Confirmed'");
            migrationBuilder.Sql("UPDATE Bookings SET Status = 'İptal Edildi' WHERE Status = 'Cancelled'");
            migrationBuilder.Sql("UPDATE Bookings SET Status = 'Beklemede' WHERE Status = 'Pending'");
            
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Bookings",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Onaylandı",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20,
                oldDefaultValue: "Confirmed");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Bookings",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "Confirmed",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20,
                oldDefaultValue: "Onaylandı");
                
            // Kayıtları geri çevir
            migrationBuilder.Sql("UPDATE Bookings SET Status = 'Confirmed' WHERE Status = 'Onaylandı'");
            migrationBuilder.Sql("UPDATE Bookings SET Status = 'Cancelled' WHERE Status = 'İptal Edildi'");
            migrationBuilder.Sql("UPDATE Bookings SET Status = 'Pending' WHERE Status = 'Beklemede'");
        }
    }
}
