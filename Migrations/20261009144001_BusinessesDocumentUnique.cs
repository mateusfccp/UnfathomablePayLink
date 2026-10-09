using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace unfathomable_pay_link.Migrations
{
    /// <inheritdoc />
    public partial class BusinessesDocumentUnique : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Businesses_Document",
                table: "Businesses",
                column: "Document",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Businesses_Document",
                table: "Businesses");
        }
    }
}
