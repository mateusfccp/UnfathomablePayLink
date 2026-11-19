using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnfathomablePayLink.Migrations;

/// <inheritdoc />
public partial class _20261119113900_RenamePaymentColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "state",
            table: "Payments",
            newName: "State");

        migrationBuilder.RenameColumn(
            name: "amount",
            table: "Payments",
            newName: "Amount");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.RenameColumn(
            name: "State",
            table: "Payments",
            newName: "state");

        migrationBuilder.RenameColumn(
            name: "Amount",
            table: "Payments",
            newName: "amount");
    }
}
