using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PryCafeteria.Migrations
{
    /// <inheritdoc />
    public partial class AgregarFechaCreacionCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Criterio 6.0 HU-Categorías: agregar FechaCreacion
            // Usa GETDATE() como default para que las categorías existentes
            // reciban la fecha actual como valor inicial sin errores.
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Categorias",
                type: "datetime",
                nullable: false,
                defaultValueSql: "(getdate())");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Categorias");
        }
    }
}
