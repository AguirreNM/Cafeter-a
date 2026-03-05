using ClosedXML.Excel;
using PryCafeteria.Models;

namespace PryCafeteria.Services
{
    public static class ExcelService
    {
        private static readonly XLColor ColorHeader     = XLColor.FromHtml("#00704A");
        private static readonly XLColor ColorHeaderDark = XLColor.FromHtml("#1B4332");
        private static readonly XLColor ColorWhite      = XLColor.White;
        private static readonly XLColor ColorRowAlt     = XLColor.FromHtml("#F4FAF7");
        private static readonly XLColor ColorBorder     = XLColor.FromHtml("#D1D5DB");
        private static readonly XLColor ColorGreen      = XLColor.FromHtml("#DCFCE7");
        private static readonly XLColor ColorAmber      = XLColor.FromHtml("#FEF3C7");
        private static readonly XLColor ColorRed        = XLColor.FromHtml("#FEE2E2");
        private static readonly XLColor ColorPurple     = XLColor.FromHtml("#EDE9FE");
        private static readonly XLColor ColorOrange     = XLColor.FromHtml("#FFEDD5");

        // Formato moneda SIN prefijo "S/" — evita corrupcion en Excel
        private const string FmtMoneda = "#,##0.00";

        private static void EstiloHeader(IXLCell cell, XLColor bg)
        {
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontSize = 11;
            cell.Style.Font.FontColor = ColorWhite;
            cell.Style.Fill.BackgroundColor = bg;
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical   = XLAlignmentVerticalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            cell.Style.Border.OutsideBorderColor = bg;
        }

        private static void EstiloFila(IXLWorksheet ws, int row, int cols, XLColor? bg = null)
        {
            var color = bg ?? (row % 2 == 0 ? ColorRowAlt : ColorWhite);
            for (int c = 1; c <= cols; c++)
            {
                ws.Cell(row, c).Style.Fill.BackgroundColor = color;
                ws.Cell(row, c).Style.Font.FontSize = 10;
                ws.Cell(row, c).Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                ws.Cell(row, c).Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                ws.Cell(row, c).Style.Border.OutsideBorderColor = ColorBorder;
            }
        }

