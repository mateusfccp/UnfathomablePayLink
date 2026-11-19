using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnfathomablePayLink.Migrations;

/// <inheritdoc />
public partial class _20261119180019_AddBusinessInvoiceProviderKey : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "InvoiceProviderKey",
            table: "Businesses",
            type: "TEXT",
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "InvoiceProviderKey",
            table: "Businesses");
    }
}
