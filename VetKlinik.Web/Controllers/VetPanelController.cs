using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Web.Data;
using VetKlinik.Web.Models.Entities;
using VetKlinik.Web.Models.Enums;
using VetKlinik.Web.Models.ViewModels;

namespace VetKlinik.Web.Controllers;

[Authorize(Roles = "Veterinarian")]
public class VetPanelController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public VetPanelController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    private async Task<Veterinarian?> GetCurrentVeterinarianAsync()
    {
        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return null;

        return await _context.Veterinarians
            .Include(v => v.Clinic)
            .FirstOrDefaultAsync(v => v.UserId == userId);
    }

    // GET: /VetPanel
    public async Task<IActionResult> Index(AppointmentStatus? status)
    {
        var vet = await GetCurrentVeterinarianAsync();
        if (vet == null)
            return Forbid();

        ViewBag.VeterinarianName = vet.FullName;
        ViewBag.ClinicName = vet.Clinic?.Name;
        ViewBag.SelectedStatus = status;

        var allVetAppointments = _context.Appointments
            .Where(a => a.VeterinarianId == vet.Id);

        ViewBag.PendingCount = await allVetAppointments.CountAsync(a => a.Status == AppointmentStatus.Pending);
        ViewBag.TodayCount = await allVetAppointments.CountAsync(a =>
            a.AppointmentDate.Date == DateTime.Today &&
            (a.Status == AppointmentStatus.Pending || a.Status == AppointmentStatus.Approved));
        ViewBag.ApprovedCount = await allVetAppointments.CountAsync(a => a.Status == AppointmentStatus.Approved);
        ViewBag.CompletedCount = await allVetAppointments.CountAsync(a => a.Status == AppointmentStatus.Completed);

        var query = allVetAppointments
            .Include(a => a.Pet)
                .ThenInclude(p => p!.Owner)
            .Include(a => a.VetService)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        var appointments = await query
            .OrderByDescending(a => a.AppointmentDate)
            .ThenBy(a => a.StartTime)
            .ToListAsync();

        return View(appointments);
    }

    // POST: /VetPanel/Approve/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var vet = await GetCurrentVeterinarianAsync();
        if (vet == null)
            return Forbid();

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id && a.VeterinarianId == vet.Id);

        if (appointment == null)
            return NotFound();

        if (appointment.Status != AppointmentStatus.Pending)
        {
            TempData["ErrorMessage"] = "Yalnızca bekleyen randevular onaylanabilir.";
            return RedirectToAction(nameof(Index));
        }

        appointment.Status = AppointmentStatus.Approved;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Randevu onaylandı.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /VetPanel/Reject/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id)
    {
        var vet = await GetCurrentVeterinarianAsync();
        if (vet == null)
            return Forbid();

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == id && a.VeterinarianId == vet.Id);

        if (appointment == null)
            return NotFound();

        if (appointment.Status != AppointmentStatus.Pending && appointment.Status != AppointmentStatus.Approved)
        {
            TempData["ErrorMessage"] = "Bu durumdaki bir randevu reddedilemez.";
            return RedirectToAction(nameof(Index));
        }

        appointment.Status = AppointmentStatus.Rejected;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Randevu reddedildi.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /VetPanel/Examine/5
    public async Task<IActionResult> Examine(int id)
    {
        var vet = await GetCurrentVeterinarianAsync();
        if (vet == null)
            return Forbid();

        var appointment = await _context.Appointments
            .Include(a => a.Pet)
                .ThenInclude(p => p!.Owner)
            .Include(a => a.VetService)
            .FirstOrDefaultAsync(a => a.Id == id && a.VeterinarianId == vet.Id);

        if (appointment == null)
            return NotFound();

        var vaccinations = await _context.VaccinationRecords
            .Include(vr => vr.Veterinarian)
            .Where(vr => vr.PetId == appointment.PetId)
            .OrderByDescending(vr => vr.AdministeredDate)
            .ToListAsync();

        var model = new VetExamineViewModel
        {
            Appointment = appointment,
            VaccinationHistory = vaccinations,
            ExaminationNotes = appointment.ExaminationNotes,
            NewVaccination = new VetVaccinationInputModel
            {
                AppointmentId = appointment.Id,
                AdministeredDate = DateTime.Today
            }
        };

        return View(model);
    }

    // POST: /VetPanel/CompleteExamination
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteExamination(int appointmentId, string? examinationNotes)
    {
        var vet = await GetCurrentVeterinarianAsync();
        if (vet == null)
            return Forbid();

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == appointmentId && a.VeterinarianId == vet.Id);

        if (appointment == null)
            return NotFound();

        if (appointment.Status == AppointmentStatus.Cancelled || appointment.Status == AppointmentStatus.Rejected)
        {
            TempData["ErrorMessage"] = "İptal edilmiş veya reddedilmiş bir randevu tamamlanamaz.";
            return RedirectToAction(nameof(Index));
        }

        if (!string.IsNullOrEmpty(examinationNotes) && examinationNotes.Length > 1000)
        {
            TempData["ErrorMessage"] = "Muayene notu en fazla 1000 karakter olabilir.";
            return RedirectToAction(nameof(Examine), new { id = appointmentId });
        }

        appointment.ExaminationNotes = examinationNotes?.Trim();
        appointment.Status = AppointmentStatus.Completed;
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Muayene notu kaydedildi ve randevu tamamlandı olarak işaretlendi.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /VetPanel/AddVaccination
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddVaccination([Bind(Prefix = "NewVaccination")] VetVaccinationInputModel input)
    {
        var vet = await GetCurrentVeterinarianAsync();
        if (vet == null)
            return Forbid();

        var appointment = await _context.Appointments
            .FirstOrDefaultAsync(a => a.Id == input.AppointmentId && a.VeterinarianId == vet.Id);

        if (appointment == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "Lütfen aşı bilgilerini eksiksiz ve geçerli giriniz.";
            return RedirectToAction(nameof(Examine), new { id = input.AppointmentId });
        }

        var record = new VaccinationRecord
        {
            PetId = appointment.PetId,
            VeterinarianId = vet.Id,
            VaccineName = input.VaccineName.Trim(),
            AdministeredDate = input.AdministeredDate.Date,
            NextDueDate = input.NextDueDate?.Date,
            Notes = input.Notes?.Trim()
        };

        _context.VaccinationRecords.Add(record);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"'{record.VaccineName}' aşı kaydı evcil hayvanın karnesine eklendi.";
        return RedirectToAction(nameof(Examine), new { id = input.AppointmentId });
    }

    // GET: /VetPanel/Availabilities
    public async Task<IActionResult> Availabilities()
    {
        var vet = await GetCurrentVeterinarianAsync();
        if (vet == null)
            return Forbid();

        var existing = await _context.VeterinarianAvailabilities
            .Where(va => va.VeterinarianId == vet.Id)
            .ToListAsync();

        DayOfWeek[] orderedDays =
        [
            DayOfWeek.Monday,
            DayOfWeek.Tuesday,
            DayOfWeek.Wednesday,
            DayOfWeek.Thursday,
            DayOfWeek.Friday,
            DayOfWeek.Saturday,
            DayOfWeek.Sunday
        ];

        var model = orderedDays.Select(day =>
        {
            var found = existing.FirstOrDefault(x => x.DayOfWeek == day);
            return new VetAvailabilityItemViewModel
            {
                Id = found?.Id ?? 0,
                DayOfWeek = day,
                DayName = GetTurkishDayName(day),
                IsAvailable = found?.IsAvailable ?? false,
                StartTime = found?.StartTime ?? new TimeSpan(9, 0, 0),
                EndTime = found?.EndTime ?? new TimeSpan(18, 0, 0)
            };
        }).ToList();

        return View(model);
    }

    // POST: /VetPanel/Availabilities
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Availabilities(List<VetAvailabilityItemViewModel> model)
    {
        var vet = await GetCurrentVeterinarianAsync();
        if (vet == null)
            return Forbid();

        foreach (var item in model)
        {
            if (item.IsAvailable && item.StartTime >= item.EndTime)
            {
                ModelState.AddModelError(string.Empty, $"{GetTurkishDayName(item.DayOfWeek)} günü için bitiş saati başlangıç saatinden sonra olmalıdır.");
            }
        }

        if (!ModelState.IsValid)
        {
            foreach (var item in model)
            {
                item.DayName = GetTurkishDayName(item.DayOfWeek);
            }
            return View(model);
        }

        var existingList = await _context.VeterinarianAvailabilities
            .Where(va => va.VeterinarianId == vet.Id)
            .ToListAsync();

        foreach (var item in model)
        {
            var existing = existingList.FirstOrDefault(x => x.DayOfWeek == item.DayOfWeek);
            if (existing != null)
            {
                existing.IsAvailable = item.IsAvailable;
                existing.StartTime = item.StartTime;
                existing.EndTime = item.EndTime;
            }
            else
            {
                _context.VeterinarianAvailabilities.Add(new VeterinarianAvailability
                {
                    VeterinarianId = vet.Id,
                    DayOfWeek = item.DayOfWeek,
                    IsAvailable = item.IsAvailable,
                    StartTime = item.StartTime,
                    EndTime = item.EndTime
                });
            }
        }

        await _context.SaveChangesAsync();
        TempData["SuccessMessage"] = "Haftalık çalışma saatleriniz başarıyla güncellendi.";
        return RedirectToAction(nameof(Availabilities));
    }

    private static string GetTurkishDayName(DayOfWeek day) => day switch
    {
        DayOfWeek.Monday => "Pazartesi",
        DayOfWeek.Tuesday => "Salı",
        DayOfWeek.Wednesday => "Çarşamba",
        DayOfWeek.Thursday => "Perşembe",
        DayOfWeek.Friday => "Cuma",
        DayOfWeek.Saturday => "Cumartesi",
        DayOfWeek.Sunday => "Pazar",
        _ => day.ToString()
    };
}