using System.Net;
using System.Net.Mail;

namespace API.Services;

public interface IEmailService
{
    Task EnviarRecuperacionAsync(
        string destinatario,
        string nombre,
        string token);

    Task EnviarConfirmacionReservaAsync(
        string destinatario,
        string nombreCliente,
        string tipoEvento,
        DateOnly fechaEvento,
        int numPersonas,
        decimal montoTotal);

    Task EnviarCotizacionAsync(
        string destinatario,
        string nombreCliente,
        int numeroCotizacion,
        string tipoEvento,
        DateOnly fechaEvento,
        decimal montoTotal,
        DateTime? fechaVencimiento,
        byte[] archivoPdf);

    Task EnviarContratoAsync(
        string destinatario,
        string nombreCliente,
        string numeroContrato,
        string tipoEvento,
        DateOnly fechaEvento,
        decimal montoTotal,
        byte[] archivoPdf);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task EnviarRecuperacionAsync(
        string destinatario,
        string nombre,
        string token)
    {
        var smtp = _config["Email:Smtp"] ?? "smtp.gmail.com";
        var puerto = int.Parse(_config["Email:Puerto"] ?? "587");

        var usuario = _config["Email:Usuario"]
            ?? throw new InvalidOperationException(
                "Email:Usuario no configurado.");

        var clave = _config["Email:Clave"]
            ?? throw new InvalidOperationException(
                "Email:Clave no configurada.");

        var urlBase = _config["Email:UrlBase"]
            ?? "http://localhost:5003";

        var enlace =
            $"{urlBase}/Auth/NuevaContrasena" +
            $"?token={Uri.EscapeDataString(token)}";

        using var mensaje = new MailMessage
        {
            From = new MailAddress(
                usuario,
                "SIGESA — Sala de Eventos Mónica"),

            Subject = "Recuperación de contraseña — SIGESA",

            IsBodyHtml = true,

            Body = $"""
                <!DOCTYPE html>
                <html lang="es">
                <body style="margin:0;padding:0;background:#f5f4ef;font-family:'Inter',Arial,sans-serif;">

                  <div style="max-width:480px;margin:40px auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.08);">

                    <div style="background:linear-gradient(135deg,#534AB7,#3d3490);padding:32px;text-align:center;">

                      <h1 style="color:#fff;margin:0;font-size:1.5rem;">
                        🎪 SIGESA
                      </h1>

                      <p style="color:rgba(255,255,255,.7);margin:4px 0 0;font-size:.875rem;">
                        Sala de Eventos Mónica
                      </p>

                    </div>

                    <div style="padding:32px;">

                      <h2 style="color:#1a1a1a;margin:0 0 8px;font-size:1.1rem;">
                        Hola, {nombre}
                      </h2>

                      <p style="color:#555;margin:0 0 24px;line-height:1.6;">
                        Recibimos una solicitud para restablecer la contraseña
                        de tu cuenta. Haz clic en el botón para continuar.
                        Este enlace expira en <strong>30 minutos</strong>.
                      </p>

                      <div style="text-align:center;margin-bottom:24px;">

                        <a href="{enlace}"
                           style="display:inline-block;background:#534AB7;color:#fff;text-decoration:none;padding:14px 32px;border-radius:10px;font-weight:600;font-size:.95rem;">

                          Restablecer contraseña

                        </a>

                      </div>

                      <p style="color:#999;font-size:.8rem;margin:0;line-height:1.6;">
                        Si no solicitaste este cambio, puedes ignorar este correo.
                        Tu contraseña actual seguirá siendo la misma.
                      </p>

                    </div>

                    <div style="background:#f8f7ff;padding:16px;text-align:center;">

                      <p style="color:#aaa;font-size:.75rem;margin:0;">
                        SIGESA v1.0 © 2026
                      </p>

                    </div>

                  </div>

                </body>
                </html>
                """
        };

        mensaje.To.Add(destinatario);

        using var client = CrearClienteSmtp(
            smtp,
            puerto,
            usuario,
            clave);

        await client.SendMailAsync(mensaje);
    }

