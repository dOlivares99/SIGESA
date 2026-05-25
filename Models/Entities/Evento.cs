using System;
using System.Collections.Generic;

namespace Models.Entities;

public partial class Evento
{
    public int EventoId { get; set; }

    public int ClienteId { get; set; }

    public int PaqueteId { get; set; }

    public string TipoEvento { get; set; } = null!;

    public DateOnly FechaEvento { get; set; }

    public int NumPersonas { get; set; }

    public string Estado { get; set; } = null!;

    public decimal MontoTotal { get; set; }

    public decimal MontoPagado { get; set; }

    public decimal? SaldoPendiente { get; set; }

    public string EstadoPago { get; set; } = null!;

    public string? Notas { get; set; }

    public DateTime FechaCreacion { get; set; }

    public int UsuarioCreacion { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<Cotizacion> Cotizacions { get; set; } = new List<Cotizacion>();

    public virtual ICollection<EventoServicio> EventoServicios { get; set; } = new List<EventoServicio>();

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual Paquete Paquete { get; set; } = null!;

    public virtual Usuario UsuarioCreacionNavigation { get; set; } = null!;
}
