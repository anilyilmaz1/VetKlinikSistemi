using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Web.Models.ViewModels;

public class VetVaccinationEditViewModel
{
    public int Id { get; set; }
    public int AppointmentId { get; set; }
    public string PetName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Aşı adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Aşı adı en fazla 100 karakter olabilir.")]
    [Display(Name = "Aşı Adı")]
    public string VaccineName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Uygulama tarihi zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Uygulama Tarihi")]
    public DateTime AdministeredDate { get; set; }

    [DataType(DataType.Date)]
    [Display(Name = "Sonraki Doz Tarihi (Opsiyonel)")]
    public DateTime? NextDueDate { get; set; }

    [StringLength(400, ErrorMessage = "Not en fazla 400 karakter olabilir.")]
    [Display(Name = "Aşı Notu")]
    public string? Notes { get; set; }
}