using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Api.Controllers;

/// <summary>
/// The videos controller.
/// </summary>
[ApiController]
[Route("apu/[controller]")]
public class HelloWorldController : ControllerBase
{
    /// <summary>
    /// Get a "Hello World" message.
    /// </summary>
    /// <response code="200">Hello World message retrieved.</response>
    /// <returns>A <see cref="ActionResult{String}"/> with "Hello World" message.</returns>
    [HttpGet("HelloWorld")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<string> GetHelloWorld()
    {
        return Ok("Hello World!");
    }

    /// <summary>
    /// Get a "Hello World" message.
    /// </summary>
    /// <response code="200">Hello World message retrieved.</response>
    /// <returns>A <see cref="ActionResult{String}"/> with "Hello World" message.</returns>
    [HttpGet("saludo")]
    public ActionResult<string> ObtenerSaludo()
    {
        var usuario = HttpContext.Items["Usuario"]?.ToString();
        var idElemento = HttpContext.Items["IdElemento"]?.ToString();

        // Hacer algo con los datos del token
        var saludo = $"Hola, {usuario} (ID: {idElemento}) desde el controlador.";

        return Ok(saludo);
    }

    /// <summary>
    /// Get a "Hello World" message.
    /// </summary>
    /// <response code="200">Hello World message retrieved.</response>
    /// <returns>A <see cref="ActionResult{String}"/> with "Hello World" message.</returns>
    [HttpGet("redirect")]
    public ActionResult<string> ObtenerRedireccion()
    {
        var usuario = HttpContext.Items["Usuario"]?.ToString();
        var idElemento = HttpContext.Items["IdElemento"]?.ToString();

        // Hacer algo con los datos del token
        var saludo = $"Hola, {usuario} (ID: {idElemento}) desde el controlador.";

        return Ok(saludo);
    }
}
