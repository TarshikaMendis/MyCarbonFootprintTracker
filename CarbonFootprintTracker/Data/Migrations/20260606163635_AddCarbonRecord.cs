using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarbonFootprintTracker.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCarbonRecord : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CarbonRecords",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TransportEmission = table.Column<double>(type: "float", nullable: false),
                    ElectricityEmission = table.Column<double>(type: "float", nullable: false),
                    FoodEmission = table.Column<double>(type: "float", nullable: false),
                    TotalEmission = table.Column<double>(type: "float", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarbonRecords", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarbonRecords");
        }
    }
}
