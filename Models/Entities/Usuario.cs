using System;
using System.Collections.Generic;

using System.Text.Json.Serialization;

namespace Models.Entities;

public partial class Usuario
{
    public int UsuarioId { get; set; }

    public int RolId { get; set; }

    public string Nombre { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    [JsonIgnore] // navegación inversa: no se serializa (evita grafo infinito en el API)
    public virtual ICollection<Auditoria> Auditoria { get; set; } = new List<Auditoria>();

    [JsonIgnore] // navegación inversa: no se serializa (evita grafo infinito en el API)
    public virtual ICollection<Cotizacion> Cotizacions { get; set; } = new List<Cotizacion>();

    [JsonIgnore] // navegación inversa: no se serializa (evita grafo infinito en el API)
    public virtual ICollection<Contrato> Contratos { get; set; } = new List<Contrato>();

    [JsonIgnore] // navegación inversa: no se serializa (evita grafo infinito en el API)
    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();

    [JsonIgnore] // navegación inversa: no se serializa (evita grafo infinito en el API)
    public virtual ICollection<Pago> Pagos { get; set; } = new List<Pago>();

    public virtual Rol Rol { get; set; } = null!;
}
