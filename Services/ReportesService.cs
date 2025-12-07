using ClosedXML.Excel;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace restapi.inventarios.Services
{
    public class VentaReporteItem
    {
        public int IdVenta { get; set; }
        public DateTime Fecha { get; set; }
        public string Vendedor { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public List<DetalleReporteItem> Detalles { get; set; } = new();
    }

    public class DetalleReporteItem
    {
        public int IdProducto { get; set; }
        public string? NombreProducto { get; set; }
        public int Cantidad { get; set; }
        public decimal Precio { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
    }

    public class ReportesService
    {
        static ReportesService()
        {
            // Configurar licencia de QuestPDF (Community es gratuita)
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public byte[] GenerarExcelVentas(List<VentaReporteItem> ventas, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            using var workbook = new XLWorkbook();
            
            // Hoja de Resumen de Ventas
            var wsResumen = workbook.Worksheets.Add("Resumen Ventas");
            
            // Título
            wsResumen.Cell(1, 1).Value = "REPORTE DE VENTAS";
            wsResumen.Cell(1, 1).Style.Font.Bold = true;
            wsResumen.Cell(1, 1).Style.Font.FontSize = 16;
            wsResumen.Range(1, 1, 1, 4).Merge();
            
            // Fechas del reporte
            var fechaReporte = $"Generado: {DateTime.Now:dd/MM/yyyy HH:mm}";
            if (fechaInicio.HasValue && fechaFin.HasValue)
                fechaReporte += $" | Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}";
            wsResumen.Cell(2, 1).Value = fechaReporte;
            wsResumen.Range(2, 1, 2, 4).Merge();
            
            // Encabezados
            var headers = new[] { "ID Venta", "Fecha", "Vendedor", "Total" };
            for (int i = 0; i < headers.Length; i++)
            {
                wsResumen.Cell(4, i + 1).Value = headers[i];
                wsResumen.Cell(4, i + 1).Style.Font.Bold = true;
                wsResumen.Cell(4, i + 1).Style.Fill.BackgroundColor = XLColor.LightBlue;
            }
            
            // Datos
            int row = 5;
            foreach (var venta in ventas)
            {
                wsResumen.Cell(row, 1).Value = venta.IdVenta;
                wsResumen.Cell(row, 2).Value = venta.Fecha;
                wsResumen.Cell(row, 2).Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                wsResumen.Cell(row, 3).Value = venta.Vendedor;
                wsResumen.Cell(row, 4).Value = venta.Total;
                wsResumen.Cell(row, 4).Style.NumberFormat.Format = "$#,##0.00";
                row++;
            }
            
            // Total general
            wsResumen.Cell(row + 1, 3).Value = "TOTAL GENERAL:";
            wsResumen.Cell(row + 1, 3).Style.Font.Bold = true;
            wsResumen.Cell(row + 1, 4).Value = ventas.Sum(v => v.Total);
            wsResumen.Cell(row + 1, 4).Style.NumberFormat.Format = "$#,##0.00";
            wsResumen.Cell(row + 1, 4).Style.Font.Bold = true;
            
            // Estadísticas
            wsResumen.Cell(row + 3, 1).Value = "ESTADÍSTICAS";
            wsResumen.Cell(row + 3, 1).Style.Font.Bold = true;
            wsResumen.Cell(row + 4, 1).Value = "Total de ventas:";
            wsResumen.Cell(row + 4, 2).Value = ventas.Count;
            wsResumen.Cell(row + 5, 1).Value = "Promedio por venta:";
            wsResumen.Cell(row + 5, 2).Value = ventas.Count > 0 ? ventas.Average(v => v.Total) : 0;
            wsResumen.Cell(row + 5, 2).Style.NumberFormat.Format = "$#,##0.00";
            
            wsResumen.Columns().AdjustToContents();
            
            // Hoja de Detalles
            var wsDetalles = workbook.Worksheets.Add("Detalles");
            
            var headersDetalle = new[] { "ID Venta", "Fecha", "Vendedor", "Producto", "Cantidad", "Precio Unit.", "IVA", "Total" };
            for (int i = 0; i < headersDetalle.Length; i++)
            {
                wsDetalles.Cell(1, i + 1).Value = headersDetalle[i];
                wsDetalles.Cell(1, i + 1).Style.Font.Bold = true;
                wsDetalles.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGreen;
            }
            
            row = 2;
            foreach (var venta in ventas)
            {
                foreach (var det in venta.Detalles)
                {
                    wsDetalles.Cell(row, 1).Value = venta.IdVenta;
                    wsDetalles.Cell(row, 2).Value = venta.Fecha;
                    wsDetalles.Cell(row, 2).Style.DateFormat.Format = "dd/MM/yyyy";
                    wsDetalles.Cell(row, 3).Value = venta.Vendedor;
                    wsDetalles.Cell(row, 4).Value = det.NombreProducto ?? $"Producto #{det.IdProducto}";
                    wsDetalles.Cell(row, 5).Value = det.Cantidad;
                    wsDetalles.Cell(row, 6).Value = det.Precio;
                    wsDetalles.Cell(row, 6).Style.NumberFormat.Format = "$#,##0.00";
                    wsDetalles.Cell(row, 7).Value = det.Iva;
                    wsDetalles.Cell(row, 7).Style.NumberFormat.Format = "$#,##0.00";
                    wsDetalles.Cell(row, 8).Value = det.Total;
                    wsDetalles.Cell(row, 8).Style.NumberFormat.Format = "$#,##0.00";
                    row++;
                }
            }
            
            wsDetalles.Columns().AdjustToContents();
            
            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] GenerarPdfVentas(List<VentaReporteItem> ventas, DateTime? fechaInicio = null, DateTime? fechaFin = null)
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(30);
                    page.DefaultTextStyle(x => x.FontSize(10));

                    page.Header().Element(c => ComponerEncabezado(c, fechaInicio, fechaFin));
                    page.Content().Element(c => ComponerContenido(c, ventas));
                    page.Footer().Element(ComponerPie);
                });
            });

            return document.GeneratePdf();
        }

        private void ComponerEncabezado(IContainer container, DateTime? fechaInicio, DateTime? fechaFin)
        {
            container.Column(column =>
            {
                column.Item().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("INVENTARIOS S.A.").FontSize(16).Bold().FontColor(Colors.Blue.Darken2);
                        col.Item().Text("Sistema de Gestión de Inventarios").FontSize(10).FontColor(Colors.Grey.Darken1);
                    });

                    row.ConstantItem(150).Column(col =>
                    {
                        col.Item().AlignRight().Text($"Fecha: {DateTime.Now:dd/MM/yyyy}").FontSize(9);
                        col.Item().AlignRight().Text($"Hora: {DateTime.Now:HH:mm}").FontSize(9);
                    });
                });

                column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Blue.Darken2);

                column.Item().AlignCenter().Text("REPORTE DE VENTAS").FontSize(14).Bold();

                if (fechaInicio.HasValue && fechaFin.HasValue)
                {
                    column.Item().AlignCenter().Text($"Período: {fechaInicio:dd/MM/yyyy} - {fechaFin:dd/MM/yyyy}").FontSize(10);
                }

                column.Item().PaddingBottom(10);
            });
        }

        private void ComponerContenido(IContainer container, List<VentaReporteItem> ventas)
        {
            container.Column(column =>
            {
                // Tabla de ventas
                column.Item().Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.ConstantColumn(50);  // ID
                        columns.RelativeColumn(2);   // Fecha
                        columns.RelativeColumn(3);   // Vendedor
                        columns.RelativeColumn(2);   // Total
                    });

                    // Encabezados
                    table.Header(header =>
                    {
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .Text("ID").FontColor(Colors.White).Bold().AlignCenter();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .Text("Fecha").FontColor(Colors.White).Bold().AlignCenter();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .Text("Vendedor").FontColor(Colors.White).Bold().AlignCenter();
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5)
                            .Text("Total").FontColor(Colors.White).Bold().AlignCenter();
                    });

                    // Filas
                    bool alternate = false;
                    foreach (var venta in ventas)
                    {
                        var bgColor = alternate ? Colors.Grey.Lighten4 : Colors.White;

                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(venta.IdVenta.ToString()).AlignCenter();
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(venta.Fecha.ToString("dd/MM/yyyy HH:mm")).AlignCenter();
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text(venta.Vendedor);
                        table.Cell().Background(bgColor).BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5)
                            .Text($"${venta.Total:N2}").AlignRight();

                        alternate = !alternate;
                    }
                });

                column.Item().PaddingTop(20);

                // Resumen
                column.Item().Background(Colors.Grey.Lighten3).Padding(15).Column(resumen =>
                {
                    resumen.Item().Text("RESUMEN").Bold().FontSize(12);
                    resumen.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Text($"Total de ventas: {ventas.Count}");
                        row.RelativeItem().Text($"Total general: ${ventas.Sum(v => v.Total):N2}").Bold();
                    });
                    if (ventas.Count > 0)
                    {
                        resumen.Item().Row(row =>
                        {
                            row.RelativeItem().Text($"Promedio por venta: ${ventas.Average(v => v.Total):N2}");
                            row.RelativeItem().Text($"Venta mayor: ${ventas.Max(v => v.Total):N2}");
                        });
                    }
                });

                // Detalles por venta
                if (ventas.Any(v => v.Detalles.Count > 0))
                {
                    column.Item().PaddingTop(20).Text("DETALLE POR VENTA").Bold().FontSize(12);

                    foreach (var venta in ventas.Where(v => v.Detalles.Count > 0))
                    {
                        column.Item().PaddingTop(10).Background(Colors.Grey.Lighten4).Padding(10).Column(ventaCol =>
                        {
                            ventaCol.Item().Text($"Venta #{venta.IdVenta} - {venta.Vendedor} - {venta.Fecha:dd/MM/yyyy}").Bold();

                            ventaCol.Item().PaddingTop(5).Table(detTable =>
                            {
                                detTable.ColumnsDefinition(cols =>
                                {
                                    cols.RelativeColumn(3); // Producto
                                    cols.ConstantColumn(60); // Cantidad
                                    cols.ConstantColumn(80); // Precio
                                    cols.ConstantColumn(80); // Total
                                });

                                detTable.Header(h =>
                                {
                                    h.Cell().Text("Producto").Bold().FontSize(9);
                                    h.Cell().Text("Cant.").Bold().FontSize(9).AlignCenter();
                                    h.Cell().Text("Precio").Bold().FontSize(9).AlignRight();
                                    h.Cell().Text("Total").Bold().FontSize(9).AlignRight();
                                });

                                foreach (var det in venta.Detalles)
                                {
                                    detTable.Cell().Text(det.NombreProducto ?? $"Producto #{det.IdProducto}").FontSize(9);
                                    detTable.Cell().Text(det.Cantidad.ToString()).FontSize(9).AlignCenter();
                                    detTable.Cell().Text($"${det.Precio:N2}").FontSize(9).AlignRight();
                                    detTable.Cell().Text($"${det.Total:N2}").FontSize(9).AlignRight();
                                }
                            });
                        });
                    }
                }
            });
        }

        private void ComponerPie(IContainer container)
        {
            container.Column(column =>
            {
                column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                column.Item().PaddingTop(5).Row(row =>
                {
                    row.RelativeItem().Text($"© {DateTime.Now.Year} Inventarios S.A.").FontSize(8).FontColor(Colors.Grey.Medium);
                    row.RelativeItem().AlignRight().Text(text =>
                    {
                        text.Span("Página ").FontSize(8);
                        text.CurrentPageNumber().FontSize(8);
                        text.Span(" de ").FontSize(8);
                        text.TotalPages().FontSize(8);
                    });
                });
            });
        }
    }
}
