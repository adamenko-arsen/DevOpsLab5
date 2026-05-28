namespace LibraryPlatform.Models.Dto;

// ── Auth ──
public record RegisterDto(string Username, string Email, string Password);
public record LoginDto(string Email, string Password);
public record AuthResponse(string Token, string Username, string Role, int UserId);

// ── Library ──
public record LibEntryCreateDto(
    string Name,
    string Version,
    string Language,
    string LicenseType,
    string RepositoryLink,
    string Description,
    int[] TagIds
);

public record LibEntryUpdateDto(
    string Name,
    string Version,
    string Language,
    string LicenseType,
    string RepositoryLink,
    string Description,
    int[] TagIds
);

public record LibEntryListDto(
    int Id,
    string Name,
    string Version,
    string Language,
    string LicenseType,
    string RepositoryLink,
    string Description,
    int? GitHubStars,
    int? GitHubOpenIssues,
    DateTime? GitHubLastUpdated,
    double AverageRating,
    int ReviewCount,
    string CreatedByUsername,
    DateTime CreatedAt,
    TagDto[] Tags
);

public record TagDto(int Id, string Name, string Category);

public record ReviewCreateDto(int Rating, string Comment);
public record ReviewDto(int Id, int Rating, string Comment, string Username, DateTime CreatedAt);

public record LanguageStatDto(string Language, int Count);
public record PlatformStatsDto(
    int TotalLibraries,
    int TotalUsers,
    int TotalReviews,
    LanguageStatDto[] LanguageDistribution
);
