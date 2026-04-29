using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryPlatform.Models;

public class LibEntry
{
    public int Id { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(30)]
    public string Version { get; set; } = string.Empty;

    [Required, MaxLength(50)]
    public string Language { get; set; } = string.Empty;

    [MaxLength(80)]
    public string LicenseType { get; set; } = string.Empty;

    [MaxLength(500)]
    public string RepositoryLink { get; set; } = string.Empty;

    [MaxLength(2000)]
    public string Description { get; set; } = string.Empty;

    public int? GitHubStars { get; set; }
    public int? GitHubOpenIssues { get; set; }
    public DateTime? GitHubLastUpdated { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CreatedByUserId { get; set; }

    [ForeignKey(nameof(CreatedByUserId))]
    public User? CreatedByUser { get; set; }

    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
    public ICollection<LibraryTag> LibraryTags { get; set; } = new List<LibraryTag>();
}
