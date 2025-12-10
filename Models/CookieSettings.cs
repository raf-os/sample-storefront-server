using System.ComponentModel.DataAnnotations;

namespace SampleStorefront.Models;

public class CookieSettings
{
    [Required]
    [Range(1, 365)]
    public int ExpiryDays { get; set; }

    public bool Secure { get; set; } = true;
    public SameSiteMode SameSite { get; set; } = SameSiteMode.None;
}