using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using LibraryPlatform.Data;
using LibraryPlatform.Models.Dto;
using LibraryPlatform.Services;

namespace LibraryPlatform.Endpoints;

public static class ReportEndpoints
{
    public static void MapReportEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reports").WithTags("Reports");

        group.MapGet("/stats", async (AppDbContext db) =>
        {
            try
            {
                Console.WriteLine("[STATS] Starting query...");

                var totalLibs = await db.LibEntries.CountAsync();
                Console.WriteLine("[STATS] Libraries: " + totalLibs);

                var totalUsers = await db.Users.CountAsync();
                Console.WriteLine("[STATS] Users: " + totalUsers);

                var totalReviews = await db.Reviews.CountAsync();
                Console.WriteLine("[STATS] Reviews: " + totalReviews);

                var langStats = await db.LibEntries
                    .GroupBy(l => l.Language)
                    .Select(g => new { Lang = g.Key, Cnt = g.Count() })
                    .OrderByDescending(s => s.Cnt)
                    .ToListAsync();

                Console.WriteLine("[STATS] Languages: " + langStats.Count);

                var result = new
                {
                    totalLibraries = totalLibs,
                    totalUsers = totalUsers,
                    totalReviews = totalReviews,
                    languageDistribution = langStats.Select(s => new { language = s.Lang, count = s.Cnt }).ToArray()
                };

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[STATS ERROR] " + ex.GetType().Name + ": " + ex.Message);
                Console.WriteLine("[STATS ERROR] Stack: " + ex.StackTrace);
                if (ex.InnerException != null)
                    Console.WriteLine("[STATS ERROR] Inner: " + ex.InnerException.Message);

                return Results.Ok(new
                {
                    totalLibraries = 0,
                    totalUsers = 0,
                    totalReviews = 0,
                    languageDistribution = Array.Empty<object>(),
                    error = ex.Message
                });
            }
        });

        group.MapGet("/export/excel", async (ClaimsPrincipal claims, AppDbContext db, ReportService reports) =>
        {
            var role = claims.FindFirstValue(ClaimTypes.Role);
            Console.WriteLine("[EXPORT] Excel requested by role: " + (role ?? "null"));
            if (role != "Admin") return Results.Forbid();

            var libs = await db.LibEntries.OrderBy(l => l.Name).ToListAsync();
            var bytes = reports.GenerateExcel(libs);
            return Results.File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Libraries.xlsx");
        }).RequireAuthorization();

        group.MapGet("/export/pdf", async (ClaimsPrincipal claims, AppDbContext db, ReportService reports) =>
        {
            var role = claims.FindFirstValue(ClaimTypes.Role);
            Console.WriteLine("[EXPORT] PDF requested by role: " + (role ?? "null"));
            if (role != "Admin") return Results.Forbid();

            var libs = await db.LibEntries.OrderBy(l => l.Name).ToListAsync();
            var bytes = reports.GeneratePdf(libs);
            return Results.File(bytes, "application/pdf", "Libraries.pdf");
        }).RequireAuthorization();
    }
}
