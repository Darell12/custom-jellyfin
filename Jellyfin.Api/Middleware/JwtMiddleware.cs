using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.Proxy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.IdentityModel.Tokens;

namespace Jellyfin.Api.Middleware;

/// <summary>
/// Middleware para interceptar las solicitudes antes de llegar al controlador capturando el jwt.
/// </summary>
public class JwtMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _secrectKey =
        "ddee6a2109c0e7b03aefc530f1d4b3da6825515bf69e322a14cfaa7e0879e9e4";

    /// <summary>
    /// Initializes a new instance of the <see cref="JwtMiddleware"/> class.
    /// </summary>
    /// <param name="next">El próximo delegado en la cadena de procesamiento.</param>
    public JwtMiddleware(RequestDelegate next)
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
        // Lógica para leer el token JWT de la URL
        var token = context.Request.Query["token"].FirstOrDefault();

        if (string.IsNullOrEmpty(token))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized").ConfigureAwait(false);
            return;
        }

        if (!IsValidToken(token))
        {
            context.Response.StatusCode = 403;
            await context.Response.WriteAsync("Invalid token").ConfigureAwait(false);
            return;
        }

        try
        {
            context.Items["Usuario"] = ObtenerIdElementoDesdeToken(token);
            context.Items["IdElemento"] = ObtenerUsuarioDesdeToken(token);
        }
        catch (SecurityTokenException)
        {
        context.Response.StatusCode = 401;
        await context.Response.WriteAsync("Invalid token").ConfigureAwait(false);
        return;
        }

        // // Lógica para verificar el token y tomar decisiones
        // if (!string.IsNullOrEmpty(token))
        // {
        //     // Capturamos token
        //     if (IsValidToken(token))
        //     {
        //         context.Items["Usuario"] = ObtenerUsuarioDesdeToken(token);
        //         context.Items["IdElemento"] = ObtenerIdElementoDesdeToken(token);
        //     }
        //     else
        //     {
        //         // Si el token no es válido, responde con un código de estado no autorizado
        //         context.Response.StatusCode = 401;
        //         await context.Response.WriteAsync("Acceso no autorizado: token inválido").ConfigureAwait(false);
        //         return;
        //     }
        // }

        // Continuar con el siguiente middleware
        await _next(context).ConfigureAwait(false);
    }

    private bool IsValidToken(string token)
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
