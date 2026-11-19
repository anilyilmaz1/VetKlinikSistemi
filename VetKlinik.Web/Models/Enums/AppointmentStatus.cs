using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Web.Models.Enums;

public enum AppointmentStatus
{
    [Display(Name = "Bekliyor")]
    Pending = 0,

    [Display(Name = "Onaylandı")]
    Approved = 1,

    [Display(Name = "Reddedildi")]
    Rejected = 2,

    [Display(Name = "Tamamlandı")]
    Completed = 3,

    [Display(Name = "İptal Edildi")]
    Cancelled = 4
}