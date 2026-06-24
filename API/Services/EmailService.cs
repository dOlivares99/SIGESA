using System.Net;
using System.Net.Mail;

namespace API.Services;

public interface IEmailService
{
    Task EnviarRecuperacionAsync(string destinatario, string nombre, string token);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;

    public EmailService(IConfiguration config)
    {
        _config = config;
    }

    public async Task EnviarRecuperacionAsync(string destinatario, string nombre, string token)
    {
        var smtp = _config["Email:Smtp"] ?? "smtp.gmail.com";
        var puerto = int.Parse(_config["Email:Puerto"] ?? "587");
        var usuario = _config["Email:Usuario"] ?? throw new InvalidOperationException("Email:Usuario no configurado.");
        var clave = _config["Email:Clave"] ?? throw new InvalidOperationException("Email:Clave no configurada.");
        var urlBase = _config["Email:UrlBase"] ?? "http://localhost:5003";

        var enlace = $"{urlBase}/Auth/NuevaContrasena?token={Uri.EscapeDataString(token)}";

        var mensaje = new MailMessage
        {
            From = new MailAddress(usuario, "SIGESA — Sala de Eventos Mónica"),
            Subject = "Recuperación de contraseña — SIGESA",
            IsBodyHtml = true,
            Body = $"""
                <!DOCTYPE html>
                <html lang="es">
                <body style="margin:0;padding:0;background:#f5f4ef;font-family:'Inter',Arial,sans-serif;">
                  <div style="max-width:480px;margin:40px auto;background:#fff;border-radius:16px;overflow:hidden;box-shadow:0 4px 20px rgba(0,0,0,.08);">
                    <div style="background:linear-gradient(135deg,#534AB7,#3d3490);padding:32px;text-align:center;">
                      <h1 style="color:#fff;margin:0;font-size:1.5rem;">🎪 SIGESA</h1>
                      <p style="color:rgba(255,255,255,.7);margin:4px 0 0;font-size:.875rem;">Sala de Eventos Mónica</p>
                    </div>
                    <div style="padding:32px;">
                      <h2 style="color:#1a1a1a;margin:0 0 8px;font-size:1.1rem;">Hola, {nombre}</h2>
                      <p style="color:#555;margin:0 0 24px;line-height:1.6;">
                        Recibimos una solicitud para restablecer la contraseña de tu cuenta.
                        Haz clic en el botón para continuar. Este enlace expira en <strong>30 minutos</strong>.
                      </p>
                      <div style="text-align:center;margin-bottom:24px;">
                        <a href="{enlace}"
                           style="display:inline-block;background:#534AB7;color:#fff;text-decoration:none;
                                  padding:14px 32px;border-radius:10px;font-weight:600;font-size:.95rem;">
                          Restablecer contraseña
                        </a>
                      </div>
                      <p style="color:#999;font-size:.8rem;margin:0;line-height:1.6;">
                        Si no solicitaste este cambio, puedes ignorar este correo.
                        Tu contraseña actual seguirá siendo la misma.
                      </p>
                    </div>
                    <div style="background:#f8f7ff;padding:16px;text-align:center;">
                      <p style="color:#aaa;font-size:.75rem;margin:0;">SIGESA v1.0 © 2026</p>
                    </div>
                  </div>
                </body>
                </html>
                """
        };

        mensaje.To.Add(destinatario);

        using var client = new SmtpClient(smtp, puerto)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(usuario, clave),
            DeliveryMethod = SmtpDeliveryMethod.Network,
            UseDefaultCredentials = false
        };

        await client.SendMailAsync(mensaje);
    }
}
