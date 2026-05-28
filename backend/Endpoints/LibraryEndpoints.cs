using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using LibraryPlatform.Data;
using LibraryPlatform.Models;
using LibraryPlatform.Models.Dto;
using LibraryPlatform.Services;
using System.Text.Json;

namespace LibraryPlatform.Endpoints;

public static class LibraryEndpoints
{
    public static void MapLibraryEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/libraries").WithTags("Libraries");

        group.MapGet("/", async (
            AppDbContext db,
            string? search,
            string? language,
            int? tagId,
            string? sort,
            int page = 1,
            int pageSize = 12) =>
        {
            var query = db.LibEntries
                .Include(l => l.Reviews)
                .Include(l => l.LibraryTags).ThenInclude(lt => lt.Tag)
                .Include(l => l.CreatedByUser)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(l => l.Name.Contains(search));

            if (!string.IsNullOrWhiteSpace(language))
                query = query.Where(l => l.Language == language);

            if (tagId.HasValue)
                query = query.Where(l => l.LibraryTags.Any(lt => lt.TagId == tagId.Value));

            query = sort switch
            {
                "stars"  => query.OrderByDescending(l => l.GitHubStars ?? 0),
                "rating" => query.OrderByDescending(l => l.Reviews.Any() ? l.Reviews.Average(r => r.Rating) : 0),
                _        => query.OrderByDescending(l => l.CreatedAt)
            };

            var total = await query.CountAsync();
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            var result = items.Select(l => new LibEntryListDto(
                l.Id, l.Name, l.Version, l.Language, l.LicenseType,
                l.RepositoryLink, l.Description,
                l.GitHubStars, l.GitHubOpenIssues, l.GitHubLastUpdated,
                l.Reviews.Any() ? Math.Round(l.Reviews.Average(r => r.Rating), 1) : 0,
                l.Reviews.Count,
                l.CreatedByUser?.Username ?? "System",
                l.CreatedAt,
                l.LibraryTags.Select(lt => new TagDto(lt.Tag!.Id, lt.Tag.Name, lt.Tag.Category)).ToArray()
            ));

            return Results.Ok(new { total, page, pageSize, data = result });
        });

        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var l = await db.LibEntries
                .Include(l => l.Reviews).ThenInclude(r => r.User)
                .Include(l => l.LibraryTags).ThenInclude(lt => lt.Tag)
                .Include(l => l.CreatedByUser)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (l is null) return Results.NotFound();

            return Results.Ok(new LibEntryListDto(
                l.Id, l.Name, l.Version, l.Language, l.LicenseType,
                l.RepositoryLink, l.Description,
                l.GitHubStars, l.GitHubOpenIssues, l.GitHubLastUpdated,
                l.Reviews.Any() ? Math.Round(l.Reviews.Average(r => r.Rating), 1) : 0,
                l.Reviews.Count,
                l.CreatedByUser?.Username ?? "System",
                l.CreatedAt,
                l.LibraryTags.Select(lt => new TagDto(lt.Tag!.Id, lt.Tag.Name, lt.Tag.Category)).ToArray()
            ));
        });

        group.MapPost("/", async (LibEntryCreateDto dto, ClaimsPrincipal claims, AppDbContext db, GitHubService github) =>
        {
            var userId = int.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var entry = new LibEntry
            {
                Name = dto.Name,
                Version = dto.Version,
                Language = dto.Language,
                LicenseType = dto.LicenseType,
                RepositoryLink = dto.RepositoryLink,
                Description = dto.Description,
                CreatedByUserId = userId
            };

            if (!string.IsNullOrWhiteSpace(dto.RepositoryLink))
            {
                var info = await github.FetchRepoInfoAsync(dto.RepositoryLink);
                if (info is not null)
                {
                    entry.GitHubStars = info.Stars;
                    entry.GitHubOpenIssues = info.OpenIssues;
                    entry.GitHubLastUpdated = info.LastUpdated;
                }
            }

            db.LibEntries.Add(entry);
            await db.SaveChangesAsync();

            if (dto.TagIds.Length > 0)
            {
                foreach (var tagId in dto.TagIds)
                    db.LibraryTags.Add(new LibraryTag { LibEntryId = entry.Id, TagId = tagId });
                await db.SaveChangesAsync();
            }

            return Results.Created($"/api/libraries/{entry.Id}", new { entry.Id });
        }).RequireAuthorization();

        group.MapPut("/{id:int}", async (int id, LibEntryUpdateDto dto, ClaimsPrincipal claims, AppDbContext db, GitHubService github) =>
        {
            var entry = await db.LibEntries.Include(l => l.LibraryTags).FirstOrDefaultAsync(l => l.Id == id);
            if (entry is null) return Results.NotFound();

            var userId = int.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = claims.FindFirstValue(ClaimTypes.Role);
            if (role != "Admin" && entry.CreatedByUserId != userId)
                return Results.Forbid();

            entry.Name = dto.Name;
            entry.Version = dto.Version;
            entry.Language = dto.Language;
            entry.LicenseType = dto.LicenseType;
            entry.RepositoryLink = dto.RepositoryLink;
            entry.Description = dto.Description;

            if (!string.IsNullOrWhiteSpace(dto.RepositoryLink))
            {
                var info = await github.FetchRepoInfoAsync(dto.RepositoryLink);
                if (info is not null)
                {
                    entry.GitHubStars = info.Stars;
                    entry.GitHubOpenIssues = info.OpenIssues;
                    entry.GitHubLastUpdated = info.LastUpdated;
                }
            }

            db.LibraryTags.RemoveRange(entry.LibraryTags);
            foreach (var tagId in dto.TagIds)
                db.LibraryTags.Add(new LibraryTag { LibEntryId = entry.Id, TagId = tagId });

            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        group.MapDelete("/{id:int}", async (int id, ClaimsPrincipal claims, AppDbContext db) =>
        {
            var role = claims.FindFirstValue(ClaimTypes.Role);
            if (role != "Admin") return Results.Forbid();

            var entry = await db.LibEntries.FindAsync(id);
            if (entry is null) return Results.NotFound();

            db.LibEntries.Remove(entry);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();

        group.MapGet("/languages", async (AppDbContext db) =>
        {
            var langs = await db.LibEntries.Select(l => l.Language).Distinct().OrderBy(l => l).ToListAsync();
            return Results.Ok(langs);
        });

        group.MapPost("/{id:int}/refresh-github", async (int id, AppDbContext db, GitHubService github) =>
        {
            var entry = await db.LibEntries.FindAsync(id);
            if (entry is null) return Results.NotFound();

            var info = await github.FetchRepoInfoAsync(entry.RepositoryLink);
            if (info is null) return Results.BadRequest(new { error = "Could not fetch GitHub data." });

            entry.GitHubStars = info.Stars;
            entry.GitHubOpenIssues = info.OpenIssues;
            entry.GitHubLastUpdated = info.LastUpdated;
            await db.SaveChangesAsync();

            return Results.Ok(new { info.Stars, info.OpenIssues, info.LastUpdated });
        }).RequireAuthorization();
    }
}
