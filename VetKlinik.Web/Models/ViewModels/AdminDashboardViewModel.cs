using VetKlinik.Web.Models.Entities;
using VetKlinik.Web.Models.Enums;

namespace VetKlinik.Web.Models.ViewModels;

public class AdminDashboardViewModel
{
    public Clinic? Clinic { get; set; }
    public int TotalVeterinarians { get; set; }
    public int TotalServices { get; set; }
    public int TotalPets { get; set; }
    public int TotalAppointments { get; set; }
    public int PendingAppointments { get; set; }
    public int ApprovedAppointments { get; set; }
    public int CompletedAppointments { get; set; }
    public decimal CompletedRevenue { get; set; }

    public AppointmentStatus? SelectedStatus { get; set; }
    public List<Appointment> Appointments { get; set; } = new();
}