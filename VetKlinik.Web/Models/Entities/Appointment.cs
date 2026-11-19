using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using VetKlinik.Web.Models.Enums;

namespace VetKlinik.Web.Models.Entities;

public class Appointment
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Evcil Hayvan")]
    public int PetId { get; set; }
    public Pet? Pet { get; set; }

    [Required]
    [Display(Name = "Veteriner Hekim")]
    public int VeterinarianId { get; set; }
    public Veterinarian? Veterinarian { get; set; }

    [Required]
    [Display(Name = "Hizmet")]
    public int VetServiceId { get; set; }
    public VetService? VetService { get; set; }

    [Required(ErrorMessage = "Randevu tarihi zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Randevu Tarihi")]
    public DateTime AppointmentDate { get; set; }

    [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
    [Display(Name = "Başlangıç Saati")]
    public TimeSpan StartTime { get; set; }

    [Display(Name = "Bitiş Saati")]
    public TimeSpan EndTime { get; set; }

    [Display(Name = "Hizmet Süresi (Dk)")]
    public int DurationMinutes { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Ücret (₺)")]
    public decimal Price { get; set; }

    [Display(Name = "Durum")]
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;

    [StringLength(400)]
    [Display(Name = "Hasta Sahibi Notu")]
    public string? OwnerNotes { get; set; }

    [StringLength(1000)]
    [Display(Name = "Hekim Muayene Notu")]
    public string? ExaminationNotes { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}