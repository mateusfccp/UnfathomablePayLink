using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnfathomablePayLink.Migrations;

/// <inheritdoc />
public partial class _20261119024544_AddApiKey : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "ApiKey",
            table: "Businesses",
            type: "TEXT",
            nullable: false,
            defaultValue: "");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "ApiKey",
            table: "Businesses");
    }
}
