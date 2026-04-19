using Application.Interfaces;

namespace API.Middleware;

public class TokenBlacklistMiddleware
{
    private readonly RequestDelegate _next;

    public TokenBlacklistMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context, IRevokedTokenRepository revokedTokenRepo)
    {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (!string.IsNullOrEmpty(token))
        {
            var isRevoked = await revokedTokenRepo.IsRevokedAsync(token);
            if (isRevoked)
            {
                context.Response.StatusCode = 401;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync("{\"success\":false,\"message\":\"Token has been revoked. Please login again.\"}");
                return;
            }
        }

        await _next(context);
    }
}