        private static void Centrar(IXLWorksheet ws, int lastRow, params int[] cols)
        {
            foreach (var c in cols)
                ws.Range(1, c, lastRow, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        // ── Stock ─────────────────────────────────────────────────────────
        public static byte[] ExportarStock(IEnumerable<ProductosTamanio> datos)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Stock");

            string[] headers = { "Producto", "Categoria", "Tamano", "Precio (S/)", "Stock", "Estado" };
            for (int i = 0; i < headers.Length; i++)
                EstiloHeader(ws.Cell(1, i + 1).SetValue(headers[i]), ColorHeader);
            ws.Row(1).Height = 22;

            int row = 2;
            var lista = datos.ToList();
            foreach (var pt in lista)
            {
                var estado = pt.Stock == 0 ? "Agotado"
                           : pt.Stock <= 5 ? "Bajo stock"
                           : "En stock";

                XLColor? colorFila = estado switch
                {
                    "Agotado"    => ColorRed,
                    "Bajo stock" => ColorAmber,
                    _            => null
                };

                ws.Cell(row, 1).Value = pt.Producto?.NombreProducto ?? "";
                ws.Cell(row, 2).Value = pt.Producto?.Categoria?.NombreCategoria ?? "";
                ws.Cell(row, 3).Value = pt.Tamanio?.NombreTamanio ?? "";
                ws.Cell(row, 4).Value = (double)pt.Precio;
                ws.Cell(row, 4).Style.NumberFormat.Format = FmtMoneda;
                ws.Cell(row, 5).Value = pt.Stock;
                ws.Cell(row, 6).Value = estado;

                EstiloFila(ws, row, headers.Length, colorFila);
                ws.Row(row).Height = 18;
                row++;
            }

            Centrar(ws, row - 1, 3, 4, 5, 6);

            // Fila total
            ws.Cell(row, 4).Value = "TOTAL";
            ws.Cell(row, 4).Style.Font.Bold = true;
            ws.Cell(row, 4).Style.Fill.BackgroundColor = XLColor.FromHtml("#D1FAE5");
            ws.Cell(row, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, 5).Value = lista.Sum(x => x.Stock);
            ws.Cell(row, 5).Style.Font.Bold = true;
            ws.Cell(row, 5).Style.Fill.BackgroundColor = XLColor.FromHtml("#D1FAE5");
            ws.Cell(row, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // ── Pedidos ───────────────────────────────────────────────────────
        public static byte[] ExportarPedidos(IEnumerable<Pedido> datos)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Pedidos");

            string[] headers = { "#Pedido", "Cliente", "Email", "Fecha", "Productos",
                                  "Tipo Entrega", "Metodo Pago", "Descuento (S/)", "Total (S/)", "Estado" };
            for (int i = 0; i < headers.Length; i++)
                EstiloHeader(ws.Cell(1, i + 1).SetValue(headers[i]), ColorHeaderDark);
            ws.Row(1).Height = 22;

            int row = 2;
            var lista = datos.ToList();
            foreach (var p in lista)
            {
                XLColor? colorFila = p.Estado switch
                {
                    "Entregado" => ColorGreen,
                    "Cancelado" => ColorRed,
                    "EnProceso" => ColorPurple,
                    "Listo"     => ColorOrange,
                    _           => ColorAmber
                };

                var resumen = p.DetallePedidos.Any()
                    ? string.Join(", ", p.DetallePedidos.Select(d =>
                        $"{d.Cantidad}x {d.NombreProductoSnapshot ?? "Producto"}"))
                    : "";

                ws.Cell(row, 1).Value = p.PedidoId;
                ws.Cell(row, 2).Value = $"{p.Usuario?.Nombre} {p.Usuario?.Apellido}".Trim();
                ws.Cell(row, 3).Value = p.Usuario?.Email ?? "";
                ws.Cell(row, 4).Value = p.FechaPedido.ToString("dd/MM/yyyy HH:mm");
                ws.Cell(row, 5).Value = resumen;
                ws.Cell(row, 6).Value = p.TipoEntrega;
                ws.Cell(row, 7).Value = p.MetodoPago?.NombreMetodoPago ?? "";
                ws.Cell(row, 8).Value = (double)p.Descuento;
                ws.Cell(row, 8).Style.NumberFormat.Format = FmtMoneda;
                ws.Cell(row, 9).Value = (double)p.Total;
                ws.Cell(row, 9).Style.NumberFormat.Format = FmtMoneda;
                ws.Cell(row, 10).Value = p.Estado;

                EstiloFila(ws, row, headers.Length, colorFila);
                ws.Row(row).Height = 18;
                row++;
            }

            Centrar(ws, row - 1, 1, 4, 6, 7, 8, 9, 10);

            // Totales
            ws.Cell(row, 8).Value = "TOTAL";
            ws.Cell(row, 8).Style.Font.Bold = true;
            ws.Cell(row, 8).Style.Fill.BackgroundColor = XLColor.FromHtml("#D1FAE5");
            ws.Cell(row, 8).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Cell(row, 9).Value = (double)lista.Sum(x => x.Total);
            ws.Cell(row, 9).Style.NumberFormat.Format = FmtMoneda;
            ws.Cell(row, 9).Style.Font.Bold = true;
            ws.Cell(row, 9).Style.Fill.BackgroundColor = XLColor.FromHtml("#D1FAE5");
            ws.Cell(row, 9).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Columns().AdjustToContents();
            ws.Column(5).Width = 40;

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // ── Clientes frecuentes ───────────────────────────────────────────
        public static byte[] ExportarClientesFrecuentes(
            IEnumerable<(string Nombre, string Email, int TotalPedidos, decimal TotalGastado)> datos)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Clientes");

            string[] headers = { "#", "Nombre", "Email", "Total Pedidos", "Total Gastado (S/)" };
            for (int i = 0; i < headers.Length; i++)
                EstiloHeader(ws.Cell(1, i + 1).SetValue(headers[i]), ColorHeaderDark);
            ws.Row(1).Height = 22;

            int row = 2;
            int rank = 1;
            foreach (var (nombre, email, totalPedidos, totalGastado) in datos)
            {
                ws.Cell(row, 1).Value = rank++;
                ws.Cell(row, 2).Value = nombre;
                ws.Cell(row, 3).Value = email;
                ws.Cell(row, 4).Value = totalPedidos;
                ws.Cell(row, 5).Value = (double)totalGastado;
                ws.Cell(row, 5).Style.NumberFormat.Format = FmtMoneda;

                EstiloFila(ws, row, headers.Length);
                ws.Row(row).Height = 18;
                row++;
            }

            Centrar(ws, row - 1, 1, 4, 5);
            ws.Columns().AdjustToContents();

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }

        // ── Productos ─────────────────────────────────────────────────────
        public static byte[] ExportarProductos(IEnumerable<Producto> datos)
        {
            using var wb = new XLWorkbook();
            var ws = wb.Worksheets.Add("Productos");

            string[] headers = { "ID", "Nombre", "Categoria", "Descripcion", "Disponible" };
            for (int i = 0; i < headers.Length; i++)
                EstiloHeader(ws.Cell(1, i + 1).SetValue(headers[i]), ColorHeader);
            ws.Row(1).Height = 22;

            int row = 2;
            foreach (var p in datos)
            {
                ws.Cell(row, 1).Value = p.ProductoId;
                ws.Cell(row, 2).Value = p.NombreProducto;
                ws.Cell(row, 3).Value = p.Categoria?.NombreCategoria ?? "";
                ws.Cell(row, 4).Value = p.Descripcion ?? "";
                ws.Cell(row, 5).Value = p.Disponible ? "Si" : "No";

                XLColor? colorFila = p.Disponible ? null : ColorRed;
                EstiloFila(ws, row, headers.Length, colorFila);
                ws.Row(row).Height = 18;
                row++;
            }

            Centrar(ws, row - 1, 1, 5);
            ws.Columns().AdjustToContents();
            ws.Column(4).Width = 35;

            using var ms = new MemoryStream();
            wb.SaveAs(ms);
            return ms.ToArray();
        }
    }
}
