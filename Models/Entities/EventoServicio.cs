using System;
using System.Collections.Generic;

namespace Data;

public partial class EventoServicio
{
    public int EventoServicioId { get; set; }

    public int EventoId { get; set; }

    public int ServicioId { get; set; }

    public int Cantidad { get; set; }

    public decimal PrecioAcordado { get; set; }

    public virtual Evento Evento { get; set; } = null!;

    public virtual Servicio Servicio { get; set; } = null!;
}
