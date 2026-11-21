using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Web.Models.ViewModels;

public class PetViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Evcil hayvan adı zorunludur.")]
    [StringLength(60)]
    [Display(Name = "Adı")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tür bilgisi zorunludur.")]
    [StringLength(40)]
    [Display(Name = "Tür")]
    public string Species { get; set; } = string.Empty;

    [Required(ErrorMessage = "Irk bilgisi zorunludur.")]
    [StringLength(60)]
    [Display(Name = "Irk")]
    public string Breed { get; set; } = string.Empty;

    [Required(ErrorMessage = "Doğum tarihi zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Doğum Tarihi")]
    public DateTime BirthDate { get; set; } = DateTime.Today;

    [Required(ErrorMessage = "Kilo bilgisi zorunludur.")]
    [Range(0.1, 200, ErrorMessage = "Kilo 0.1 ile 200 kg arasında olmalıdır.")]
    [Display(Name = "Kilo (kg)")]
    public decimal WeightKg { get; set; }

    public string? ExistingPhotoUrl { get; set; }

    [Display(Name = "Evcil Hayvan Fotoğrafı (İsteğe Bağlı)")]
    public IFormFile? PhotoFile { get; set; }
}