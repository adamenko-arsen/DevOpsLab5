using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LibraryPlatform.Models;

public class Tag
{
    public int Id { get; set; }

    [Required, MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Category { get; set; } = string.Empty;

    public ICollection<LibraryTag> LibraryTags { get; set; } = new List<LibraryTag>();
}

public class LibraryTag
{
    public int LibEntryId { get; set; }
    public int TagId { get; set; }

    [ForeignKey(nameof(LibEntryId))]
    public LibEntry? LibEntry { get; set; }

    [ForeignKey(nameof(TagId))]
    public Tag? Tag { get; set; }
}
