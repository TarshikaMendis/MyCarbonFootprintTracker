using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarbonFootprintTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWasteEmission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "WasteEmission",
                table: "CarbonRecords",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "WasteEmission",
                table: "CarbonRecords");
        }
    }
}
