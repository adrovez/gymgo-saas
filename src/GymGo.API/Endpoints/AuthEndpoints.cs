using GymGo.Application.Auth.Commands.Login;
using GymGo.Application.Auth.DTOs;
using MediatR;

namespace GymGo.API.Endpoints;

public static class AuthEndpoints
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Auth")
            .AllowAnonymous();

        // ── POST /api/v1/auth/login ──────────────────────────────────────────
        group.MapPost("/login", async (LoginCommand command, ISender sender, CancellationToken ct) =>
        {
            var response = await sender.Send(command, ct);
            return Results.Ok(response);
        })
        .WithSummary("Iniciar sesión")
        .WithDescription(
            "Autentica al usuario con email y contraseña. " +
            "Para usuarios GymAdmin / Receptionist / Instructor, enviar el header **X-Tenant-Id** con el Id del gimnasio. " +
            "PlatformAdmin no requiere ese header. " +
            "Retorna un JWT Bearer que debe incluirse en el header `Authorization: Bearer {token}` en las peticiones posteriores.")
        .Produces<LoginResponseDto>()
        .ProducesProblem(400)
        .ProducesProblem(422);

        return app;
    }
}
