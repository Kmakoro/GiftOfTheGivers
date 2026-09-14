using System.ComponentModel.DataAnnotations;

namespace GiftOfTheGivers.Models;

public class CurrencyReference
{
    [Key, StringLength(3, MinimumLength = 3)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(60)]
    public string DisplayName { get; set; } = string.Empty;

    [Required, StringLength(8)]
    public string Symbol { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}
