using System;
using System.Collections.Generic;

namespace Data;

public partial class Cotizacion
{
    public int CotizacionId { get; set; }

    public int EventoId { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = null!;

    public string? MotivoRechazo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public DateTime? FechaVencimiento { get; set; }

    public string? RutaPdf { get; set; }

    public int UsuarioCreacion { get; set; }

    public virtual Evento Evento { get; set; } = null!;

    public virtual Usuario UsuarioCreacionNavigation { get; set; } = null!;
}
