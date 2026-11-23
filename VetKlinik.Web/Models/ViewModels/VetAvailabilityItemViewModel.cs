using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Web.Models.ViewModels;

public class VetAvailabilityItemViewModel
{
    public int Id {get; set;}
    public DayOfWeek DayOfWeek {get; set;}
    public string DayName {get; set;} = string.Empty;
    public bool IsAvailable {get; set;}

    [Required(ErrorMessage = "Başlangıç saati zorunludur.")]
    [DataType(DataType.Time)]
    public TimeSpan StartTime {get; set;} = new TimeSpan(9,0,0);

    [Required(ErrorMessage = "Bitiş saati zorunludur.")]
    [DataType(DataType.Time)]
    public TimeSpan EndTime {get; set;} = new TimeSpan(18,0,0);
}
