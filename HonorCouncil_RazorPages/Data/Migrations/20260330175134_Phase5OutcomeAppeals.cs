using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HonorCouncil_RazorPages.Data.Migrations
{
    /// <inheritdoc />
    public partial class Phase5OutcomeAppeals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "OutcomeIssuedUtc",
                table: "HonorCases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OutcomeSummary",
                table: "HonorCases",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNotes",
                table: "Appeals",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedUtc",
                table: "Appeals",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OutcomeIssuedUtc",
                table: "HonorCases");

            migrationBuilder.DropColumn(
                name: "OutcomeSummary",
                table: "HonorCases");

            migrationBuilder.DropColumn(
                name: "ReviewNotes",
                table: "Appeals");

            migrationBuilder.DropColumn(
                name: "ReviewedUtc",
                table: "Appeals");
        }
    }
}
