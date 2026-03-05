using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PryCafeteria.Migrations
{
    /// <summary>
    /// HU-Stock E9: tabla de auditoría para movimientos de stock (Ingreso, Venta, Ajuste)
    /// </summary>
    public partial class AgregarStockMovimientos : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StockMovimientos",
                columns: table => new
                {
                    StockMovimientoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),

                    ProductoTamanioId = table.Column<int>(type: "int", nullable: false),

                    Tipo = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),

                    Cantidad = table.Column<int>(type: "int", nullable: false),

                    StockResultante = table.Column<int>(type: "int", nullable: false),

                    Fecha = table.Column<DateTime>(type: "datetime", nullable: false,
                        defaultValueSql: "(getdate())"),

                    UsuarioId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StockMovimientos", x => x.StockMovimientoId);

                    table.ForeignKey(
                        name: "FK_StockMovimientos_ProductosTamanios",
                        column: x => x.ProductoTamanioId,
                        principalTable: "ProductosTamanios",
                        principalColumn: "ProductoTamanioId",
                        onDelete: ReferentialAction.Restrict);

                    table.ForeignKey(
                        name: "FK_StockMovimientos_AspNetUsers",
                        column: x => x.UsuarioId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            // Índice para consultas rápidas de historial por variante
            migrationBuilder.CreateIndex(
                name: "IX_StockMovimientos_ProductoTamanioId",
                table: "StockMovimientos",
                column: "ProductoTamanioId");

            // Índice por usuario para auditoría
            migrationBuilder.CreateIndex(
                name: "IX_StockMovimientos_UsuarioId",
                table: "StockMovimientos",
                column: "UsuarioId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "StockMovimientos");
        }
    }
}
