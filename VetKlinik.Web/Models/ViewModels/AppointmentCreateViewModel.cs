using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace VetKlinik.Web.Models.ViewModels;

public class AppointmentCreateViewModel
{
    [Required(ErrorMessage = "Lütfen bir evcil hayvan seçiniz.")]
    [Display(Name = "Evcil Hayvan")]
    public int PetId { get; set; }

    [Required(ErrorMessage = "Lütfen bir veteriner hekim seçiniz.")]
    [Display(Name = "Veteriner Hekim")]
    public int VeterinarianId { get; set; }

    [Required(ErrorMessage = "Lütfen bir hizmet seçiniz.")]
    [Display(Name = "Hizmet")]
    public int VetServiceId { get; set; }

    [Required(ErrorMessage = "Randevu tarihi zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Randevu Tarihi")]
    public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

    [Required(ErrorMessage = "Lütfen müsait bir saat dilimi seçiniz.")]
    [Display(Name = "Randevu Saati")]
    public TimeSpan StartTime { get; set; }

    [StringLength(400, ErrorMessage = "Not en fazla 400 karakter olabilir.")]
    [Display(Name = "Hekime İletmek İstediğiniz Not (Opsiyonel)")]
    public string? OwnerNotes { get; set; }

    public IEnumerable<SelectListItem> Pets { get; set; } = new List<SelectListItem>();
    public IEnumerable<SelectListItem> Veterinarians { get; set; } = new List<SelectListItem>();
}