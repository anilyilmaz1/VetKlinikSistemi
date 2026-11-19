using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Web.Models.Entities;

public class Veterinarian
{
    public int Id { get; set; }

    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    [Required]
    [Display(Name = "Klinik")]
    public int ClinicId { get; set; }
    public Clinic? Clinic { get; set; }

    [Required(ErrorMessage = "Hekim adı soyadı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Hekim Adı Soyadı")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
    [StringLength(150)]
    [Display(Name = "Uzmanlık Alanı")]
    public string Specialization { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Hakkında")]
    public string? Bio { get; set; }

    public ICollection<VeterinarianService> VeterinarianServices { get; set; } = new List<VeterinarianService>();
    public ICollection<VeterinarianAvailability> Availabilities { get; set; } = new List<VeterinarianAvailability>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}