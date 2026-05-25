using System;
using System.Collections.Generic;

namespace Models.Entities;

public partial class TipoPago
{
    public int TipoPagoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
