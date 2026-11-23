using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Web.Models.ViewModels;

public class AdminServiceViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Hizmet adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Hizmet adı en fazla 100 karakter olabilir.")]
    [Display(Name = "Hizmet Adı")]
    public string Name { get; set; } = string.Empty;

    [StringLength(400, ErrorMessage = "Açıklama en fazla 400 karakter olabilir.")]
    [Display(Name = "Açıklama")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Süre zorunludur.")]
    [Range(10, 480, ErrorMessage = "Süre 10 ile 480 dakika arasında olmalıdır.")]
    [Display(Name = "Süre (Dakika)")]
    public int DurationMinutes { get; set; } = 30;

    [Required(ErrorMessage = "Ücret zorunludur.")]
    [Range(0, 100000, ErrorMessage = "Geçerli bir ücret giriniz.")]
    [Display(Name = "Ücret (₺)")]
    public decimal Price { get; set; }
}