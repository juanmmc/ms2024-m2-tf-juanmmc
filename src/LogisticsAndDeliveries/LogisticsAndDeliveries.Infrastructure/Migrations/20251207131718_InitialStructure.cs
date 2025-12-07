using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LogisticsAndDeliveries.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "driver",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    latitude = table.Column<double>(type: "double precision", nullable: true),
                    longitude = table.Column<double>(type: "double precision", nullable: true),
                    lastLocationUpdate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_driver", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "package",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    driverId = table.Column<Guid>(type: "uuid", nullable: false),
                    number = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    patientId = table.Column<Guid>(type: "uuid", nullable: false),
                    patientName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    patientPhone = table.Column<string>(type: "character varying(15)", maxLength: 15, nullable: false),
                    deliveryAddress = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    deliveryLatitude = table.Column<double>(type: "double precision", nullable: false),
                    deliveryLongitude = table.Column<double>(type: "double precision", nullable: false),
                    deliveryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    deliveryOrder = table.Column<int>(type: "integer", nullable: false),
                    deliveryStatus = table.Column<string>(type: "text", nullable: false),
                    deliveryEvidence = table.Column<string>(type: "text", nullable: true),
                    incidentType = table.Column<string>(type: "text", nullable: true),
                    incidentDescription = table.Column<string>(type: "text", nullable: true),
                    updatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_package", x => x.id);
                    table.ForeignKey(
                        name: "FK_package_driver_driverId",
                        column: x => x.driverId,
                        principalTable: "driver",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_package_driverId",
                table: "package",
                column: "driverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "package");

            migrationBuilder.DropTable(
                name: "driver");
        }
    }
}
