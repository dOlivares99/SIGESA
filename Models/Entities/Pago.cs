using System;
using System.Collections.Generic;

namespace Data;

public partial class Pago
{
    public int PagoId { get; set; }

    public int EventoId { get; set; }

    public int TipoPagoId { get; set; }

    public int MetodoPagoId { get; set; }

    public decimal Monto { get; set; }

    public DateTime FechaPago { get; set; }

    public string? UrlComprobante { get; set; }

    public string? Observacion { get; set; }

    public int UsuarioId { get; set; }

    public virtual Evento Evento { get; set; } = null!;

    public virtual MetodoPago MetodoPago { get; set; } = null!;

    public virtual TipoPago TipoPago { get; set; } = null!;

    public virtual Usuario Usuario { get; set; } = null!;
}
