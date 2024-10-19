using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETICARET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class da_fixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Qunatity",
                table: "OrderItem",
                newName: "Quantity");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Quantity",
                table: "OrderItem",
                newName: "Qunatity");
        }
    }
}
