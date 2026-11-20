using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnfathomablePayLink.Migrations;

/// <inheritdoc />
public partial class _20261119235235_RestrictPaymentBusinessDelete : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Payments_Businesses_BusinessId",
            table: "Payments");

        migrationBuilder.AddForeignKey(
            name: "FK_Payments_Businesses_BusinessId",
            table: "Payments",
            column: "BusinessId",
            principalTable: "Businesses",
            principalColumn: "Id",
            onDelete: ReferentialAction.Restrict);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropForeignKey(
            name: "FK_Payments_Businesses_BusinessId",
            table: "Payments");

        migrationBuilder.AddForeignKey(
            name: "FK_Payments_Businesses_BusinessId",
            table: "Payments",
            column: "BusinessId",
            principalTable: "Businesses",
            principalColumn: "Id",
            onDelete: ReferentialAction.Cascade);
    }
}
