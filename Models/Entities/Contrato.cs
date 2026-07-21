using System;
using System.Collections.Generic;

namespace Models.Entities;

public partial class Contrato
{
    public int ContratoId { get; set; }

    public int CotizacionId { get; set; }

    public string NumeroContrato { get; set; } = null!;

    public DateTime FechaContrato { get; set; }

    public string Estado { get; set; } = null!;

    public string? Observaciones { get; set; }

    public string? RutaPdf { get; set; }

    public int UsuarioCreacion { get; set; }

    public virtual Cotizacion Cotizacion { get; set; } = null!;

    public virtual Usuario UsuarioCreacionNavigation { get; set; } = null!;
}