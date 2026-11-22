using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Web.Data;
using VetKlinik.Web.Models.Entities;
using VetKlinik.Web.Models.Enums;
using VetKlinik.Web.Models.ViewModels;

namespace VetKlinik.Web.Controllers;

[Authorize]
public class AppointmentController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public AppointmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: /Appointment
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Challenge();

        var hasPets = await _context.Pets.AnyAsync(p => p.OwnerId == userId);
        ViewBag.HasPets = hasPets;

        var appointments = await _context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
                .ThenInclude(v => v!.Clinic)
            .Include(a => a.VetService)
            .Where(a => a.Pet != null && a.Pet.OwnerId == userId)
            .OrderByDescending(a => a.AppointmentDate)
            .ThenByDescending(a => a.StartTime)
            .ToListAsync();

        return View(appointments);
    }

    // GET: /Appointment/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var userId = _userManager.GetUserId(User);

        var appointment = await _context.Appointments
            .Include(a => a.Pet)
            .Include(a => a.Veterinarian)
                .ThenInclude(v => v!.Clinic)
            .Include(a => a.VetService)
            .FirstOrDefaultAsync(a => a.Id == id && a.Pet != null && a.Pet.OwnerId == userId);

        if (appointment == null)
            return NotFound();

        return View(appointment);
    }

    // GET: /Appointment/Create
    public async Task<IActionResult> Create(int? petId)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Challenge();

        var userPets = await _context.Pets
            .Where(p => p.OwnerId == userId)
            .OrderBy(p => p.Name)
            .ToListAsync();

        if (!userPets.Any())
        {
            TempData["WarningMessage"] = "Randevu oluşturabilmek için önce en az bir evcil hayvan eklemelisiniz.";
            return RedirectToAction("Create", "Pet");
        }

        var model = new AppointmentCreateViewModel
        {
            PetId = petId ?? userPets.First().Id,
            AppointmentDate = DateTime.Today.AddDays(1)
        };

        await PopulateDropdownsAsync(model, userId);
        return View(model);
    }

    // POST: /Appointment/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AppointmentCreateViewModel model)
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Challenge();

        // Evcil hayvan sahiplik kontrolü
        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => p.Id == model.PetId && p.OwnerId == userId);

        if (pet == null)
        {
            ModelState.AddModelError(nameof(model.PetId), "Geçersiz evcil hayvan seçimi.");
        }

        // Tarih kontrolü
        if (model.AppointmentDate.Date < DateTime.Today)
        {
            ModelState.AddModelError(nameof(model.AppointmentDate), "Geçmiş bir tarihe randevu alınamaz.");
        }
        else if (model.AppointmentDate.Date == DateTime.Today && model.StartTime <= DateTime.Now.TimeOfDay)
        {
            ModelState.AddModelError(nameof(model.StartTime), "Bugün için geçmiş bir saat seçilemez.");
        }

        // Hekim ve hizmet eşleşme kontrolü
        var vetServiceLink = await _context.VeterinarianServices
            .Include(vs => vs.VetService)
            .FirstOrDefaultAsync(vs => vs.VeterinarianId == model.VeterinarianId && vs.VetServiceId == model.VetServiceId);

        if (vetServiceLink == null || vetServiceLink.VetService == null)
        {
            ModelState.AddModelError(nameof(model.VetServiceId), "Seçilen veteriner hekim bu hizmeti vermemektedir.");
        }

        if (ModelState.IsValid && vetServiceLink?.VetService != null)
        {
            var service = vetServiceLink.VetService;
            var endTime = model.StartTime.Add(TimeSpan.FromMinutes(service.DurationMinutes));

            // Hekimin ilgili gündeki çalışma saati kontrolü
            var availability = await _context.VeterinarianAvailabilities
                .FirstOrDefaultAsync(va =>
                    va.VeterinarianId == model.VeterinarianId &&
                    va.DayOfWeek == model.AppointmentDate.DayOfWeek &&
                    va.IsAvailable);

            if (availability == null || model.StartTime < availability.StartTime || endTime > availability.EndTime)
            {
                ModelState.AddModelError(nameof(model.StartTime), "Seçilen saat dilimi veteriner hekimin çalışma saatleri dışındadır.");
            }
            else
            {
                // Çakışan aktif randevu kontrolü
                var hasConflict = await _context.Appointments
                    .AnyAsync(a =>
                        a.VeterinarianId == model.VeterinarianId &&
                        a.AppointmentDate.Date == model.AppointmentDate.Date &&
                        a.Status != AppointmentStatus.Cancelled &&
                        a.Status != AppointmentStatus.Rejected &&
                        model.StartTime < a.EndTime &&
                        endTime > a.StartTime);

                if (hasConflict)
                {
                    ModelState.AddModelError(nameof(model.StartTime), "Seçilen saat dilimi az önce doldu. Lütfen başka bir saat seçiniz.");
                }
                else
                {
                    var appointment = new Appointment
                    {
                        PetId = model.PetId,
                        VeterinarianId = model.VeterinarianId,
                        VetServiceId = model.VetServiceId,
                        AppointmentDate = model.AppointmentDate.Date,
                        StartTime = model.StartTime,
                        EndTime = endTime,
                        DurationMinutes = service.DurationMinutes,
                        Price = service.Price,
                        Status = AppointmentStatus.Pending,
                        OwnerNotes = model.OwnerNotes?.Trim(),
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Appointments.Add(appointment);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = "Randevu talebiniz başarıyla oluşturuldu. Hekim onayı bekleniyor.";
                    return RedirectToAction(nameof(Index));
                }
            }
        }

        await PopulateDropdownsAsync(model, userId);
        return View(model);
    }

    // POST: /Appointment/Cancel/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = _userManager.GetUserId(User);

        var appointment = await _context.Appointments
            .Include(a => a.Pet)
            .FirstOrDefaultAsync(a => a.Id == id && a.Pet != null && a.Pet.OwnerId == userId);

        if (appointment == null)
            return NotFound();

        if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Approved)
        {
            TempData["ErrorMessage"] = "Yalnızca bekleyen veya onaylanmış randevular iptal edilebilir.";
            return RedirectToAction(nameof(Index));
        }

        appointment.Status = AppointmentStatus.Cancelled;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Randevunuz iptal edildi.";
        return RedirectToAction(nameof(Index));
    }

    // AJAX: Seçilen veterinerin verdiği hizmetleri döner
    [HttpGet]
    public async Task<IActionResult> GetServicesByVeterinarian(int veterinarianId)
    {
        var services = await _context.VeterinarianServices
            .Where(vs => vs.VeterinarianId == veterinarianId && vs.VetService != null)
            .Select(vs => new
            {
                id = vs.VetService!.Id,
                name = vs.VetService.Name,
                durationMinutes = vs.VetService.DurationMinutes,
                price = vs.VetService.Price
            })
            .OrderBy(s => s.name)
            .ToListAsync();

        return Json(services);
    }

    // AJAX: Seçilen veteriner, hizmet ve tarih için müsait saat dilimlerini döner
    [HttpGet]
    public async Task<IActionResult> GetAvailableSlots(int veterinarianId, int vetServiceId, DateTime date)
    {
        if (date.Date < DateTime.Today)
            return Json(Array.Empty<object>());

        var vetServiceLink = await _context.VeterinarianServices
            .Include(vs => vs.VetService)
            .FirstOrDefaultAsync(vs => vs.VeterinarianId == veterinarianId && vs.VetServiceId == vetServiceId);

        if (vetServiceLink?.VetService == null)
            return Json(Array.Empty<object>());

        var availability = await _context.VeterinarianAvailabilities
            .FirstOrDefaultAsync(va =>
                va.VeterinarianId == veterinarianId &&
                va.DayOfWeek == date.DayOfWeek &&
                va.IsAvailable);

        if (availability == null)
            return Json(Array.Empty<object>());

        var duration = TimeSpan.FromMinutes(vetServiceLink.VetService.DurationMinutes);
        var step = TimeSpan.FromMinutes(30);

        var existingAppointments = await _context.Appointments
            .Where(a =>
                a.VeterinarianId == veterinarianId &&
                a.AppointmentDate.Date == date.Date &&
                a.Status != AppointmentStatus.Cancelled &&
                a.Status != AppointmentStatus.Rejected)
            .Select(a => new { a.StartTime, a.EndTime })
            .ToListAsync();

        var slots = new List<object>();
        var current = availability.StartTime;
        var nowTime = DateTime.Now.TimeOfDay;

        while (current.Add(duration) <= availability.EndTime)
        {
            var slotEnd = current.Add(duration);
            var isPastToday = date.Date == DateTime.Today && current <= nowTime;
            var isConflict = existingAppointments.Any(a => current < a.EndTime && slotEnd > a.StartTime);

            if (!isPastToday && !isConflict)
            {
                slots.Add(new
                {
                    value = current.ToString(@"hh\:mm\:ss"),
                    text = $"{current:hh\\:mm} - {slotEnd:hh\\:mm}"
                });
            }

            current = current.Add(step);
        }

        return Json(slots);
    }

    private async Task PopulateDropdownsAsync(AppointmentCreateViewModel model, string userId)
    {
        model.Pets = await _context.Pets
            .Where(p => p.OwnerId == userId)
            .OrderBy(p => p.Name)
            .Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = $"{p.Name} ({p.Species} - {p.Breed})"
            })
            .ToListAsync();

        model.Veterinarians = await _context.Veterinarians
            .Include(v => v.Clinic)
            .OrderBy(v => v.FullName)
            .Select(v => new SelectListItem
            {
                Value = v.Id.ToString(),
                Text = $"{v.FullName} — {v.Specialization} ({v.Clinic!.Name})"
            })
            .ToListAsync();
    }
}