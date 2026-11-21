using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Web.Data;
using VetKlinik.Web.Models.Entities;
using VetKlinik.Web.Models.ViewModels;

namespace VetKlinik.Web.Controllers;

[Authorize]
public class PetController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWebHostEnvironment _env;

    public PetController(
        ApplicationDbContext context,
        UserManager<ApplicationUser> userManager,
        IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _env = env;
    }

    // GET: /Pet
    public async Task<IActionResult> Index()
    {
        var userId = _userManager.GetUserId(User);
        var pets = await _context.Pets
            .Where(p => p.OwnerId == userId)
            .OrderByDescending(p => p.Id)
            .ToListAsync();

        return View(pets);
    }

    // GET: /Pet/Create
    [HttpGet]
    public IActionResult Create()
    {
        return View(new PetViewModel());
    }

    // POST: /Pet/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PetViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var userId = _userManager.GetUserId(User);
        if (string.IsNullOrEmpty(userId))
            return Challenge();

        var photoUrl = await SavePhotoAsync(model.PhotoFile);

        var pet = new Pet
        {
            OwnerId = userId,
            Name = model.Name,
            Species = model.Species,
            Breed = model.Breed,
            BirthDate = model.BirthDate,
            WeightKg = model.WeightKg,
            PhotoUrl = photoUrl
        };

        _context.Pets.Add(pet);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{pet.Name} başarıyla eklendi.";
        return RedirectToAction(nameof(Index));
    }

    // GET: /Pet/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var userId = _userManager.GetUserId(User);
        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

        if (pet == null)
            return NotFound();

        var model = new PetViewModel
        {
            Id = pet.Id,
            Name = pet.Name,
            Species = pet.Species,
            Breed = pet.Breed,
            BirthDate = pet.BirthDate,
            WeightKg = pet.WeightKg,
            ExistingPhotoUrl = pet.PhotoUrl
        };

        return View(model);
    }

    // POST: /Pet/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, PetViewModel model)
    {
        if (id != model.Id)
            return BadRequest();

        if (!ModelState.IsValid)
            return View(model);

        var userId = _userManager.GetUserId(User);
        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

        if (pet == null)
            return NotFound();

        if (model.PhotoFile != null && model.PhotoFile.Length > 0)
        {
            DeletePhotoFile(pet.PhotoUrl);
            pet.PhotoUrl = await SavePhotoAsync(model.PhotoFile);
        }

        pet.Name = model.Name;
        pet.Species = model.Species;
        pet.Breed = model.Breed;
        pet.BirthDate = model.BirthDate;
        pet.WeightKg = model.WeightKg;

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = $"{pet.Name} bilgileri güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    // POST: /Pet/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = _userManager.GetUserId(User);
        var pet = await _context.Pets
            .FirstOrDefaultAsync(p => p.Id == id && p.OwnerId == userId);

        if (pet == null)
            return NotFound();

        try
        {
            var photoUrl = pet.PhotoUrl;
            _context.Pets.Remove(pet);
            await _context.SaveChangesAsync();

            DeletePhotoFile(photoUrl);
            TempData["SuccessMessage"] = $"{pet.Name} kaydı silindi.";
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "Bu evcil hayvana ait randevu veya aşı geçmişi bulunduğu için silinemez.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task<string?> SavePhotoAsync(IFormFile? photoFile)
    {
        if (photoFile == null || photoFile.Length == 0)
            return null;

        var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "pets");
        Directory.CreateDirectory(uploadsFolder);

        var extension = Path.GetExtension(photoFile.FileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using var stream = new FileStream(filePath, FileMode.Create);
        await photoFile.CopyToAsync(stream);

        return $"/uploads/pets/{fileName}";
    }

    private void DeletePhotoFile(string? photoUrl)
    {
        if (string.IsNullOrEmpty(photoUrl))
            return;

        var relativePath = photoUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.Combine(_env.WebRootPath, relativePath);

        if (System.IO.File.Exists(fullPath))
        {
            System.IO.File.Delete(fullPath);
        }
    }
}