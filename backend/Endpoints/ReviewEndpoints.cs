using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using LibraryPlatform.Data;
using LibraryPlatform.Models;
using LibraryPlatform.Models.Dto;

namespace LibraryPlatform.Endpoints;

public static class ReviewEndpoints
{
    public static void MapReviewEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api").WithTags("Reviews");

        group.MapGet("/libraries/{libId:int}/reviews", async (int libId, AppDbContext db) =>
        {
            var reviews = await db.Reviews
                .Where(r => r.LibEntryId == libId)
                .Include(r => r.User)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new ReviewDto(r.Id, r.Rating, r.Comment, r.User!.Username, r.CreatedAt))
                .ToListAsync();

            return Results.Ok(reviews);
        });

        group.MapPost("/libraries/{libId:int}/reviews", async (int libId, ReviewCreateDto dto, ClaimsPrincipal claims, AppDbContext db) =>
        {
            var userId = int.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier)!);

            if (await db.Reviews.AnyAsync(r => r.UserId == userId && r.LibEntryId == libId))
                return Results.BadRequest(new { error = "You have already reviewed this library." });

            var review = new Review
            {
                LibEntryId = libId,
                UserId = userId,
                Rating = dto.Rating,
                Comment = dto.Comment
            };

            db.Reviews.Add(review);
            await db.SaveChangesAsync();
            return Results.Created($"/api/libraries/{libId}/reviews", new { review.Id });
        }).RequireAuthorization();

        group.MapDelete("/reviews/{id:int}", async (int id, ClaimsPrincipal claims, AppDbContext db) =>
        {
            var review = await db.Reviews.FindAsync(id);
            if (review is null) return Results.NotFound();

            var userId = int.Parse(claims.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var role = claims.FindFirstValue(ClaimTypes.Role);
            if (role != "Admin" && review.UserId != userId)
                return Results.Forbid();

            db.Reviews.Remove(review);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();
    }
}
