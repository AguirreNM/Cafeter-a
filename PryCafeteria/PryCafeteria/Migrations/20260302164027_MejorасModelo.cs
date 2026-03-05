using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PryCafeteria.Migrations
{
    /// <inheritdoc />
    public partial class MejorасModelo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Disponible",
                table: "ProductosTamanios",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "Disponible",
                table: "MetodosPago",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreProductoSnapshot",
                table: "DetallePedido",
                type: "varchar(100)",
                unicode: false,
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NombreTamanioSnapshot",
                table: "DetallePedido",
                type: "varchar(30)",
                unicode: false,
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Disponible",
                table: "Categorias",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Disponible",
                table: "ProductosTamanios");

            migrationBuilder.DropColumn(
                name: "Disponible",
                table: "MetodosPago");

            migrationBuilder.DropColumn(
                name: "NombreProductoSnapshot",
                table: "DetallePedido");

            migrationBuilder.DropColumn(
                name: "NombreTamanioSnapshot",
                table: "DetallePedido");

            migrationBuilder.DropColumn(
                name: "Disponible",
                table: "Categorias");
        }
    }
}
