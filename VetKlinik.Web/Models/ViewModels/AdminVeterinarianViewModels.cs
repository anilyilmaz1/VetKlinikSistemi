using System.ComponentModel.DataAnnotations;

namespace VetKlinik.Web.Models.ViewModels;

public class ServiceCheckboxItem
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
    public decimal Price { get; set; }
    public bool IsSelected { get; set; }
}

public class AdminVeterinarianCreateViewModel
{
    [Required(ErrorMessage = "Hekim adı soyadı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Hekim Adı Soyadı")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta adresi zorunludur.")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz.")]
    [Display(Name = "Giriş E-posta Adresi")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur.")]
    [DataType(DataType.Password)]
    [Display(Name = "Giriş Şifresi")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
    [StringLength(150)]
    [Display(Name = "Uzmanlık Alanı")]
    public string Specialization { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Hakkında / Biyografi")]
    public string? Bio { get; set; }

    [StringLength(250)]
    [Display(Name = "Adres")]
    public string? Address { get; set; }

    public List<ServiceCheckboxItem> AvailableServices { get; set; } = new();
}

public class AdminVeterinarianEditViewModel
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Hekim adı soyadı zorunludur.")]
    [StringLength(100)]
    [Display(Name = "Hekim Adı Soyadı")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Uzmanlık alanı zorunludur.")]
    [StringLength(150)]
    [Display(Name = "Uzmanlık Alanı")]
    public string Specialization { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Hakkında / Biyografi")]
    public string? Bio { get; set; }

    public List<ServiceCheckboxItem> AvailableServices { get; set; } = new();
}