using System;
using System.Collections.Generic;

namespace Data;

public partial class Auditoria
{
    public int AuditoriaId { get; set; }

    public int? UsuarioId { get; set; }

    public string Tabla { get; set; } = null!;

    public string Accion { get; set; } = null!;

    public int? RegistroId { get; set; }

    public string? ValorAnterior { get; set; }

    public string? ValorNuevo { get; set; }

    public DateTime FechaHora { get; set; }

    public string? DireccionIp { get; set; }

    public virtual Usuario? Usuario { get; set; }
}
