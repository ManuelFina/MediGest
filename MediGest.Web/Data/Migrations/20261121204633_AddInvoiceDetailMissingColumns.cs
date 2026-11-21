using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MediGest.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddInvoiceDetailMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AuthorizationNumber",
                table: "InvoiceDetails",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "ServiceDate",
                table: "InvoiceDetails",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AuthorizationNumber",
                table: "InvoiceDetails");

            migrationBuilder.DropColumn(
                name: "ServiceDate",
                table: "InvoiceDetails");
        }
    }
}
