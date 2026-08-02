using GaleriaArte.Web.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GaleriaArte.Web.Documents
{
    public class FacturaPdfDocument : IDocument
    {
        private readonly Factura _factura;

        public FacturaPdfDocument(Factura factura)
        {
            _factura = factura;
        }

        public DocumentMetadata GetMetadata()
        {
            return DocumentMetadata.Default;
        }

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);
                page.DefaultTextStyle(
                    style => style.FontSize(10)
                );

                page.Header().Element(ComponerEncabezado);

                page.Content()
                    .PaddingVertical(20)
                    .Element(ComponerContenido);

                page.Footer()
                    .AlignCenter()
                    .Text(text =>
                    {
                        text.Span(
                            "Documento generado por el Sistema de Gestión de la Galería de Arte · Página "
                        );

                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });
            });
        }

        private void ComponerEncabezado(
            IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(column =>
                {
                    column.Item()
                        .Text("GALERÍA DE ARTE")
                        .FontSize(22)
                        .Bold();

                    column.Item()
                        .Text("Comprobante de venta de obras de arte")
                        .FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(150)
                    .AlignRight()
                    .Column(column =>
                    {
                        column.Item()
                            .AlignRight()
                            .Text("FACTURA")
                            .FontColor(Colors.Grey.Darken1);

                        column.Item()
                            .AlignRight()
                            .Text($"#{_factura.IdFactura}")
                            .FontSize(22)
                            .Bold();
                    });
            });
        }

        private void ComponerContenido(
            IContainer container)
        {
            container.Column(column =>
            {
                column.Spacing(18);

                column.Item()
                    .LineHorizontal(1)
                    .LineColor(Colors.Grey.Lighten2);

                column.Item()
                    .Element(ComponerInformacion);

                column.Item()
                    .Element(ComponerTabla);

                column.Item()
                    .AlignRight()
                    .Width(260)
                    .Element(ComponerTotales);

                column.Item()
                    .PaddingTop(20)
                    .AlignCenter()
                    .Column(footer =>
                    {
                        footer.Item()
                            .Text("Gracias por su compra.")
                            .Bold();

                        footer.Item()
                            .Text(
                                "Conserve este documento como comprobante de la transacción."
                            )
                            .FontColor(Colors.Grey.Darken1);
                    });
            });
        }

        private void ComponerInformacion(
            IContainer container)
        {
            container.Row(row =>
            {
                row.RelativeItem()
                    .Border(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Padding(14)
                    .Column(column =>
                    {
                        column.Item()
                            .Text("Información del cliente")
                            .Bold();

                        column.Item()
                            .PaddingTop(8)
                            .Text($"Cliente: {_factura.NombreCliente}");
                    });

                row.ConstantItem(15);

                row.RelativeItem()
                    .Border(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .Padding(14)
                    .Column(column =>
                    {
                        column.Item()
                            .Text("Información de la venta")
                            .Bold();

                        column.Item()
                            .PaddingTop(8)
                            .Text(
                                $"Fecha: {_factura.FechaFactura:dd/MM/yyyy HH:mm}"
                            );

                        column.Item()
                            .Text(
                                $"Método de pago: {_factura.MetodoPago}"
                            );

                        column.Item()
                            .Text(
                                $"Vendedor: {_factura.NombreUsuario}"
                            );

                        column.Item()
                            .Text(
                                $"Estado: {_factura.EstadoFactura}"
                            );
                    });
            });
        }

        private void ComponerTabla(
            IContainer container)
        {
            container.Column(column =>
            {
                column.Item()
                    .Text("Obras adquiridas")
                    .FontSize(13)
                    .Bold();

                column.Item()
                    .PaddingTop(8)
                    .Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.ConstantColumn(70);
                            columns.RelativeColumn(3);
                            columns.ConstantColumn(60);
                            columns.ConstantColumn(100);
                            columns.ConstantColumn(100);
                        });

                        table.Header(header =>
                        {
                            header.Cell()
                                .Element(EstiloEncabezado)
                                .Text("Código");

                            header.Cell()
                                .Element(EstiloEncabezado)
                                .Text("Obra");

                            header.Cell()
                                .Element(EstiloEncabezado)
                                .AlignCenter()
                                .Text("Cantidad");

                            header.Cell()
                                .Element(EstiloEncabezado)
                                .AlignRight()
                                .Text("Precio");

                            header.Cell()
                                .Element(EstiloEncabezado)
                                .AlignRight()
                                .Text("Subtotal");
                        });

                        foreach (DetalleFactura detalle
                                 in _factura.Detalles)
                        {
                            table.Cell()
                                .Element(EstiloCelda)
                                .Text(detalle.CodigoObra);

                            table.Cell()
                                .Element(EstiloCelda)
                                .Text(detalle.NombreObra);

                            table.Cell()
                                .Element(EstiloCelda)
                                .AlignCenter()
                                .Text(detalle.Cantidad.ToString());

                            table.Cell()
                                .Element(EstiloCelda)
                                .AlignRight()
                                .Text(
                                    $"₡{detalle.PrecioUnitario:N2}"
                                );

                            table.Cell()
                                .Element(EstiloCelda)
                                .AlignRight()
                                .Text(
                                    $"₡{detalle.Subtotal:N2}"
                                );
                        }
                    });
            });
        }

        private void ComponerTotales(
            IContainer container)
        {
            container.Column(column =>
            {
                column.Item()
                    .Element(FilaTotal)
                    .Text(text =>
                    {
                        text.Span("Subtotal");
                        text.Span(
                            $"₡{_factura.Subtotal:N2}"
                        ).Bold();
                    });

                column.Item()
                    .Element(FilaTotal)
                    .Text(text =>
                    {
                        text.Span("IVA (13%)");
                        text.Span(
                            $"₡{_factura.Impuesto:N2}"
                        ).Bold();
                    });

                column.Item()
                    .PaddingTop(6)
                    .BorderTop(1)
                    .BorderColor(Colors.Grey.Lighten2)
                    .PaddingVertical(10)
                    .Text(text =>
                    {
                        text.AlignCenter();

                        text.Span("TOTAL: ")
                            .FontSize(14)
                            .Bold();

                        text.Span(
                            $"₡{_factura.Total:N2}"
                        )
                        .FontSize(14)
                        .Bold();
                    });
            });
        }

        private static IContainer EstiloEncabezado(
            IContainer container)
        {
            return container
                .Background(Colors.Grey.Lighten3)
                .PaddingVertical(8)
                .PaddingHorizontal(6)
                .DefaultTextStyle(
                    style => style.Bold()
                );
        }

        private static IContainer EstiloCelda(
            IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(Colors.Grey.Lighten3)
                .PaddingVertical(8)
                .PaddingHorizontal(6);
        }

        private static IContainer FilaTotal(
            IContainer container)
        {
            return container
                .PaddingVertical(6);
        }
    }
}