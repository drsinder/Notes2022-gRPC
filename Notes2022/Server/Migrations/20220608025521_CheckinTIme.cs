using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notes2022.Server.Migrations
{
    /// <inheritdoc />
    public partial class CheckinTIme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "StartTime",
                table: "ActiveUsers",
                newName: "CheckinTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "CheckinTime",
                table: "ActiveUsers",
                newName: "StartTime");
        }
    }
}
