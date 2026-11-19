using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Web.Models.Entities;

public class VeterinarianAvailability
{
    public int Id { get; set; }

    [Required]
    public int VeterinarianId { get; set; }
    public Veterinarian? Veterinarian { get; set; }

    [Required]
    [Display(Name = "Gün")]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    [Display(Name = "Başlangıç Saati")]
    public TimeSpan StartTime { get; set; }

    [Required]
    [Display(Name = "Bitiş Saati")]
    public TimeSpan EndTime { get; set; }

    [Display(Name = "Müsait mi?")]
    public bool IsAvailable { get; set; } = true;
}