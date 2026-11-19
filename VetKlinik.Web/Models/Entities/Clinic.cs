using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Web.Models.Entities;

public class Clinic
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Klinik adı zorunludur.")]
    [StringLength(120)]
    [Display(Name = "Klinik Adı")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Adres zorunludur.")]
    [StringLength(250)]
    [Display(Name = "Adres")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Telefon numarası zorunludur.")]
    [StringLength(20)]
    [Display(Name = "Telefon")]
    public string Phone { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Açılış Saati")]
    public TimeSpan OpeningTime { get; set; } = new TimeSpan(9, 0, 0);

    [Required]
    [Display(Name = "Kapanış Saati")]
    public TimeSpan ClosingTime { get; set; } = new TimeSpan(18, 0, 0);

    public ICollection<VetService> Services { get; set; } = new List<VetService>();
    public ICollection<Veterinarian> Veterinarians { get; set; } = new List<Veterinarian>();
}