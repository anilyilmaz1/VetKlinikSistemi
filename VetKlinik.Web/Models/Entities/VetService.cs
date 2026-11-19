using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetKlinik.Web.Models.Entities;

public class VetService
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Klinik")]
    public int ClinicId { get; set; }
    public Clinic? Clinic { get; set; }

    [Required(ErrorMessage = "Hizmet adı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Hizmet Adı")]
    public string Name { get; set; } = string.Empty;

    [StringLength(400)]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Required]
    [Range(10, 480, ErrorMessage = "Süre 10 ile 480 dakika arasında olmalıdır.")]
    [Display(Name = "Süre (Dakika)")]
    public int DurationMinutes { get; set; }

    [Required]
    [Column(TypeName = "decimal(10,2)")]
    [Range(0, 100000, ErrorMessage = "Geçerli bir ücret giriniz.")]
    [Display(Name = "Ücret (₺)")]
    public decimal Price { get; set; }

    public ICollection<VeterinarianService> VeterinarianServices { get; set; } = new List<VeterinarianService>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}