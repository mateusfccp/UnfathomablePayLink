using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnfathomablePayLink.Migrations;

/// <inheritdoc />
public partial class _20261119113931_AddBusinessWebhookUrl : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<string>(
            name: "WebhookUrl",
            table: "Businesses",
            type: "TEXT",
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "WebhookUrl",
            table: "Businesses");
    }
}