    public async Task EnviarConfirmacionReservaAsync(
        string destinatario,
        string nombreCliente,
        string tipoEvento,
        DateOnly fechaEvento,
        int numPersonas,
        decimal montoTotal)
    {
        var smtp = _config["Email:Smtp"] ?? "smtp.gmail.com";
        var puerto = int.Parse(_config["Email:Puerto"] ?? "587");

        var usuario = _config["Email:Usuario"]
            ?? throw new InvalidOperationException(
                "Email:Usuario no configurado.");

        var clave = _config["Email:Clave"]
            ?? throw new InvalidOperationException(
                "Email:Clave no configurada.");

        using var mensaje = new MailMessage
        {
            From = new MailAddress(
                usuario,
                "SIGESA — Sala de Eventos Mónica"),

            Subject = "Confirmación de tu reserva — SIGESA",

            IsBodyHtml = true,

            Body = $"""
                <!DOCTYPE html>
                <html lang="es">
                <body style="margin:0;padding:0;background:#f5f4ef;font-family:'Inter',Arial,sans-serif;">

                  <div style="max-width:480px;margin:40px auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.08);">

                    <div style="background:linear-gradient(135deg,#534AB7,#3d3490);padding:32px;text-align:center;">

                      <h1 style="color:#fff;margin:0;font-size:1.5rem;">
                        🎪 SIGESA
                      </h1>

                      <p style="color:rgba(255,255,255,.7);margin:4px 0 0;font-size:.875rem;">
                        Sala de Eventos Mónica
                      </p>

                    </div>

                    <div style="padding:32px;">

                      <h2 style="color:#1a1a1a;margin:0 0 8px;font-size:1.1rem;">
                        Hola, {nombreCliente}
                      </h2>

                      <p style="color:#555;margin:0 0 24px;line-height:1.6;">
                        Tu reserva para <strong>{tipoEvento}</strong>
                        ha sido <strong>confirmada</strong>.
                        Aquí tienes el resumen:
                      </p>

                      <table style="width:100%;border-collapse:collapse;margin-bottom:24px;">

                        <tr>
                          <td style="padding:8px 0;color:#999;font-size:.85rem;">
                            Fecha del evento
                          </td>

                          <td style="padding:8px 0;color:#1a1a1a;font-size:.85rem;text-align:right;">
                            {fechaEvento:dd/MM/yyyy}
                          </td>
                        </tr>

                        <tr>
                          <td style="padding:8px 0;color:#999;font-size:.85rem;">
                            Número de personas
                          </td>

                          <td style="padding:8px 0;color:#1a1a1a;font-size:.85rem;text-align:right;">
                            {numPersonas}
                          </td>
                        </tr>

                        <tr>
                          <td style="padding:8px 0;color:#999;font-size:.85rem;">
                            Monto total
                          </td>

                          <td style="padding:8px 0;color:#1a1a1a;font-size:.85rem;text-align:right;">
                            ₡{montoTotal:N2}
                          </td>
                        </tr>

                      </table>

                      <p style="color:#999;font-size:.8rem;margin:0;line-height:1.6;">
                        Si tienes alguna duda sobre tu reserva,
                        contáctanos respondiendo a este correo.
                      </p>

                    </div>

                    <div style="background:#f8f7ff;padding:16px;text-align:center;">

                      <p style="color:#aaa;font-size:.75rem;margin:0;">
                        SIGESA v1.0 © 2026
                      </p>

                    </div>

                  </div>

                </body>
                </html>
                """
        };

        mensaje.To.Add(destinatario);

        using var client = CrearClienteSmtp(
            smtp,
            puerto,
            usuario,
            clave);

        await client.SendMailAsync(mensaje);
    }

