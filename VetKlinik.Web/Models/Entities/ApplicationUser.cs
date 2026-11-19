using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VetKlinik.Web.Models.Entities;

public class ApplicationUser : IdentityUser
{
    [Required(ErrorMessage = "Ad Soyad alanı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = string.Empty;

    [StringLength(250)]
    [Display(Name = "Adres")]
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Pet> Pets { get; set; } = new List<Pet>();
    public Veterinarian? VeterinarianProfile { get; set; }
}