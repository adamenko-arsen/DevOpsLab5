using Microsoft.EntityFrameworkCore;
using LibraryPlatform.Data;
using LibraryPlatform.Models.Dto;

namespace LibraryPlatform.Endpoints;

public static class TagEndpoints
{
    public static void MapTagEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/tags").WithTags("Tags");

        group.MapGet("/", async (AppDbContext db) =>
        {
            var tags = await db.Tags
                .OrderBy(t => t.Category).ThenBy(t => t.Name)
                .Select(t => new TagDto(t.Id, t.Name, t.Category))
                .ToListAsync();
            return Results.Ok(tags);
        });
    }
}
