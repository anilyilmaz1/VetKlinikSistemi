using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Web.Data;
using VetKlinik.Web.Models.Entities;
using VetKlinik.Web.Models.Enums;
using VetKlinik.Web.Models.ViewModels;

namespace VetKlinik.Web.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Admin
    public async Task<IActionResult> Index(AppointmentStatus? status)
    {
        var clinic = await _context.Clinics.FirstOrDefaultAsync();

        var appointmentsQuery = _context.Appointments
            .Include(a => a.Pet)
                .ThenInclude(p => p!.Owner)
            .Include(a => a.Veterinarian)
            .Include(a => a.VetService)
            .AsQueryable();

        if (status.HasValue)
        {
            appointmentsQuery = appointmentsQuery.Where(a => a.Status == status.Value);
        }

        var model = new AdminDashboardViewModel
        {
            Clinic = clinic,
            TotalVeterinarians = await _context.Veterinarians.CountAsync(),
            TotalServices = await _context.VetServices.CountAsync(),
            TotalPets = await _context.Pets.CountAsync(),
            TotalAppointments = await _context.Appointments.CountAsync(),
            PendingAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Pending),
            ApprovedAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Approved),
            CompletedAppointments = await _context.Appointments.CountAsync(a => a.Status == AppointmentStatus.Completed),
            CompletedRevenue = await _context.Appointments
                .Where(a => a.Status == AppointmentStatus.Completed)
                .SumAsync(a => (decimal?)a.Price) ?? 0m,
            SelectedStatus = status,
            Appointments = await appointmentsQuery
                .OrderByDescending(a => a.AppointmentDate)
                .ThenBy(a => a.StartTime)
                .ToListAsync()
        };

        return View(model);
    }

    // GET: /Admin/ClinicSettings
    public async Task<IActionResult> ClinicSettings()
    {
        var clinic = await _context.Clinics.FirstOrDefaultAsync();
        if (clinic == null)
        {
            clinic = new Clinic();
        }
        return View(clinic);
    }

    // POST: /Admin/ClinicSettings
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ClinicSettings(Clinic model)
    {
        if (model.OpeningTime >= model.ClosingTime)
        {
            ModelState.AddModelError(nameof(model.ClosingTime), "Kapanış saati açılış saatinden sonra olmalıdır.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var clinic = await _context.Clinics.FirstOrDefaultAsync();
        if (clinic == null)
        {
            _context.Clinics.Add(model);
        }
        else
        {
            clinic.Name = model.Name.Trim();
            clinic.Address = model.Address.Trim();
            clinic.Phone = model.Phone.Trim();
            clinic.OpeningTime = model.OpeningTime;
            clinic.ClosingTime = model.ClosingTime;
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Klinik bilgileri ve çalışma saatleri güncellendi.";
        return RedirectToAction(nameof(ClinicSettings));
    }

    // GET: /Admin/Services
    public async Task<IActionResult> Services()
    {
        var services = await _context.VetServices
            .Include(s => s.VeterinarianServices)
            .Include(s => s.Appointments)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return View(services);
    }

    // GET: /Admin/CreateService
    public IActionResult CreateService()
    {
        return View(new AdminServiceViewModel());
    }

    // POST: /Admin/CreateService
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateService(AdminServiceViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var clinic = await _context.Clinics.FirstOrDefaultAsync();
        if (clinic == null)
        {
            TempData["ErrorMessage"] = "Önce klinik bilgilerini tanımlamalısınız.";
            return RedirectToAction(nameof(ClinicSettings));
        }

        var service = new VetService
        {
            ClinicId = clinic.Id,
            Name = model.Name.Trim(),
            Description = model.Description?.Trim(),
            DurationMinutes = model.DurationMinutes,
            Price = model.Price
        };

        _context.VetServices.Add(service);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"'{service.Name}' hizmeti başarıyla eklendi.";
        return RedirectToAction(nameof(Services));
    }

    // GET: /Admin/EditService/5
    public async Task<IActionResult> EditService(int id)
    {
        var service = await _context.VetServices.FindAsync(id);
        if (service == null)
            return NotFound();

        var model = new AdminServiceViewModel
        {
            Id = service.Id,
            Name = service.Name,
            Description = service.Description,
            DurationMinutes = service.DurationMinutes,
            Price = service.Price
        };

        return View(model);
    }

    // POST: /Admin/EditService/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditService(int id, AdminServiceViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var service = await _context.VetServices.FindAsync(id);
        if (service == null)
            return NotFound();

        service.Name = model.Name.Trim();
        service.Description = model.Description?.Trim();
        service.DurationMinutes = model.DurationMinutes;
        service.Price = model.Price;

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"'{service.Name}' hizmeti güncellendi.";
        return RedirectToAction(nameof(Services));
    }

    // POST: /Admin/DeleteService/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteService(int id)
    {
        var service = await _context.VetServices
            .Include(s => s.Appointments)
            .Include(s => s.VeterinarianServices)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (service == null)
            return NotFound();

        if (service.Appointments.Any())
        {
            TempData["ErrorMessage"] = $"'{service.Name}' hizmetine bağlı {service.Appointments.Count} adet randevu kaydı bulunduğu için silinemez.";
            return RedirectToAction(nameof(Services));
        }

        _context.VeterinarianServices.RemoveRange(service.VeterinarianServices);
        _context.VetServices.Remove(service);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"'{service.Name}' hizmeti silindi.";
        return RedirectToAction(nameof(Services));
    }

    // GET: /Admin/Veterinarians
    public async Task<IActionResult> Veterinarians()
    {
        var vets = await _context.Veterinarians
            .Include(v => v.User)
            .Include(v => v.VeterinarianServices)
                .ThenInclude(vs => vs.VetService)
            .Include(v => v.Appointments)
            .OrderBy(v => v.FullName)
            .ToListAsync();

        return View(vets);
    }

    // GET: /Admin/CreateVeterinarian
    public async Task<IActionResult> CreateVeterinarian()
    {
        var services = await _context.VetServices.OrderBy(s => s.Name).ToListAsync();
        var model = new AdminVeterinarianCreateViewModel
        {
            AvailableServices = services.Select(s => new ServiceCheckboxItem
            {
                ServiceId = s.Id,
                ServiceName = s.Name,
                DurationMinutes = s.DurationMinutes,
                Price = s.Price,
                IsSelected = false
            }).ToList()
        };

        return View(model);
    }

    // POST: /Admin/CreateVeterinarian
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateVeterinarian(AdminVeterinarianCreateViewModel model)
    {
        var clinic = await _context.Clinics.FirstOrDefaultAsync();
        if (clinic == null)
        {
            TempData["ErrorMessage"] = "Önce klinik bilgilerini tanımlamalısınız.";
            return RedirectToAction(nameof(ClinicSettings));
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var existingUser = await _userManager.FindByEmailAsync(model.Email.Trim());
        if (existingUser != null)
        {
            ModelState.AddModelError(nameof(model.Email), "Bu e-posta adresi sistemde zaten kayıtlı.");
            return View(model);
        }

        var user = new ApplicationUser
        {
            UserName = model.Email.Trim(),
            Email = model.Email.Trim(),
            EmailConfirmed = true,
            FullName = model.FullName.Trim(),
            Address = model.Address?.Trim()
        };

        var createResult = await _userManager.CreateAsync(user, model.Password);
        if (!createResult.Succeeded)
        {
            foreach (var error in createResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }

        await _userManager.AddToRoleAsync(user, "Veterinarian");

        var vet = new Veterinarian
        {
            UserId = user.Id,
            ClinicId = clinic.Id,
            FullName = model.FullName.Trim(),
            Specialization = model.Specialization.Trim(),
            Bio = model.Bio?.Trim()
        };

        _context.Veterinarians.Add(vet);
        await _context.SaveChangesAsync();

        // Seçilen hizmetleri hekime ata
        var selectedServiceIds = model.AvailableServices
            .Where(s => s.IsSelected)
            .Select(s => s.ServiceId)
            .ToList();

        foreach (var serviceId in selectedServiceIds)
        {
            _context.VeterinarianServices.Add(new VeterinarianService
            {
                VeterinarianId = vet.Id,
                VetServiceId = serviceId
            });
        }

        // Varsayılan çalışma saatleri (Pazartesi - Cumartesi)
        DayOfWeek[] defaultWorkDays =
        [
            DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday,
            DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday
        ];

        foreach (var day in defaultWorkDays)
        {
            _context.VeterinarianAvailabilities.Add(new VeterinarianAvailability
            {
                VeterinarianId = vet.Id,
                DayOfWeek = day,
                StartTime = clinic.OpeningTime,
                EndTime = clinic.ClosingTime,
                IsAvailable = true
            });
        }

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{vet.FullName} sisteme veteriner hekim olarak eklendi.";
        return RedirectToAction(nameof(Veterinarians));
    }

    // GET: /Admin/EditVeterinarian/5
    public async Task<IActionResult> EditVeterinarian(int id)
    {
        var vet = await _context.Veterinarians
            .Include(v => v.User)
            .Include(v => v.VeterinarianServices)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vet == null)
            return NotFound();

        var allServices = await _context.VetServices.OrderBy(s => s.Name).ToListAsync();
        var assignedIds = vet.VeterinarianServices.Select(vs => vs.VetServiceId).ToHashSet();

        var model = new AdminVeterinarianEditViewModel
        {
            Id = vet.Id,
            Email = vet.User?.Email ?? "-",
            FullName = vet.FullName,
            Specialization = vet.Specialization,
            Bio = vet.Bio,
            AvailableServices = allServices.Select(s => new ServiceCheckboxItem
            {
                ServiceId = s.Id,
                ServiceName = s.Name,
                DurationMinutes = s.DurationMinutes,
                Price = s.Price,
                IsSelected = assignedIds.Contains(s.Id)
            }).ToList()
        };

        return View(model);
    }

    // POST: /Admin/EditVeterinarian/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditVeterinarian(int id, AdminVeterinarianEditViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var vet = await _context.Veterinarians
            .Include(v => v.User)
            .Include(v => v.VeterinarianServices)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vet == null)
            return NotFound();

        vet.FullName = model.FullName.Trim();
        vet.Specialization = model.Specialization.Trim();
        vet.Bio = model.Bio?.Trim();

        if (vet.User != null)
        {
            vet.User.FullName = vet.FullName;
        }

        // Hizmet eşleştirmelerini güncelle
        _context.VeterinarianServices.RemoveRange(vet.VeterinarianServices);

        var selectedIds = model.AvailableServices
            .Where(s => s.IsSelected)
            .Select(s => s.ServiceId)
            .ToList();

        foreach (var serviceId in selectedIds)
        {
            _context.VeterinarianServices.Add(new VeterinarianService
            {
                VeterinarianId = vet.Id,
                VetServiceId = serviceId
            });
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = $"{vet.FullName} bilgileri ve hizmet atamaları güncellendi.";
        return RedirectToAction(nameof(Veterinarians));
    }
}