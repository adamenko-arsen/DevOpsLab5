using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryPlatform.Models;

public class Review
{
    public int Id { get; set; }

    [Range(1, 5)]
    public int Rating { get; set; }

    [MaxLength(1000)]
    public string Comment { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int LibEntryId { get; set; }
    public int UserId { get; set; }

    [ForeignKey(nameof(LibEntryId))]
    public LibEntry? LibEntry { get; set; }

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }
}