    public async Task EnviarCotizacionAsync(
        string destinatario,
        string nombreCliente,
        int numeroCotizacion,
        string tipoEvento,
        DateOnly fechaEvento,
        decimal montoTotal,
        DateTime? fechaVencimiento,
        byte[] archivoPdf)
    {
        if (string.IsNullOrWhiteSpace(destinatario))
        {
            throw new ArgumentException(
                "El correo del destinatario es obligatorio.",
                nameof(destinatario));
        }

        if (archivoPdf == null || archivoPdf.Length == 0)
        {
            throw new ArgumentException(
                "El archivo PDF de la cotización está vacío.",
                nameof(archivoPdf));
        }

        var smtp = _config["Email:Smtp"] ?? "smtp.gmail.com";
        var puerto = int.Parse(_config["Email:Puerto"] ?? "587");

        var usuario = _config["Email:Usuario"]
            ?? throw new InvalidOperationException(
                "Email:Usuario no configurado.");

        var clave = _config["Email:Clave"]
            ?? throw new InvalidOperationException(
                "Email:Clave no configurada.");

        var vencimientoTexto = fechaVencimiento.HasValue
            ? fechaVencimiento.Value.ToString("dd/MM/yyyy")
            : "No definido";

        using var mensaje = new MailMessage
        {
            From = new MailAddress(
                usuario,
                "SIGESA — Sala de Eventos Mónica"),

            Subject =
                $"Cotización #{numeroCotizacion} — Sala de Eventos Mónica",

            IsBodyHtml = true,

            Body = $"""
                <!DOCTYPE html>
                <html lang="es">
                <body style="margin:0;padding:0;background:#f5f4ef;font-family:'Inter',Arial,sans-serif;">

                  <div style="max-width:520px;margin:40px auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.08);">

                    <div style="background:linear-gradient(135deg,#534AB7,#3d3490);padding:32px;text-align:center;">

                      <h1 style="color:#fff;margin:0;font-size:1.5rem;">
                        🎪 SIGESA
                      </h1>

                      <p style="color:rgba(255,255,255,.75);margin:4px 0 0;font-size:.875rem;">
                        Sala de Eventos Mónica
                      </p>

                    </div>

                    <div style="padding:32px;">

                      <h2 style="color:#1a1a1a;margin:0 0 10px;font-size:1.15rem;">
                        Hola, {nombreCliente}
                      </h2>

                      <p style="color:#555;margin:0 0 24px;line-height:1.6;">
                        Hemos preparado la cotización
                        <strong>#{numeroCotizacion}</strong>
                        para tu evento. Encontrarás el documento completo
                        adjunto en formato PDF.
                      </p>

                      <div style="background:#f8f7ff;border:1px solid #ece9ff;border-radius:12px;padding:18px;margin-bottom:24px;">

                        <table style="width:100%;border-collapse:collapse;">

                          <tr>
                            <td style="padding:7px 0;color:#888;font-size:.85rem;">
                              Tipo de evento
                            </td>

                            <td style="padding:7px 0;color:#1a1a1a;font-size:.85rem;text-align:right;font-weight:600;">
                              {tipoEvento}
                            </td>
                          </tr>

                          <tr>
                            <td style="padding:7px 0;color:#888;font-size:.85rem;">
                              Fecha del evento
                            </td>

                            <td style="padding:7px 0;color:#1a1a1a;font-size:.85rem;text-align:right;font-weight:600;">
                              {fechaEvento:dd/MM/yyyy}
                            </td>
                          </tr>

                          <tr>
                            <td style="padding:7px 0;color:#888;font-size:.85rem;">
                              Válida hasta
                            </td>

                            <td style="padding:7px 0;color:#1a1a1a;font-size:.85rem;text-align:right;font-weight:600;">
                              {vencimientoTexto}
                            </td>
                          </tr>

                          <tr>
                            <td style="padding:12px 0 0;color:#534AB7;font-size:.95rem;font-weight:700;border-top:1px solid #ddd9ff;">
                              Total
                            </td>

                            <td style="padding:12px 0 0;color:#534AB7;font-size:1rem;text-align:right;font-weight:700;border-top:1px solid #ddd9ff;">
                              ₡{montoTotal:N2}
                            </td>
                          </tr>

                        </table>

                      </div>

                      <p style="color:#555;margin:0 0 12px;line-height:1.6;">
                        📎 El PDF con todos los detalles está adjunto a este correo.
                      </p>

                      <p style="color:#999;font-size:.8rem;margin:0;line-height:1.6;">
                        Si tienes alguna consulta o deseas realizar un cambio,
                        puedes responder directamente a este correo.
                      </p>

                    </div>

                    <div style="background:#f8f7ff;padding:16px;text-align:center;">

                      <p style="color:#aaa;font-size:.75rem;margin:0;">
                        SIGESA v1.0 © 2026
                      </p>

                    </div>

                  </div>

                </body>
                </html>
                """
        };

        mensaje.To.Add(destinatario);

        var flujoPdf = new MemoryStream(archivoPdf);

        var adjunto = new Attachment(
            flujoPdf,
            $"Cotizacion-{numeroCotizacion}.pdf",
            "application/pdf");

        mensaje.Attachments.Add(adjunto);

        using var client = CrearClienteSmtp(
            smtp,
            puerto,
            usuario,
            clave);

        await client.SendMailAsync(mensaje);
    }

