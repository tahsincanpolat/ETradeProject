using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ETICARET.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class b : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItem_Products_ProductId1",
                table: "CartItem");

            migrationBuilder.DropIndex(
                name: "IX_CartItem_ProductId1",
                table: "CartItem");

            migrationBuilder.DropColumn(
                name: "ProductId1",
                table: "CartItem");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "CartItem",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_ProductId",
                table: "CartItem",
                column: "ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItem_Products_ProductId",
                table: "CartItem",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItem_Products_ProductId",
                table: "CartItem");

            migrationBuilder.DropIndex(
                name: "IX_CartItem_ProductId",
                table: "CartItem");

            migrationBuilder.AlterColumn<string>(
                name: "ProductId",
                table: "CartItem",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ProductId1",
                table: "CartItem",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CartItem_ProductId1",
                table: "CartItem",
                column: "ProductId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItem_Products_ProductId1",
                table: "CartItem",
                column: "ProductId1",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
