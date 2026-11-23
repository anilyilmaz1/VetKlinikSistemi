using System.ComponentModel.DataAnnotations;
using VetKlinik.Web.Models.Entities;

namespace VetKlinik.Web.Models.ViewModels;

public class VetExamineViewModel
{
    public Appointment Appointment { get; set; } = null!;
    public List<VaccinationRecord> VaccinationHistory { get; set; } = new();

    [StringLength(1000, ErrorMessage = "Muayene notu en fazla 1000 karakter olabilir.")]
    [Display(Name = "Hekim Muayene Notu")]
    public string? ExaminationNotes { get; set; }

    public VetVaccinationInputModel NewVaccination { get; set; } = new();
}

public class VetVaccinationInputModel
{
    [Required]
    public int AppointmentId { get; set; }

    [Required(ErrorMessage = "Aşı adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Aşı adı en fazla 100 karakter olabilir.")]
    [Display(Name = "Aşı Adı")]
    public string VaccineName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yapıldığı tarih zorunludur.")]
    [DataType(DataType.Date)]
    [Display(Name = "Yapıldığı Tarih")]
    public DateTime AdministeredDate { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Bir Sonraki Doz Tarihi (Opsiyonel)")]
    public DateTime? NextDueDate { get; set; }

    [StringLength(300, ErrorMessage = "Not en fazla 300 karakter olabilir.")]
    [Display(Name = "Aşı Notu")]
    public string? Notes { get; set; }
}