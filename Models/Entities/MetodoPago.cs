using System;
using System.Collections.Generic;

using System.Text.Json.Serialization;

namespace Models.Entities;

public partial class MetodoPago
{
    public int MetodoPagoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    [JsonIgnore] // navegación inversa: no se serializa (evita grafo infinito en el API)
    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
