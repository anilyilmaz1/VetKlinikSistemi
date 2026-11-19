using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace VetKlinik.Web.Models.Entities;

public class Pet
{
    public int Id { get; set; }

    [Required]
    public string OwnerId { get; set; } = string.Empty;
    public ApplicationUser? Owner { get; set; }

    [Required(ErrorMessage = "Evcil hayvan adı zorunludur.")]
    [StringLength(60)]
    [Display(Name = "Adı")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tür bilgisi zorunludur.")]
    [StringLength(40)]
    [Display(Name = "Tür (Kedi, Köpek vb.)")]
    public string Species { get; set; } = string.Empty;

    [Required(ErrorMessage = "Irk bilgisi zorunludur.")]
    [StringLength(60)]
    [Display(Name = "Irk")]
    public string Breed { get; set; } = string.Empty;

    [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Doğum Tarihi")]
    public DateTime BirthDate { get; set; }

    [Required(ErrorMessage = "Kilo bilgisi zorunludur.")]
    [Column(TypeName = "decimal(5,2)")]
    [Range(0.1, 200, ErrorMessage = "Kilo 0.1 ile 200 kg arasında olmalıdır.")]
    [Display(Name = "Kilo (kg)")]
    public decimal WeightKg { get; set; }

    [StringLength(300)]
    [Display(Name = "Fotoğraf Yolu")]
    public string? PhotoUrl { get; set; }

    public ICollection<VaccinationRecord> VaccinationRecords { get; set; } = new List<VaccinationRecord>();
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
}