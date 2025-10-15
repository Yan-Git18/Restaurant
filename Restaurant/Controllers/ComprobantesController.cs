using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Restaurant.Models;
using RESTAURANT.Data;

namespace Restaurant.Controllers
{
    [Authorize(Roles = "Administrador, Cajero")]
    public class ComprobantesController : Controller
    {
        private readonly AppDbContext _context;

        public ComprobantesController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var comprobantes = await _context.Comprobantes
                .Include(c => c.Venta)
                .OrderByDescending(c => c.FechaEmision)
                .ToListAsync();

            return View(comprobantes);
        }

        [HttpGet]
        public IActionResult Crear()
        {
            ViewBag.Ventas = _context.Ventas
                .Include(v => v.Pedido)
                    .ThenInclude(p => p.Cliente)
                .Where(v => v.Comprobante == null)
                .ToList();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Rest_Comprobante comprobante)
        {
            comprobante.FechaEmision = DateTime.Now;
            _context.Add(comprobante);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Comprobante generado correctamente.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var comprobante = await _context.Comprobantes.FindAsync(id);
            if (comprobante == null)
                return NotFound();

            ViewBag.Ventas = await _context.Ventas
                .Include(v => v.Pedido)
                    .ThenInclude(p => p.Cliente)
                .ToListAsync();

            return View(comprobante);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Rest_Comprobante comprobante)
        {
            if (id != comprobante.Id)
                return NotFound();

            comprobante.FechaEmision = DateTime.Now;
            _context.Update(comprobante);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Comprobante actualizado correctamente.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Detalles(int id)
        {
            var comprobante = await _context.Comprobantes
                .Include(c => c.Venta)
                    .ThenInclude(v => v.Pedido)
                        .ThenInclude(p => p.Cliente)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comprobante == null)
                return NotFound();

            return View(comprobante);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmar(int id)
        {
            var comprobante = await _context.Comprobantes.FindAsync(id);
            if (comprobante == null)
                return NotFound();

            _context.Comprobantes.Remove(comprobante);
            await _context.SaveChangesAsync();

            TempData["Mensaje"] = "Comprobante eliminado correctamente.";
            TempData["Tipo"] = "success";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> GenerarPDF(int id)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var comprobante = await _context.Comprobantes
                .Include(c => c.Venta)
                    .ThenInclude(v => v.Pedido)
                        .ThenInclude(p => p.Cliente)
                .Include(c => c.Venta)
                    .ThenInclude(v => v.Pedido)
                        .ThenInclude(p => p.DetallesPedido)
                            .ThenInclude(dp => dp.Producto)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (comprobante == null)
                return NotFound();

            var pedido = comprobante.Venta?.Pedido;
            var cliente = pedido?.Cliente;
            var detalles = pedido?.DetallesPedido ?? new List<Rest_DetallePedido>();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(40);
                    page.Size(PageSizes.A4);

                    page.Header().Column(header =>
                    {
                        // Banner superior
                        header.Item().Background("#D32F2F").Padding(15).Column(bannerCol =>
                        {
                            bannerCol.Item().Text("Delizioso Restaurant 🍝")
                                .FontSize(26)
                                .Bold()
                                .FontColor("#FFFFFF");

                            bannerCol.Item().Text("Comida italiana auténtica")
                                .FontSize(11)
                                .FontColor("#FFEBEE");
                        });

                        // Información de la empresa
                        header.Item().Padding(10).Background("#FAFAFA").Row(row =>
                        {
                            row.RelativeItem().Column(col =>
                            {
                                col.Item().Text("RUC: 20481234567").FontSize(9).SemiBold();
                                col.Item().Text("Av. Gourmet 123 - Lima, Perú").FontSize(9);
                                col.Item().Text("Tel: (01) 234-5678").FontSize(9);
                            });

                            row.RelativeItem().AlignRight().Column(col =>
                            {
                                col.Item().Background("#D32F2F").Padding(8).Column(innerCol =>
                                {
                                    innerCol.Item().Text("COMPROBANTE")
                                        .FontSize(11)
                                        .Bold()
                                        .FontColor("#FFFFFF")
                                        .AlignCenter();
                                    innerCol.Item().Text($"N° {comprobante.Numero}")
                                        .FontSize(14)
                                        .Bold()
                                        .FontColor("#FFFFFF")
                                        .AlignCenter();
                                });
                            });
                        });

                        header.Item().PaddingVertical(5);
                    });

                    // Contenido principal
                    page.Content().PaddingVertical(10).Column(col =>
                    {
                        // Información del comprobante y cliente
                        col.Item().Row(row =>
                        {
                            // Información del comprobante
                            row.RelativeItem().Padding(5).Column(cardCol =>
                            {
                                cardCol.Item().Background("#E3F2FD").Padding(12).Column(infoCol =>
                                {
                                    infoCol.Item().Text("Información del Comprobante")
                                        .FontSize(11)
                                        .Bold()
                                        .FontColor("#1565C0");

                                    infoCol.Item().PaddingTop(5).Text($"Tipo: {comprobante.Tipo}")
                                        .FontSize(10);
                                    infoCol.Item().Text($"Fecha: {comprobante.FechaEmision:dd/MM/yyyy}")
                                        .FontSize(10);
                                    infoCol.Item().Text($"Hora: {comprobante.FechaEmision:HH:mm}")
                                        .FontSize(10);
                                });
                            });

                            // Información del cliente
                            row.RelativeItem().Padding(5).Column(cardCol =>
                            {
                                cardCol.Item().Background("#F3E5F5").Padding(12).Column(infoCol =>
                                {
                                    infoCol.Item().Text("Datos del Cliente")
                                        .FontSize(11)
                                        .Bold()
                                        .FontColor("#6A1B9A");

                                    infoCol.Item().PaddingTop(5).Text($"{cliente?.Nombre} {cliente?.Apellidos}")
                                        .FontSize(10)
                                        .SemiBold();
                                    infoCol.Item().Text($"Correo: {cliente?.Correo}")
                                        .FontSize(9);
                                });
                            });
                        });

                        col.Item().PaddingVertical(15);

                        // Título de la tabla
                        col.Item().Background("#D32F2F").Padding(8).Text("Detalle del Pedido")
                            .FontSize(13)
                            .Bold()
                            .FontColor("#FFFFFF");

                        col.Item().PaddingVertical(5);

                        // Tabla de productos
                        col.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.2f);
                                columns.RelativeColumn(1.3f);
                            });
                                                        
                            table.Header(header =>
                            {
                                header.Cell().Element(CellHeader).Text("#");
                                header.Cell().Element(CellHeader).Text("Producto");
                                header.Cell().Element(CellHeader).AlignCenter().Text("Cant.");
                                header.Cell().Element(CellHeader).AlignRight().Text("P. Unit.");
                                header.Cell().Element(CellHeader).AlignRight().Text("Subtotal");
                                header.Cell().Element(CellHeader).AlignRight().Text("IGV");
                                header.Cell().Element(CellHeader).AlignRight().Text("Total");

                                static IContainer CellHeader(IContainer container)
                                {
                                    return container
                                        .Background("#FFCDD2")
                                        .Padding(8)
                                        .DefaultTextStyle(x => x.FontSize(10).Bold().FontColor("#B71C1C"));
                                }
                            });

                            int i = 1;
                            foreach (var det in detalles)
                            {
                                var precio = det.Producto?.Precio ?? 0;
                                var subtotal = det.Subtotal;
                                var igv = subtotal * 0.18m;
                                var total = subtotal + igv;

                                var bgColor = i % 2 == 0 ? "#FFFFFF" : "#FAFAFA";

                                table.Cell().Element(c => CellBody(c, bgColor)).Text(i++.ToString());
                                table.Cell().Element(c => CellBody(c, bgColor)).Text(det.Producto?.Nombre ?? "-");
                                table.Cell().Element(c => CellBody(c, bgColor)).AlignCenter().Text(det.Cantidad.ToString());
                                table.Cell().Element(c => CellBody(c, bgColor)).AlignRight().Text($"S/ {precio:F2}");
                                table.Cell().Element(c => CellBody(c, bgColor)).AlignRight().Text($"S/ {subtotal:F2}");
                                table.Cell().Element(c => CellBody(c, bgColor)).AlignRight().Text($"S/ {igv:F2}");
                                table.Cell().Element(c => CellBody(c, bgColor)).AlignRight().Text($"S/ {total:F2}").SemiBold();
                            }

                            static IContainer CellBody(IContainer container, string bgColor)
                            {
                                return container
                                    .Background(bgColor)
                                    .Padding(8)
                                    .DefaultTextStyle(x => x.FontSize(9.5f));
                            }
                        });

                        col.Item().PaddingVertical(10);

                        // Sumas totales
                        col.Item().AlignRight().Width(250).Column(totalCol =>
                        {
                            decimal subtotal = detalles.Sum(d => d.Subtotal);
                            decimal igv = subtotal * 0.18m;
                            decimal total = subtotal + igv;

                            totalCol.Item().BorderBottom(1).BorderColor("#E0E0E0").Padding(5).Row(row =>
                            {
                                row.RelativeItem().Text("Subtotal:").FontSize(11);
                                row.RelativeItem().AlignRight().Text($"S/ {subtotal:F2}").FontSize(11);
                            });

                            totalCol.Item().BorderBottom(1).BorderColor("#E0E0E0").Padding(5).Row(row =>
                            {
                                row.RelativeItem().Text("IGV (18%):").FontSize(11);
                                row.RelativeItem().AlignRight().Text($"S/ {igv:F2}").FontSize(11);
                            });

                            totalCol.Item().Background("#D32F2F").Padding(10).Row(row =>
                            {
                                row.RelativeItem().Text("TOTAL:").FontSize(13).Bold().FontColor("#FFFFFF");
                                row.RelativeItem().AlignRight().Text($"S/ {total:F2}").FontSize(14).Bold().FontColor("#FFFFFF");
                            });
                        });

                        col.Item().PaddingVertical(15);

                        // Mensaje de agradecimiento
                        col.Item().Background("#E8F5E9").Padding(12).Column(thanksCol =>
                        {
                            thanksCol.Item().Text("¡Gracias por su preferencia! 💚")
                                .FontSize(13)
                                .Bold()
                                .AlignCenter()
                                .FontColor("#2E7D32");

                            thanksCol.Item().PaddingTop(3).Text("Esperamos volver a atenderle pronto")
                                .FontSize(10)
                                .AlignCenter()
                                .Italic()
                                .FontColor("#388E3C");
                        });
                    });

                    // Pie de página
                    page.Footer().BorderTop(1).BorderColor("#E0E0E0").PaddingTop(10).Column(footer =>
                    {
                        footer.Item().AlignCenter().Text(txt =>
                        {
                            txt.Span("Documento generado el ").FontSize(8).FontColor("#757575");
                            txt.Span(DateTime.Now.ToString("dd/MM/yyyy HH:mm")).FontSize(8).SemiBold().FontColor("#424242");
                        });

                        footer.Item().AlignCenter().Text("www.deliziosorestaurant.com | contacto@delizioso.pe")
                            .FontSize(8)
                            .FontColor("#757575");
                    });
                });
            });

            var pdfBytes = document.GeneratePdf();
            return File(pdfBytes, "application/pdf", $"Comprobante_{comprobante.Numero}.pdf");
        }

    }
}
