using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Models;

public class ProjectStatusReference
{
    [Key, StringLength(40)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string DisplayName { get; set; } = string.Empty;

    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}
