using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using LibraryPlatform.Data;
using LibraryPlatform.Models;
using LibraryPlatform.Models.Dto;

namespace LibraryPlatform.Endpoints;

public static class FavoriteEndpoints
{
    public static void MapFavoriteEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/favorites").WithTags("Favorites").RequireAuthorization();

        group.MapGet("/", async (ClaimsPrincipal claims, AppDbContext db) =>
        {
            var userId = int.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var favs = await db.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.LibEntry).ThenInclude(l => l!.Reviews)
                .Include(f => f.LibEntry).ThenInclude(l => l!.LibraryTags).ThenInclude(lt => lt.Tag)
                .Include(f => f.LibEntry).ThenInclude(l => l!.CreatedByUser)
                .Select(f => f.LibEntry!)
                .ToListAsync();

            var result = favs.Select(l => new LibEntryListDto(
                l.Id, l.Name, l.Version, l.Language, l.LicenseType,
                l.RepositoryLink, l.Description,
                l.GitHubStars, l.GitHubOpenIssues, l.GitHubLastUpdated,
                l.Reviews.Any() ? Math.Round(l.Reviews.Average(r => r.Rating), 1) : 0,
                l.Reviews.Count,
                l.CreatedByUser?.Username ?? "System",
                l.CreatedAt,
                l.LibraryTags.Select(lt => new TagDto(lt.Tag!.Id, lt.Tag.Name, lt.Tag.Category)).ToArray()
            ));

            return Results.Ok(result);
        });

        group.MapPost("/{libId:int}", async (int libId, ClaimsPrincipal claims, AppDbContext db) =>
        {
            var userId = int.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (await db.Favorites.AnyAsync(f => f.UserId == userId && f.LibEntryId == libId))
                return Results.BadRequest(new { error = "Already in favorites." });

            db.Favorites.Add(new Favorite { UserId = userId, LibEntryId = libId });
            await db.SaveChangesAsync();
            return Results.Ok(new { message = "Added to favorites." });
        });

        group.MapDelete("/{libId:int}", async (int libId, ClaimsPrincipal claims, AppDbContext db) =>
        {
            var userId = int.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var fav = await db.Favorites.FirstOrDefaultAsync(f => f.UserId == userId && f.LibEntryId == libId);
            if (fav is null) return Results.NotFound();

            db.Favorites.Remove(fav);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });

        group.MapGet("/ids", async (ClaimsPrincipal claims, AppDbContext db) =>
        {
            var userId = int.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var ids = await db.Favorites.Where(f => f.UserId == userId).Select(f => f.LibEntryId).ToListAsync();
            return Results.Ok(ids);
        });
    }
}
