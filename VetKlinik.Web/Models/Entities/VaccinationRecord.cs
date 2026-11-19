using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Web.Models.Entities;

public class VaccinationRecord
{
    public int Id { get; set; }

    [Required]
    [Display(Name = "Evcil Hayvan")]
    public int PetId { get; set; }
    public Pet? Pet { get; set; }

    [Display(Name = "Uygulayan Hekim")]
    public int? VeterinarianId { get; set; }
    public Veterinarian? Veterinarian { get; set; }

    [Required(ErrorMessage = "Aşı adı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Aşı Adı")]
    public string VaccineName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yapıldığı tarih zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Yapıldığı Tarih")]
    public DateTime AdministeredDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Bir Sonraki Doz Tarihi")]
    public DateTime? NextDueDate { get; set; }

    [StringLength(300)]
    [Display(Name = "Notlar")]
    public string? Notes { get; set; }
}