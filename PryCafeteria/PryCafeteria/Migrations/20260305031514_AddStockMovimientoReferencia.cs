using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PryCafeteria.Migrations
{
    /// <inheritdoc />
    public partial class AddStockMovimientoReferencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Referencia",
                table: "StockMovimientos",
                type: "varchar(200)",
                unicode: false,
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Referencia",
                table: "StockMovimientos");
        }
    }
}
