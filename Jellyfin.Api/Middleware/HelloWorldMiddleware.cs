using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace Jellyfin.Api.Middleware;

/// <summary>
/// Middleware para interceptar las solicitudes antes de llegar al controlador.
/// </summary>
public class HelloWorldMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _secrectKey =
        "ddee6a2109c0e7b03aefc530f1d4b3da6825515bf69e322a14cfaa7e0879e9e4";

    /// <summary>
    /// Initializes a new instance of the <see cref="HelloWorldMiddleware"/> class.
    /// </summary>
    /// <param name="next">El próximo delegado en la cadena de procesamiento.</param>
    public HelloWorldMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Método invocado al procesar una solicitud.
    /// </summary>
    /// <param name="context">El contexto de la solicitud HTTP.</param>
    /// <returns>Una tarea que representa la operación asincrónica.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        // Obtener token
        if (context.Request.Headers.TryGetValue("Authorization", out StringValues headerValues))
        {
            var token = headerValues
                .ToString()
                .Replace("Bearer ", string.Empty, StringComparison.Ordinal);
            // Validar el token
            if (EsTokenValido(token))
            {
                // Agregar los datos del token al HttpContext.Items
                context.Items["Usuario"] = ObtenerUsuarioDesdeToken(token);
                var idElementoGuidNullable = ObtenerIdElementoDesdeToken(token);
                if (idElementoGuidNullable.HasValue)
                {
                string idElemento = idElementoGuidNullable.Value.ToString().Replace("-", string.Empty, StringComparison.Ordinal).Trim();
                context.Items["IdElemento"] = idElemento.Trim();
                }
                else
                {
                context.Items["IdElemento"] = ObtenerIdElementoDesdeToken(token);
                }
            }
            else
            {
                // Si el token no es válido, responde con un código de estado no autorizado
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Acceso no autorizado token invalido").ConfigureAwait(false);
            }
        }
        else
        {
            // Si no se proporciona un token en el encabezado de autorización, responde con un código de estado no autorizado
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Acceso no autorizado Token no proporcionado").ConfigureAwait(false);
        }

        // Puedes realizar lógica antes de pasar la solicitud al siguiente middleware o controlador
        // En este caso, simplemente escribiremos en la consola para demostrar la interceptación.
        Console.WriteLine("HelloWorld intercepted.");
        Console.WriteLine(context.Items["Usuario"]);
        Console.WriteLine(context.Items["IdElemento"]);
        await _next.Invoke(context).ConfigureAwait(false);
    }

    private bool EsTokenValido(string token)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.UTF8.GetBytes(_secrectKey);

            // Configuración de la validación del token
            var validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            // Validar el token
            var principal = tokenHandler.ValidateToken(
                token,
                validationParameters,
                out var validatedToken);
            return true; // La validación fue exitosa

            // Si necesitas acceder a las claims del token, puedes hacerlo a través de 'principal'
        }
        catch (Exception)
        {
            // Manejar cualquier error durante la validación
            return false; // La validación no fue exitosa
        }
    }

    private Guid? ObtenerIdElementoDesdeToken(string token)
    {
        try
        {
            // Configuración del lector de token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.UTF8.GetBytes(_secrectKey); // Asegúrate de tener la clave correcta

            // Configuración de la validación del token
            var validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            // Decodificar el token
            var claimsPrincipal = tokenHandler.ValidateToken(
                token,
                validationParameters,
                out var validatedToken);

            // Verificar si Identity es nulo antes de intentar acceder a sus propiedades
            if (claimsPrincipal?.Identity != null)
            {
            // Extraer el ID del elemento desde las claims
            var idElementoClaim = ((ClaimsIdentity)claimsPrincipal.Identity).FindFirst(
                "IdElemento");

            if (idElementoClaim != null && Guid.TryParse(idElementoClaim.Value, out Guid idElementoGuid))
            {
                // Devolver el Guid obtenido
                return idElementoGuid;
            }
            }

            // Si no se encuentra la claim "IdElemento" o Identity es nulo, devuelve null o lanza una excepción según tus necesidades.
            return null;
        }
        catch (Exception ex)
        {
            // Manejar cualquier excepción durante la decodificación del token
            Console.WriteLine($"Error al decodificar el token: {ex.Message}");
            return null;
        }
    }

    private string? ObtenerUsuarioDesdeToken(string token)
    {
        try
        {
            // Configuración del lector de token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.UTF8.GetBytes(_secrectKey); // Asegúrate de tener la clave correcta

            // Configuración de la validación del token
            var validationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero
            };

            // Decodificar el token
            var claimsPrincipal = tokenHandler.ValidateToken(
                token,
                validationParameters,
                out var validatedToken);

            if (claimsPrincipal?.Identity != null)
            {
                // Extraer el ID del elemento desde las claims
                var usuarioClaim = ((ClaimsIdentity)claimsPrincipal.Identity).FindFirst("usuario");

                if (usuarioClaim != null)
                {
                    return usuarioClaim.Value;
                }
            }

            // Si no se encuentra la claim "IdElemento", devuelve null o lanza una excepción según tus necesidades.
            return null;
        }
        catch (Exception ex)
        {
            // Manejar cualquier excepción durante la decodificación del token
            Console.WriteLine($"Error al decodificar el token: {ex.Message}");
            return null;
        }
    }
}

/// <summary>
/// Middleware para interceptar las solicitudes antes de llegar al controlador es.
/// </summary>
#pragma warning disable SA1402 // FileMayOnlyContainASingleType
public static class HelloWorldMiddlewareExtensions
#pragma warning restore SA1402 // FileMayOnlyContainASingleType
{
    /// <summary>
    /// Joins a first name and a last name together into a single string.
    /// </summary>
    /// <param name="builder">The first name to join.</param>
    /// <returns>The joined names.</returns>
    public static IApplicationBuilder UseHelloWorldMiddleware(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<HelloWorldMiddleware>();
    }
}
