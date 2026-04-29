using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryPlatform.Models;

public class Favorite
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public int LibEntryId { get; set; }

    public DateTime AddedAt { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(UserId))]
    public User? User { get; set; }

    [ForeignKey(nameof(LibEntryId))]
    public LibEntry? LibEntry { get; set; }
}