    public async Task EnviarContratoAsync(
        string destinatario,
        string nombreCliente,
        string numeroContrato,
        string tipoEvento,
        DateOnly fechaEvento,
        decimal montoTotal,
        byte[] archivoPdf)
    {
        if (string.IsNullOrWhiteSpace(destinatario))
        {
            throw new ArgumentException(
                "El correo del destinatario es obligatorio.",
                nameof(destinatario));
        }

        if (string.IsNullOrWhiteSpace(numeroContrato))
        {
            throw new ArgumentException(
                "El número del contrato es obligatorio.",
                nameof(numeroContrato));
        }

        if (archivoPdf == null || archivoPdf.Length == 0)
        {
            throw new ArgumentException(
                "El archivo PDF del contrato está vacío.",
                nameof(archivoPdf));
        }

        var smtp = _config["Email:Smtp"] ?? "smtp.gmail.com";
        var puerto = int.Parse(_config["Email:Puerto"] ?? "587");

        var usuario = _config["Email:Usuario"]
            ?? throw new InvalidOperationException(
                "Email:Usuario no configurado.");

        var clave = _config["Email:Clave"]
            ?? throw new InvalidOperationException(
                "Email:Clave no configurada.");

        using var mensaje = new MailMessage
        {
            From = new MailAddress(
                usuario,
                "SIGESA — Sala de Eventos Mónica"),

            Subject =
                $"Contrato {numeroContrato} — Sala de Eventos Mónica",

            IsBodyHtml = true,

            Body = $"""
                <!DOCTYPE html>
                <html lang="es">
                <body style="margin:0;padding:0;background:#f5f4ef;font-family:'Inter',Arial,sans-serif;">

                  <div style="max-width:520px;margin:40px auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.08);">

                    <div style="background:linear-gradient(135deg,#534AB7,#3d3490);padding:32px;text-align:center;">

                      <h1 style="color:#fff;margin:0;font-size:1.5rem;">
                        🎪 SIGESA
                      </h1>

                      <p style="color:rgba(255,255,255,.75);margin:4px 0 0;font-size:.875rem;">
                        Sala de Eventos Mónica
                      </p>

                    </div>

                    <div style="padding:32px;">

                      <h2 style="color:#1a1a1a;margin:0 0 10px;font-size:1.15rem;">
                        Hola, {nombreCliente}
                      </h2>

                      <p style="color:#555;margin:0 0 24px;line-height:1.6;">
                        Hemos preparado el contrato
                        <strong>{numeroContrato}</strong>
                        correspondiente a tu evento.
                        Encontrarás el documento completo adjunto en formato PDF.
                      </p>

                      <div style="background:#f8f7ff;border:1px solid #ece9ff;border-radius:12px;padding:18px;margin-bottom:24px;">

                        <table style="width:100%;border-collapse:collapse;">

                          <tr>
                            <td style="padding:7px 0;color:#888;font-size:.85rem;">
                              Número de contrato
                            </td>

                            <td style="padding:7px 0;color:#1a1a1a;font-size:.85rem;text-align:right;font-weight:600;">
                              {numeroContrato}
                            </td>
                          </tr>

                          <tr>
                            <td style="padding:7px 0;color:#888;font-size:.85rem;">
                              Tipo de evento
                            </td>

                            <td style="padding:7px 0;color:#1a1a1a;font-size:.85rem;text-align:right;font-weight:600;">
                              {tipoEvento}
                            </td>
                          </tr>

                          <tr>
                            <td style="padding:7px 0;color:#888;font-size:.85rem;">
                              Fecha del evento
                            </td>

                            <td style="padding:7px 0;color:#1a1a1a;font-size:.85rem;text-align:right;font-weight:600;">
                              {fechaEvento:dd/MM/yyyy}
                            </td>
                          </tr>

                          <tr>
                            <td style="padding:12px 0 0;color:#534AB7;font-size:.95rem;font-weight:700;border-top:1px solid #ddd9ff;">
                              Total contratado
                            </td>

                            <td style="padding:12px 0 0;color:#534AB7;font-size:1rem;text-align:right;font-weight:700;border-top:1px solid #ddd9ff;">
                              ₡{montoTotal:N2}
                            </td>
                          </tr>

                        </table>

                      </div>

                      <p style="color:#555;margin:0 0 12px;line-height:1.6;">
                        📎 El contrato completo está adjunto
                        a este correo en formato PDF.
                      </p>

                      <p style="color:#999;font-size:.8rem;margin:0;line-height:1.6;">
                        Te recomendamos revisar cuidadosamente la información
                        y las condiciones del contrato. Si tienes alguna consulta,
                        puedes responder directamente a este correo.
                      </p>

                    </div>

                    <div style="background:#f8f7ff;padding:16px;text-align:center;">

                      <p style="color:#aaa;font-size:.75rem;margin:0;">
                        SIGESA v1.0 © 2026
                      </p>

                    </div>

                  </div>

                </body>
                </html>
                """
        };

        mensaje.To.Add(destinatario);

        var flujoPdf = new MemoryStream(archivoPdf);

        var adjunto = new Attachment(
            flujoPdf,
            $"Contrato-{numeroContrato}.pdf",
            "application/pdf");

        mensaje.Attachments.Add(adjunto);

        using var client = CrearClienteSmtp(
            smtp,
            puerto,
            usuario,
            clave);

        await client.SendMailAsync(mensaje);
    }

    private static SmtpClient CrearClienteSmtp(
        string smtp,
        int puerto,
        string usuario,
        string clave)
    {
        return new SmtpClient(smtp, puerto)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(
                usuario,
                clave),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };
    }
}
