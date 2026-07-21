using Models.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace API.Services;

public class ContratoPdfService : IContratoPdfService
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

    public byte[] GenerarPdf(Contrato contrato)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var cotizacion = contrato.Cotizacion
            ?? throw new InvalidOperationException(
                "El contrato no tiene una cotización asociada.");

        var evento = cotizacion.Evento
            ?? throw new InvalidOperationException(
                "La cotización no tiene un evento asociado.");

        var cliente = evento.Cliente
            ?? throw new InvalidOperationException(
                "El evento no tiene un cliente asociado.");

        var paquete = evento.Paquete
            ?? throw new InvalidOperationException(
                "El evento no tiene un paquete asociado.");

        var totalServicios = evento.EventoServicios?
            .Sum(es => es.Cantidad * es.PrecioAcordado) ?? 0;

        var saldoPendiente =
            evento.SaldoPendiente
            ?? Math.Max(0, cotizacion.Total - evento.MontoPagado);

        var pdf = Document.Create(document =>
        {
            document.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(0);

                page.DefaultTextStyle(style =>
                    style
                        .FontSize(10)
                        .FontColor(TextoOscuro));

                CrearEncabezado(page, contrato);

                page.Content()
                    .PaddingHorizontal(35)
                    .PaddingVertical(24)
                    .Column(column =>
                    {
                        column.Spacing(15);

                        CrearInformacionGeneral(
                            column,
                            contrato,
                            evento);

                        column.Item()
                            .Element(container =>
                                CrearSeccion(
                                    container,
                                    "PARTES DEL CONTRATO",
                                    content =>
                                    {
                                        content.Item()
                                            .Text(text =>
                                            {
                                                text.Span(
                                                        "Entre ")
                                                    .FontColor(TextoOscuro);

                                                text.Span(
                                                        "Sala de Eventos Mónica")
                                                    .Bold()
                                                    .FontColor(MoradoPrincipal);

                                                text.Span(
                                                    ", representada para efectos de este documento por la administración de SIGESA, en adelante denominada ");

                                                text.Span(
                                                        "“LA EMPRESA”")
                                                    .Bold();

                                                text.Span(
                                                    ", y ");

                                                text.Span(cliente.Nombre)
                                                    .Bold()
                                                    .FontColor(MoradoPrincipal);

                                                text.Span(
                                                    $", portador(a) del documento de identidad {cliente.Documento}, en adelante denominado(a) ");

                                                text.Span(
                                                        "“EL CLIENTE”")
                                                    .Bold();

                                                text.Span(
                                                    ", se celebra el presente contrato de prestación de servicios para eventos.");
                                            });
                                    }));

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
                                                            "Nombre completo",
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
                                                            cliente.Email
                                                            ?? "No registrado");
                                                    });
                                            });
                                    }));

                        column.Item()
                            .Element(container =>
                                CrearSeccion(
                                    container,
                                    "OBJETO DEL CONTRATO",
                                    content =>
                                    {
                                        content.Item()
                                            .Text(
                                                $"LA EMPRESA se compromete a brindar a EL CLIENTE los servicios necesarios para la realización de un evento de tipo “{evento.TipoEvento}”, de acuerdo con las condiciones, paquete y servicios adicionales detallados en este contrato.")
                                            .LineHeight(1.4f);
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
                                                            "Fecha del evento",
                                                            evento.FechaEvento
                                                                .ToString(
                                                                    "dd/MM/yyyy"));

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
                                                            "Paquete contratado",
                                                            paquete.Nombre);

                                                        AgregarEtiquetaValor(
                                                            item,
                                                            "Duración",
                                                            $"{paquete.DuracionHoras} horas");

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
                                    "PAQUETE CONTRATADO",
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
                                                            paquete.Descripcion
                                                            ?? "Sin descripción");
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
                                                            "Capacidad máxima",
                                                            $"{paquete.MaxPersonas} personas");
                                                    });
                                            });
                                    }));

                        CrearServiciosAdicionales(
                            column,
                            evento);

                        CrearResumenEconomico(
                            column,
                            cotizacion,
                            evento,
                            paquete,
                            totalServicios,
                            saldoPendiente);

                        column.Item()
                            .PageBreak();

                        column.Item()
                            .Text("CLÁUSULAS DEL CONTRATO")
                            .FontSize(16)
                            .Bold()
                            .FontColor(MoradoPrincipal);

                        CrearClausula(
                            column,
                            "PRIMERA. OBJETO",
                            $"LA EMPRESA brindará los servicios relacionados con el evento de tipo {evento.TipoEvento}, programado para el {evento.FechaEvento:dd/MM/yyyy}, conforme con el paquete y servicios seleccionados.");

                        CrearClausula(
                            column,
                            "SEGUNDA. PRECIO Y FORMA DE PAGO",
                            $"El precio total acordado es de {FormatearMonto(cotizacion.Total)}. EL CLIENTE ha pagado {FormatearMonto(evento.MontoPagado)} y mantiene un saldo pendiente de {FormatearMonto(saldoPendiente)}. Los pagos deberán realizarse mediante los métodos autorizados por LA EMPRESA.");

                        CrearClausula(
                            column,
                            "TERCERA. RESERVACIÓN DE LA FECHA",
                            "La fecha del evento quedará reservada conforme con las políticas de pago establecidas por LA EMPRESA. El incumplimiento de los pagos acordados podrá provocar la cancelación de la reservación.");

                        CrearClausula(
                            column,
                            "CUARTA. OBLIGACIONES DE LA EMPRESA",
                            "LA EMPRESA deberá brindar las instalaciones, el paquete contratado y los servicios adicionales incluidos en el presente documento, procurando que se encuentren disponibles y en condiciones adecuadas para la realización del evento.");

                        CrearClausula(
                            column,
                            "QUINTA. OBLIGACIONES DEL CLIENTE",
                            "EL CLIENTE deberá proporcionar información verdadera, respetar la capacidad máxima contratada, cumplir con las normas del establecimiento y responder por cualquier daño provocado por él o por sus invitados.");

                        CrearClausula(
                            column,
                            "SEXTA. CAMBIOS EN EL EVENTO",
                            "Cualquier cambio de fecha, cantidad de personas, paquete o servicios deberá ser solicitado con anticipación y quedará sujeto a disponibilidad y a posibles ajustes en el monto total.");

                        CrearClausula(
                            column,
                            "SÉTIMA. CANCELACIÓN",
                            "En caso de cancelación por parte de EL CLIENTE, la devolución de los montos pagados estará sujeta a las políticas administrativas vigentes y a los gastos en que haya incurrido LA EMPRESA.");

                        CrearClausula(
                            column,
                            "OCTAVA. CASO FORTUITO O FUERZA MAYOR",
                            "Ninguna de las partes será responsable por incumplimientos provocados por situaciones fuera de su control razonable. En estos casos, ambas partes procurarán acordar una nueva fecha o una solución equivalente.");

                        CrearClausula(
                            column,
                            "NOVENA. ACEPTACIÓN",
                            "Las partes manifiestan que han leído y comprendido el contenido del presente contrato y que aceptan las condiciones aquí establecidas.");

                        if (!string.IsNullOrWhiteSpace(
                                contrato.Observaciones))
                        {
                            column.Item()
                                .Element(container =>
                                    CrearSeccion(
                                        container,
                                        "OBSERVACIONES",
                                        content =>
                                        {
                                            content.Item()
                                                .Text(
                                                    contrato.Observaciones)
                                                .LineHeight(1.4f);
                                        }));
                        }

                        if (!string.IsNullOrWhiteSpace(evento.Notas))
                        {
                            column.Item()
                                .Element(container =>
                                    CrearSeccion(
                                        container,
                                        "NOTAS DEL EVENTO",
                                        content =>
                                        {
                                            content.Item()
                                                .Text(evento.Notas)
                                                .LineHeight(1.4f);
                                        }));
                        }

                        CrearFirmas(column, cliente);
                    });

                CrearPiePagina(page);
            });
        });

        return pdf.GeneratePdf();
    }

    private static void CrearEncabezado(
        PageDescriptor page,
        Contrato contrato)
    {
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

                row.ConstantItem(220)
                    .AlignRight()
                    .AlignMiddle()
                    .Column(column =>
                    {
                        column.Item()
                            .AlignRight()
                            .Text("CONTRATO DE SERVICIOS")
                            .FontSize(18)
                            .Bold()
                            .FontColor(Colors.White);

                        column.Item()
                            .AlignRight()
                            .Text(contrato.NumeroContrato)
                            .FontSize(10)
                            .FontColor("#DDD9FF");
                    });
            });
    }

    private static void CrearInformacionGeneral(
        ColumnDescriptor column,
        Contrato contrato,
        Evento evento)
    {
        column.Item()
            .Row(row =>
            {
                row.RelativeItem()
                    .Column(item =>
                    {
                        item.Item()
                            .Text("Fecha de emisión")
                            .FontSize(9)
                            .FontColor(TextoGris);

                        item.Item()
                            .Text(
                                contrato.FechaContrato
                                    .ToString("dd/MM/yyyy"))
                            .Bold();
                    });

                row.RelativeItem()
                    .Column(item =>
                    {
                        item.Item()
                            .Text("Fecha del evento")
                            .FontSize(9)
                            .FontColor(TextoGris);

                        item.Item()
                            .Text(
                                evento.FechaEvento
                                    .ToString("dd/MM/yyyy"))
                            .Bold();
                    });

                row.RelativeItem()
                    .AlignRight()
                    .Column(item =>
                    {
                        item.Item()
                            .AlignRight()
                            .Text("Estado")
                            .FontSize(9)
                            .FontColor(TextoGris);

                        item.Item()
                            .AlignRight()
                            .Background(VerdeClaro)
                            .PaddingHorizontal(10)
                            .PaddingVertical(4)
                            .Text(contrato.Estado)
                            .Bold()
                            .FontSize(9)
                            .FontColor(Verde);
                    });
            });
    }

    private static void CrearServiciosAdicionales(
        ColumnDescriptor column,
        Evento evento)
    {
        column.Item()
            .Column(serviciosColumn =>
            {
                serviciosColumn.Spacing(8);

                serviciosColumn.Item()
                    .Text("SERVICIOS ADICIONALES")
                    .Bold()
                    .FontSize(12)
                    .FontColor(MoradoPrincipal);

                if (evento.EventoServicios != null
                    && evento.EventoServicios.Any())
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
                                    .Element(EstiloEncabezadoTabla)
                                    .Text("Servicio");

                                header.Cell()
                                    .Element(EstiloEncabezadoTabla)
                                    .AlignCenter()
                                    .Text("Cantidad");

                                header.Cell()
                                    .Element(EstiloEncabezadoTabla)
                                    .AlignRight()
                                    .Text("Precio unitario");

                                header.Cell()
                                    .Element(EstiloEncabezadoTabla)
                                    .AlignRight()
                                    .Text("Subtotal");
                            });

                            foreach (var servicio
                                     in evento.EventoServicios)
                            {
                                var subtotal =
                                    servicio.Cantidad
                                    * servicio.PrecioAcordado;

                                table.Cell()
                                    .Element(EstiloCeldaTabla)
                                    .Text(
                                        servicio.Servicio?.Nombre
                                        ?? "Servicio");

                                table.Cell()
                                    .Element(EstiloCeldaTabla)
                                    .AlignCenter()
                                    .Text(
                                        servicio.Cantidad.ToString());

                                table.Cell()
                                    .Element(EstiloCeldaTabla)
                                    .AlignRight()
                                    .Text(
                                        FormatearMonto(
                                            servicio.PrecioAcordado));

                                table.Cell()
                                    .Element(EstiloCeldaTabla)
                                    .AlignRight()
                                    .Text(
                                        FormatearMonto(subtotal));
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
    }

    private static void CrearResumenEconomico(
        ColumnDescriptor column,
        Cotizacion cotizacion,
        Evento evento,
        Paquete paquete,
        decimal totalServicios,
        decimal saldoPendiente)
    {
        column.Item()
            .AlignRight()
            .Width(300)
            .Background(MoradoClaro)
            .Border(1)
            .BorderColor(BordeClaro)
            .Padding(16)
            .Column(totalColumn =>
            {
                totalColumn.Spacing(7);

                AgregarFilaMonto(
                    totalColumn,
                    "Precio del paquete",
                    paquete.PrecioBase);

                AgregarFilaMonto(
                    totalColumn,
                    "Servicios adicionales",
                    totalServicios);

                AgregarFilaMonto(
                    totalColumn,
                    "Monto pagado",
                    evento.MontoPagado);

                totalColumn.Item()
                    .PaddingVertical(3)
                    .LineHorizontal(1)
                    .LineColor(MoradoPrincipal);

                totalColumn.Item()
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text("TOTAL")
                            .FontSize(14)
                            .Bold()
                            .FontColor(MoradoPrincipal);

                        row.ConstantItem(130)
                            .AlignRight()
                            .Text(
                                FormatearMonto(
                                    cotizacion.Total))
                            .FontSize(14)
                            .Bold()
                            .FontColor(MoradoPrincipal);
                    });

                totalColumn.Item()
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text("SALDO PENDIENTE")
                            .FontSize(11)
                            .Bold()
                            .FontColor(TextoOscuro);

                        row.ConstantItem(130)
                            .AlignRight()
                            .Text(
                                FormatearMonto(
                                    saldoPendiente))
                            .FontSize(11)
                            .Bold()
                            .FontColor(TextoOscuro);
                    });
            });
    }

    private static void CrearClausula(
        ColumnDescriptor column,
        string titulo,
        string contenido)
    {
        column.Item()
            .Background(FondoSeccion)
            .BorderLeft(4)
            .BorderColor(MoradoPrincipal)
            .Padding(13)
            .Column(item =>
            {
                item.Spacing(5);

                item.Item()
                    .Text(titulo)
                    .Bold()
                    .FontSize(10)
                    .FontColor(MoradoPrincipal);

                item.Item()
                    .Text(contenido)
                    .FontSize(9.5f)
                    .LineHeight(1.4f)
                    .FontColor(TextoOscuro);
            });
    }

    private static void CrearFirmas(
        ColumnDescriptor column,
        Cliente cliente)
    {
        column.Item()
            .PaddingTop(35)
            .Row(row =>
            {
                row.RelativeItem()
                    .PaddingRight(15)
                    .Column(item =>
                    {
                        item.Item()
                            .Height(45);

                        item.Item()
                            .LineHorizontal(1)
                            .LineColor(TextoOscuro);

                        item.Item()
                            .PaddingTop(5)
                            .AlignCenter()
                            .Text("LA EMPRESA")
                            .Bold()
                            .FontSize(9);

                        item.Item()
                            .AlignCenter()
                            .Text("Sala de Eventos Mónica")
                            .FontSize(8)
                            .FontColor(TextoGris);
                    });

                row.RelativeItem()
                    .PaddingLeft(15)
                    .Column(item =>
                    {
                        item.Item()
                            .Height(45);

                        item.Item()
                            .LineHorizontal(1)
                            .LineColor(TextoOscuro);

                        item.Item()
                            .PaddingTop(5)
                            .AlignCenter()
                            .Text("EL CLIENTE")
                            .Bold()
                            .FontSize(9);

                        item.Item()
                            .AlignCenter()
                            .Text(cliente.Nombre)
                            .FontSize(8)
                            .FontColor(TextoGris);

                        item.Item()
                            .AlignCenter()
                            .Text($"Documento: {cliente.Documento}")
                            .FontSize(8)
                            .FontColor(TextoGris);
                    });
            });
    }

    private static void CrearPiePagina(
        PageDescriptor page)
    {
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
                                "Contrato de prestación de servicios para eventos.")
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

    private static void AgregarFilaMonto(
        ColumnDescriptor column,
        string etiqueta,
        decimal monto)
    {
        column.Item()
            .Row(row =>
            {
                row.RelativeItem()
                    .Text(etiqueta)
                    .FontColor(TextoGris);

                row.ConstantItem(130)
                    .AlignRight()
                    .Text(FormatearMonto(monto));
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
