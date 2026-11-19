using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using VetKlinik.Web.Models.Entities;

namespace VetKlinik.Web.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<VetService> VetServices => Set<VetService>();
    public DbSet<Veterinarian> Veterinarians => Set<Veterinarian>();
    public DbSet<VeterinarianService> VeterinarianServices => Set<VeterinarianService>();
    public DbSet<VeterinarianAvailability> VeterinarianAvailabilities => Set<VeterinarianAvailability>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<VaccinationRecord> VaccinationRecords => Set<VaccinationRecord>();
    public DbSet<Appointment> Appointments => Set<Appointment>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<VeterinarianService>()
            .HasKey(vs => new { vs.VeterinarianId, vs.VetServiceId });

        builder.Entity<VeterinarianService>()
            .HasOne(vs => vs.Veterinarian)
            .WithMany(v => v.VeterinarianServices)
            .HasForeignKey(vs => vs.VeterinarianId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<VeterinarianService>()
            .HasOne(vs => vs.VetService)
            .WithMany(s => s.VeterinarianServices)
            .HasForeignKey(vs => vs.VetServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Veterinarian>()
            .HasOne(v => v.Clinic)
            .WithMany(c => c.Veterinarians)
            .HasForeignKey(v => v.ClinicId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Veterinarian>()
            .HasOne(v => v.User)
            .WithOne(u => u.VeterinarianProfile)
            .HasForeignKey<Veterinarian>(v => v.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasOne(a => a.Pet)
            .WithMany(p => p.Appointments)
            .HasForeignKey(a => a.PetId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasOne(a => a.Veterinarian)
            .WithMany(v => v.Appointments)
            .HasForeignKey(a => a.VeterinarianId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Appointment>()
            .HasOne(a => a.VetService)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.VetServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<VaccinationRecord>()
            .HasOne(vr => vr.Veterinarian)
            .WithMany()
            .HasForeignKey(vr => vr.VeterinarianId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}