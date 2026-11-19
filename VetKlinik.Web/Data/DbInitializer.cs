using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Web.Models.Entities;
using VetKlinik.Web.Models.Enums;

namespace VetKlinik.Web.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();

        string[] roles =["Admin", "Veterinarian", "PetOwner"];
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }  

        const string adminEmail = "b241200001@sakarya.edu.tr";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email =adminEmail,
                EmailConfirmed = true,
                FullName = "Anıl Yılmaz (Sistem Yöneticisi)",
                Address = "Serdivan, Sakarya" 
            };

            var createAdmin = await userManager.CreateAsync(adminUser, "sau");
            if (createAdmin.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "admin");
            }               
        }

        if (!await context.Clinics.AnyAsync())
        {
            var clinic = new Clinic
            {
                Name = "Sakarya Minik Dostlar Veteriner Kliniği",
                Address = "Kemalpaşa Mah. Üniversite Cad. No:42 Serdivan / Sakarya",
                Phone = "0264 555 01 01",
                OpeningTime = new TimeSpan(9, 0, 0),
                ClosingTime = new TimeSpan(18, 0, 0) 
            };
            context.Clinics.Add(clinic);
            await context.SaveChangesAsync();

            var services = new List<VetService>
            {
                new()
                {
                    ClinicId = clinic.Id,
                    Name = "Genel Sağlık Muayenesi",
                    Description = "Kapsamlı fiziksel muayene, ateş ve nabız kontrolü, beslenme danışmanlığı.",
                    DurationMinutes = 30,
                    Price = 450m
                },
                new()
                {
                    ClinicId = clinic.Id,
                    Name = "Karma Aşı Uygulaması",
                    Description = "Kedi ve köpekler için yıllık koruyucu karma aşı ve aşı karnesi kaydı.",
                    DurationMinutes = 20,
                    Price = 650m
                },
                new()
                {
                    ClinicId = clinic.Id,
                    Name = "Kuduz Aşısı ve Mikroçip",
                    Description = "Resmi kimliklendirme mikroçip uygulaması ve kuduz aşısı.",
                    DurationMinutes = 30,
                    Price = 750m
                },
                new()
                {
                    ClinicId = clinic.Id,
                    Name = "Diş Taşı Temizliği ve Ağız Bakımı",
                    Description = "Ultrasonik cihaz ile diş taşı temizliği ve diş eti sağlığı kontrolü.",
                    DurationMinutes = 45,
                    Price = 1200m
                },
                new()
                {
                    ClinicId = clinic.Id,
                    Name = "Detaylı Ultrason ve Kan Tahlili",
                    Description = "Tam kan sayımı (hemogram), biyokimya paneli ve karın bölgesi ultrasonu.",
                    DurationMinutes = 60,
                    Price = 1500m
                }
            };
            context.VetServices.AddRange(services);
            await context.SaveChangesAsync();

            var vet1User = new ApplicationUser
            {
                UserName = "vet.ayse@vetklinik.com",
                Email = "vet.ayse@vetklinik.com",
                EmailConfirmed = true,
                FullName = "Dr. Vet. Ayşe Demir",
                Address = "Serdivan, Sakarya"
            };
            await userManager.CreateAsync(vet1User, "sau");
            await userManager.AddToRoleAsync(vet1User, "Veterinarian");
            
            var vet2User = new ApplicationUser
            {
                UserName = "vet.mehmet@vetklinik.com",
                Email = "vet.mehmet@vetklinik.com",
                EmailConfirmed = true,
                FullName = "Dr. Vet. Mehmet Kaya",
                Address = "Adapazarı, Sakarya"
            };
            await userManager.CreateAsync(vet2User, "sau");
            await userManager.AddToRoleAsync(vet2User, "Veterinarian");
            
            var vet1 = new Veterinarian
            {
                UserId = vet1User.Id,
                ClinicId = clinic.Id,
                FullName = "Dr. Vet. Ayşe Demir",
                Specialization = "İç Hastalıkları ve Koruyucu Aşılama",
                Bio = "8 yıllık klinik deneyimiyle kedi ve köpek iç hastalıkları ile aşılama takibi alanında uzman."
            };

            var vet2 = new Veterinarian
            {
                UserId = vet2User.Id,
                ClinicId = clinic.Id,
                FullName = "Dr. Vet. Mehmet Kaya",
                Specialization = "Cerrahi, Diş Sağlığı ve Görüntüleme",
                Bio = "Veteriner cerrahi, ağız-diş sağlığı ve ultrasonografik tanı konularında uzman."
            };
            
            context.Veterinarians.AddRange(vet1, vet2);
            await context.SaveChangesAsync();

            context.VeterinarianServices.AddRange(
                new VeterinarianService { VeterinarianId = vet1.Id, VetServiceId = services[0].Id },
                new VeterinarianService { VeterinarianId = vet1.Id, VetServiceId = services[1].Id },
                new VeterinarianService { VeterinarianId = vet1.Id, VetServiceId = services[2].Id },
                new VeterinarianService { VeterinarianId = vet2.Id, VetServiceId = services[0].Id },
                new VeterinarianService { VeterinarianId = vet2.Id, VetServiceId = services[3].Id },
                new VeterinarianService { VeterinarianId = vet2.Id, VetServiceId = services[4].Id }
            );

            DayOfWeek[] workDays =
            [
                DayOfWeek.Monday,
                DayOfWeek.Tuesday,
                DayOfWeek.Wednesday,
                DayOfWeek.Thursday,
                DayOfWeek.Friday,
                DayOfWeek.Saturday
            ];

            foreach (var day in workDays)
            {
                context.VeterinarianAvailabilities.Add(new VeterinarianAvailability
                {
                    VeterinarianId = vet1.Id,
                    DayOfWeek = day,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(18, 0, 0),
                    IsAvailable = true
                });

                context.VeterinarianAvailabilities.Add(new VeterinarianAvailability
                {
                    VeterinarianId = vet2.Id,
                    DayOfWeek = day,
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(18, 0, 0),
                    IsAvailable = true
                });
            }

            var ownerUser = new ApplicationUser
            {
                UserName = "hasta@vetklinik.com",
                Email = "hasta@vetklinik.com",
                EmailConfirmed = true,
                FullName = "Zeynep Arslan",
                Address = "Mavi Durak, Serdivan / Sakarya"
            };
            await userManager.CreateAsync(ownerUser, "sau");
            await userManager.AddToRoleAsync(ownerUser, "PetOwner");

            var pet1 = new Pet
            {
                OwnerId = ownerUser.Id,
                Name = "Boncuk",
                Species = "Kedi",
                Breed = "Tekir",
                BirthDate = DateTime.Today.AddYears(-2),
                WeightKg = 4.3m
            };

            var pet2 = new Pet
            {
                OwnerId = ownerUser.Id,
                Name = "Karabaş",
                Species = "Köpek",
                Breed = "Golden Retriever",
                BirthDate = DateTime.Today.AddYears(-3),
                WeightKg = 28.5m
            };

            context.Pets.AddRange(pet1, pet2);
            await context.SaveChangesAsync();
            
            context.VaccinationRecords.Add(new VaccinationRecord
            {
                PetId = pet1.Id,
                VeterinarianId = vet1.Id,
                VaccineName = "Kedi Karma Aşısı (1. Doz)",
                AdministeredDate = DateTime.Today.AddMonths(-6),
                NextDueDate = DateTime.Today.AddMonths(6),
                Notes = "Aşı sonrası genel durumu gayet iyi."
            });

            context.Appointments.Add(new Appointment
            {
                PetId = pet2.Id,
                VeterinarianId = vet1.Id,
                VetServiceId = services[0].Id,
                AppointmentDate = DateTime.Today.AddDays(1),
                StartTime = new TimeSpan(10, 0, 0),
                EndTime = new TimeSpan(10, 30, 0),
                DurationMinutes = 30,
                Price = services[0].Price,
                Status = AppointmentStatus.Approved,
                OwnerNotes = "Yıllık rutin kontrol ve kilo takibi."
            });

            await context.SaveChangesAsync();



        }
    }

}