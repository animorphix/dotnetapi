using System.Diagnostics;
using System.Security.Claims;
using Api.Services;
using DAL;
using Microsoft.IdentityModel.Tokens;

namespace Api;

public class TokenValidatorMiddleware
{
   private readonly RequestDelegate _next;
   
   public TokenValidatorMiddleware(RequestDelegate next)
   {
        _next = next;
   }

   public async Task InvokeAsync(HttpContext context, UserService userService)
   {
        var isOk = true;
        var sessionIdString = context.User.Claims.FirstOrDefault(x => x.Type == "SessionId")?.Value;

        if (Guid.TryParse(sessionIdString, out var sessionId))
        {
            var session = await userService.GetSessionById(sessionId);

            if (!session.IsActive)
            {
                isOk = false;
                context.Response.Clear();
                context.Response.StatusCode = 401;
            }

        }

        if (isOk)
        {
            await _next(context);
        }
        //var auth = context.Request.Headers.Authorization.FirstOrDefault()?.;
        //var principal = new JwtSecurityTokenHandler().ValidateToken(auth, validParams, out var securityToken);
        
   }
}

public static class TokenValidatiorMiddlewareExtension
{
    public static IApplicationBuilder UseTokenValidator(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<TokenValidatorMiddleware>();
    }
}
