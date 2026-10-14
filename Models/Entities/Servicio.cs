using System;
using System.Collections.Generic;

using System.Text.Json.Serialization;

namespace Models.Entities;

public partial class Servicio
{
    public int ServicioId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Categoria { get; set; } = null!;

    public decimal PrecioUnitario { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    [JsonIgnore] // navegación inversa: no se serializa (evita grafo infinito en el API)
    public virtual ICollection<EventoServicio> EventoServicios { get; set; } = new List<EventoServicio>();
}
