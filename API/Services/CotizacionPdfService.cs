using Models.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace API.Services
{
    public class CotizacionPdfService : ICotizacionPdfService
    {
        private const string MoradoPrincipal = "#534AB7";
        private const string MoradoOscuro = "#3D3490";
        private const string MoradoClaro = "#F0EEFF";
        private const string FondoSeccion = "#F8F7FF";
        private const string BordeClaro = "#ECE9FF";
        private const string Verde = "#0F6E56";
        private const string VerdeClaro = "#E6F4F0";
        private const string TextoOscuro = "#1E1E1E";
        private const string TextoGris = "#6C757D";

        public byte[] GenerarPdf(Cotizacion cotizacion)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            var evento = cotizacion.Evento;
            var cliente = evento.Cliente;
            var paquete = evento.Paquete;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(0);

                    page.DefaultTextStyle(style =>
                        style
                            .FontSize(10)
                            .FontColor(TextoOscuro));

                    page.Header()
                        .Background(MoradoPrincipal)
                        .PaddingVertical(22)
                        .PaddingHorizontal(35)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Column(column =>
                                {
                                    column.Item()
                                        .Text("SIGESA")
                                        .FontSize(25)
                                        .Bold()
                                        .FontColor(Colors.White);

                                    column.Item()
                                        .Text("Sala de Eventos Mónica")
                                        .FontSize(10)
                                        .FontColor("#DDD9FF");
                                });

                            row.ConstantItem(180)
                                .AlignRight()
                                .AlignMiddle()
                                .Column(column =>
                                {
                                    column.Item()
                                        .AlignRight()
                                        .Text("COTIZACIÓN")
                                        .FontSize(20)
                                        .Bold()
                                        .FontColor(Colors.White);

                                    column.Item()
                                        .AlignRight()
                                        .Text($"N.º {cotizacion.CotizacionId}")
                                        .FontSize(10)
                                        .FontColor("#DDD9FF");
                                });
                        });

                    page.Content()
                        .PaddingHorizontal(35)
                        .PaddingVertical(25)
                        .Column(column =>
                        {
                            column.Spacing(16);

                            column.Item()
                                .Row(row =>
                                {
                                    row.RelativeItem()
                                        .Column(left =>
                                        {
                                            left.Spacing(3);

                                            left.Item()
                                                .Text("Fecha de emisión")
                                                .FontSize(9)
                                                .FontColor(TextoGris);

                                            left.Item()
                                                .Text(
                                                    cotizacion.FechaCreacion
                                                        .ToString("dd/MM/yyyy"))
                                                .Bold();
                                        });

                                    row.RelativeItem()
                                        .Column(center =>
                                        {
                                            center.Spacing(3);

                                            center.Item()
                                                .Text("Fecha de vencimiento")
                                                .FontSize(9)
                                                .FontColor(TextoGris);

                                            center.Item()
                                                .Text(
                                                    cotizacion.FechaVencimiento.HasValue
                                                        ? cotizacion.FechaVencimiento.Value
                                                            .ToString("dd/MM/yyyy")
                                                        : "No definida")
                                                .Bold();
                                        });

                                    row.RelativeItem()
                                        .AlignRight()
                                        .Column(right =>
                                        {
                                            right.Spacing(3);

                                            right.Item()
                                                .AlignRight()
                                                .Text("Estado")
                                                .FontSize(9)
                                                .FontColor(TextoGris);

                                            right.Item()
                                                .AlignRight()
                                                .Background(VerdeClaro)
                                                .PaddingHorizontal(10)
                                                .PaddingVertical(4)
                                                .Text(cotizacion.Estado)
                                                .Bold()
                                                .FontSize(9)
                                                .FontColor(Verde);
                                        });
                                });

                            column.Item()
                                .Element(container =>
                                    CrearSeccion(
                                        container,
                                        "DATOS DEL CLIENTE",
                                        content =>
                                        {
                                            content.Item()
                                                .Row(row =>
                                                {
                                                    row.RelativeItem()
                                                        .Column(item =>
                                                        {
                                                            AgregarEtiquetaValor(
                                                                item,
                                                                "Nombre",
                                                                cliente.Nombre);

                                                            AgregarEtiquetaValor(
                                                                item,
                                                                "Documento",
                                                                cliente.Documento);
                                                        });

                                                    row.RelativeItem()
                                                        .Column(item =>
                                                        {
                                                            AgregarEtiquetaValor(
                                                                item,
                                                                "Teléfono",
                                                                cliente.Telefono);

                                                            AgregarEtiquetaValor(
                                                                item,
                                                                "Correo electrónico",
                                                                cliente.Email ??
                                                                "No registrado");
                                                        });
                                                });
                                        }));

                            column.Item()
                                .Element(container =>
                                    CrearSeccion(
                                        container,
                                        "DETALLES DEL EVENTO",
                                        content =>
                                        {
                                            content.Item()
                                                .Row(row =>
                                                {
                                                    row.RelativeItem()
                                                        .Column(item =>
                                                        {
                                                            AgregarEtiquetaValor(
                                                                item,
                                                                "Tipo de evento",
                                                                evento.TipoEvento);

                                                            AgregarEtiquetaValor(
                                                                item,
                                                                "Cantidad de personas",
                                                                evento.NumPersonas
                                                                    .ToString());
                                                        });

                                                    row.RelativeItem()
                                                        .Column(item =>
                                                        {
                                                            AgregarEtiquetaValor(
                                                                item,
                                                                "Fecha del evento",
                                                                evento.FechaEvento
                                                                    .ToString(
                                                                        "dd/MM/yyyy"));

                                                            AgregarEtiquetaValor(
                                                                item,
                                                                "Estado del evento",
                                                                evento.Estado);
                                                        });
                                                });
                                        }));

                            column.Item()
                                .Element(container =>
                                    CrearSeccion(
                                        container,
                                        "PAQUETE SELECCIONADO",
                                        content =>
                                        {
                                            content.Item()
                                                .Row(row =>
                                                {
                                                    row.RelativeItem(2)
                                                        .Column(item =>
                                                        {
                                                            AgregarEtiquetaValor(
                                                                item,
                                                                "Nombre",
                                                                paquete.Nombre);

                                                            AgregarEtiquetaValor(
                                                                item,
                                                                "Descripción",
                                                                paquete.Descripcion ??
                                                                "Sin descripción");
                                                        });

                                                    row.RelativeItem()
                                                        .Column(item =>
                                                        {
                                                            AgregarEtiquetaValor(
                                                                item,
                                                                "Precio base",
                                                                FormatearMonto(
                                                                    paquete.PrecioBase));

                                                            AgregarEtiquetaValor(
                                                                item,
                                                                "Duración",
                                                                $"{paquete.DuracionHoras} horas");

                                                            AgregarEtiquetaValor(
                                                                item,
                                                                "Capacidad máxima",
                                                                $"{paquete.MaxPersonas} personas");
                                                        });
                                                });
                                        }));

                            column.Item()
                                .Column(serviciosColumn =>
                                {
                                    serviciosColumn.Spacing(8);

                                    serviciosColumn.Item()
                                        .Text("SERVICIOS ADICIONALES")
                                        .Bold()
                                        .FontSize(12)
                                        .FontColor(MoradoPrincipal);

                                    if (evento.EventoServicios.Any())
                                    {
                                        serviciosColumn.Item()
                                            .Table(table =>
                                            {
                                                table.ColumnsDefinition(columns =>
                                                {
                                                    columns.RelativeColumn(4);
                                                    columns.RelativeColumn(1);
                                                    columns.RelativeColumn(2);
                                                    columns.RelativeColumn(2);
                                                });

                                                table.Header(header =>
                                                {
                                                    header.Cell()
                                                        .Element(
                                                            EstiloEncabezadoTabla)
                                                        .Text("Servicio");

                                                    header.Cell()
                                                        .Element(
                                                            EstiloEncabezadoTabla)
                                                        .AlignCenter()
                                                        .Text("Cantidad");

                                                    header.Cell()
                                                        .Element(
                                                            EstiloEncabezadoTabla)
                                                        .AlignRight()
                                                        .Text("Precio unitario");

                                                    header.Cell()
                                                        .Element(
                                                            EstiloEncabezadoTabla)
                                                        .AlignRight()
                                                        .Text("Subtotal");
                                                });

                                                foreach (
                                                    var eventoServicio
                                                    in evento.EventoServicios)
                                                {
                                                    var subtotal =
                                                        eventoServicio.Cantidad *
                                                        eventoServicio
                                                            .PrecioAcordado;

                                                    table.Cell()
                                                        .Element(EstiloCeldaTabla)
                                                        .Text(
                                                            eventoServicio
                                                                .Servicio.Nombre);

                                                    table.Cell()
                                                        .Element(EstiloCeldaTabla)
                                                        .AlignCenter()
                                                        .Text(
                                                            eventoServicio
                                                                .Cantidad
                                                                .ToString());

                                                    table.Cell()
                                                        .Element(EstiloCeldaTabla)
                                                        .AlignRight()
                                                        .Text(
                                                            FormatearMonto(
                                                                eventoServicio
                                                                    .PrecioAcordado));

                                                    table.Cell()
                                                        .Element(EstiloCeldaTabla)
                                                        .AlignRight()
                                                        .Text(
                                                            FormatearMonto(
                                                                subtotal));
                                                }
                                            });
                                    }
                                    else
                                    {
                                        serviciosColumn.Item()
                                            .Background(FondoSeccion)
                                            .Border(1)
                                            .BorderColor(BordeClaro)
                                            .Padding(12)
                                            .Text(
                                                "No se agregaron servicios adicionales.")
                                            .FontColor(TextoGris);
                                    }
                                });

                            column.Item()
                                .AlignRight()
                                .Width(270)
                                .Background(MoradoClaro)
                                .Border(1)
                                .BorderColor(BordeClaro)
                                .Padding(16)
                                .Column(totalColumn =>
                                {
                                    totalColumn.Spacing(7);

                                    totalColumn.Item()
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("Precio del paquete")
                                                .FontColor(TextoGris);

                                            row.ConstantItem(110)
                                                .AlignRight()
                                                .Text(
                                                    FormatearMonto(
                                                        paquete.PrecioBase));
                                        });

                                    var totalServicios =
                                        evento.EventoServicios.Sum(
                                            servicio =>
                                                servicio.Cantidad *
                                                servicio.PrecioAcordado);

                                    totalColumn.Item()
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text(
                                                    "Servicios adicionales")
                                                .FontColor(TextoGris);

                                            row.ConstantItem(110)
                                                .AlignRight()
                                                .Text(
                                                    FormatearMonto(
                                                        totalServicios));
                                        });

                                    totalColumn.Item()
                                        .PaddingVertical(4)
                                        .LineHorizontal(1)
                                        .LineColor(MoradoPrincipal);

                                    totalColumn.Item()
                                        .Row(row =>
                                        {
                                            row.RelativeItem()
                                                .Text("TOTAL")
                                                .FontSize(15)
                                                .Bold()
                                                .FontColor(MoradoPrincipal);

                                            row.ConstantItem(130)
                                                .AlignRight()
                                                .Text(
                                                    FormatearMonto(
                                                        cotizacion.Total))
                                                .FontSize(15)
                                                .Bold()
                                                .FontColor(MoradoPrincipal);
                                        });
                                });

                            if (!string.IsNullOrWhiteSpace(evento.Notas))
                            {
                                column.Item()
                                    .Background(FondoSeccion)
                                    .BorderLeft(4)
                                    .BorderColor(MoradoPrincipal)
                                    .Padding(12)
                                    .Column(notas =>
                                    {
                                        notas.Item()
                                            .Text("Notas del evento")
                                            .Bold()
                                            .FontColor(MoradoPrincipal);

                                        notas.Item()
                                            .PaddingTop(4)
                                            .Text(evento.Notas)
                                            .FontColor(TextoGris);
                                    });
                            }

                            column.Item()
                                .PaddingTop(4)
                                .AlignCenter()
                                .Text(text =>
                                {
                                    text.Span(
                                            "Esta cotización es válida hasta el ")
                                        .FontColor(TextoGris);

                                    text.Span(
                                            cotizacion.FechaVencimiento.HasValue
                                                ? cotizacion
                                                    .FechaVencimiento.Value
                                                    .ToString("dd/MM/yyyy")
                                                : "día indicado por la administración")
                                        .Bold()
                                        .FontColor(MoradoPrincipal);

                                    text.Span(".");
                                });
                        });

                    page.Footer()
                        .PaddingHorizontal(35)
                        .PaddingVertical(15)
                        .BorderTop(1)
                        .BorderColor(BordeClaro)
                        .Row(row =>
                        {
                            row.RelativeItem()
                                .Column(column =>
                                {
                                    column.Item()
                                        .Text(
                                            "SIGESA · Sala de Eventos Mónica")
                                        .Bold()
                                        .FontSize(9)
                                        .FontColor(MoradoPrincipal);

                                    column.Item()
                                        .Text(
                                            "Gracias por confiar en nosotros.")
                                        .FontSize(8)
                                        .FontColor(TextoGris);
                                });

                            row.RelativeItem()
                                .AlignRight()
                                .AlignMiddle()
                                .Text(text =>
                                {
                                    text.Span("Página ")
                                        .FontSize(8)
                                        .FontColor(TextoGris);

                                    text.CurrentPageNumber()
                                        .FontSize(8)
                                        .FontColor(TextoGris);

                                    text.Span(" de ")
                                        .FontSize(8)
                                        .FontColor(TextoGris);

                                    text.TotalPages()
                                        .FontSize(8)
                                        .FontColor(TextoGris);
                                });
                        });
                });
            });

            return pdf.GeneratePdf();
        }

        private static void CrearSeccion(
            IContainer container,
            string titulo,
            Action<ColumnDescriptor> contenido)
        {
            container
                .Background(FondoSeccion)
                .Border(1)
                .BorderColor(BordeClaro)
                .Padding(15)
                .Column(column =>
                {
                    column.Spacing(10);

                    column.Item()
                        .Text(titulo)
                        .Bold()
                        .FontSize(12)
                        .FontColor(MoradoPrincipal);

                    column.Item()
                        .LineHorizontal(1)
                        .LineColor(BordeClaro);

                    contenido(column);
                });
        }

        private static void AgregarEtiquetaValor(
            ColumnDescriptor column,
            string etiqueta,
            string? valor)
        {
            column.Item()
                .PaddingBottom(8)
                .Column(item =>
                {
                    item.Item()
                        .Text(etiqueta)
                        .FontSize(8)
                        .FontColor(TextoGris);

                    item.Item()
                        .Text(
                            string.IsNullOrWhiteSpace(valor)
                                ? "No registrado"
                                : valor)
                        .FontSize(10)
                        .SemiBold()
                        .FontColor(TextoOscuro);
                });
        }

        private static IContainer EstiloEncabezadoTabla(
            IContainer container)
        {
            return container
                .Background(MoradoPrincipal)
                .PaddingVertical(8)
                .PaddingHorizontal(7)
                .DefaultTextStyle(style =>
                    style
                        .FontSize(9)
                        .Bold()
                        .FontColor(Colors.White));
        }

        private static IContainer EstiloCeldaTabla(
            IContainer container)
        {
            return container
                .BorderBottom(1)
                .BorderColor(BordeClaro)
                .PaddingVertical(8)
                .PaddingHorizontal(7)
                .DefaultTextStyle(style =>
                    style
                        .FontSize(9)
                        .FontColor(TextoOscuro));
        }

        private static string FormatearMonto(decimal monto)
        {
            return $"₡{monto:N2}";
        }
    }
}
