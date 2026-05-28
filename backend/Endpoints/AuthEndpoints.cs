using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using LibraryPlatform.Data;
using LibraryPlatform.Models;
using LibraryPlatform.Models.Dto;

namespace LibraryPlatform.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (RegisterDto dto, AppDbContext db, IConfiguration config) =>
        {
            if (await db.Users.AnyAsync(u => u.Email == dto.Email))
                return Results.BadRequest(new { error = "Email already registered." });

            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = UserRole.User
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var token = GenerateJwt(user, config);
            return Results.Ok(new AuthResponse(token, user.Username, user.Role.ToString(), user.Id));
        });

        group.MapPost("/login", async (LoginDto dto, AppDbContext db, IConfiguration config) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);
            if (user is null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Results.Unauthorized();

            var token = GenerateJwt(user, config);
            return Results.Ok(new AuthResponse(token, user.Username, user.Role.ToString(), user.Id));
        });

        group.MapGet("/me", async (ClaimsPrincipal claims, AppDbContext db) =>
        {
            var userId = int.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await db.Users.FindAsync(userId);
            if (user is null) return Results.Unauthorized();
            return Results.Ok(new { user.Id, user.Username, user.Email, Role = user.Role.ToString() });
        }).RequireAuthorization();

        group.MapGet("/test", (ClaimsPrincipal claims) =>
        {
            var claimList = claims.Claims.Select(c => new { c.Type, c.Value }).ToList();
            return Results.Ok(new { authenticated = claims.Identity?.IsAuthenticated, claimList });
        }).RequireAuthorization();
    }

    private static string GenerateJwt(User user, IConfiguration config)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString())
        };

        var token = new JwtSecurityToken(
            issuer: config["Jwt:Issuer"],
            audience: config["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(double.Parse(config["Jwt:ExpireMinutes"]!)),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
