using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace InSureX.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BillFilePath",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DoctorName",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HospitalName",
                table: "Claims",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TreatmentDate",
                table: "Claims",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BillFilePath",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "DoctorName",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "HospitalName",
                table: "Claims");

            migrationBuilder.DropColumn(
                name: "TreatmentDate",
                table: "Claims");
        }
    }
}
