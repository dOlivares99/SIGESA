using Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientesApiController(IClienteService clienteService) : ControllerBase
{
    // GET api/clientesapi
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var clientes = await clienteService.ObtenerTodosAsync();
        return Ok(clientes);
    }

    // GET api/clientesapi/5
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var cliente = await clienteService.ObtenerPorIdAsync(id);
        if (cliente == null) return NotFound();
        return Ok(cliente);
    }

    // GET api/clientesapi/5/historial
    [HttpGet("{id}/historial")]
    public async Task<IActionResult> Historial(int id)
    {
        var cliente = await clienteService.ObtenerConHistorialAsync(id);
        if (cliente == null) return NotFound();
        return Ok(cliente);
    }

    // POST api/clientesapi
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CrearClienteRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var documentoExiste = await clienteService.DocumentoExisteAsync(request.Documento);
        if (documentoExiste)
            return Conflict(new { mensaje = "El documento ya está registrado." });

        var result = await clienteService.CrearAsync(
            request.Nombre, request.Documento, request.Telefono, request.Email);

        return result
            ? Ok(new { mensaje = "Cliente creado correctamente." })
            : StatusCode(500, new { mensaje = "Error al crear el cliente." });
    }

    // PUT api/clientesapi/5
    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] ActualizarClienteRequest request)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var documentoExiste = await clienteService.DocumentoExisteAsync(request.Documento, id);
        if (documentoExiste)
            return Conflict(new { mensaje = "El documento ya está en uso por otro cliente." });

        var result = await clienteService.ActualizarAsync(
            id, request.Nombre, request.Documento, request.Telefono, request.Email);

        return result
            ? Ok(new { mensaje = "Cliente actualizado correctamente." })
            : NotFound(new { mensaje = "Cliente no encontrado." });
    }
}

public record CrearClienteRequest(string Nombre, string Documento, string Telefono, string? Email);
public record ActualizarClienteRequest(string Nombre, string Documento, string Telefono, string? Email);