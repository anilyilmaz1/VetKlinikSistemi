namespace VetKlinik.Web.Models.Entities;

public class VeterinarianService
{
    public int VeterinarianId { get; set; }
    public Veterinarian? Veterinarian { get; set; }

    public int VetServiceId { get; set; }
    public VetService? VetService { get; set; }
}