using System;
using System.Collections.Generic;

using System.Text.Json.Serialization;

namespace Models.Entities;

public partial class Paquete
{
    public int PaqueteId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal PrecioBase { get; set; }

    public int MaxPersonas { get; set; }

    public int DuracionHoras { get; set; }

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    [JsonIgnore] // navegación inversa: no se serializa (evita grafo infinito en el API)
    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();
}
