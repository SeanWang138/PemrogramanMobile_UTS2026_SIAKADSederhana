using Microsoft.EntityFrameworkCore;
using SiakadSederhana.API.Models;

namespace SiakadSederhana.API.Data
{
    public class SiakadDbContext : DbContext
    {
        public SiakadDbContext(DbContextOptions<SiakadDbContext> options) : base(options) { }

        public DbSet<Dosen> Dosen => Set<Dosen>();
        public DbSet<Mahasiswa> Mahasiswa => Set<Mahasiswa>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Dosen>().HasIndex(d => d.Nidn).IsUnique();
            modelBuilder.Entity<Mahasiswa>().HasIndex(m => m.Nim).IsUnique();

            modelBuilder.Entity<Mahasiswa>()
                .HasOne(m => m.DosenWali)
                .WithMany(d => d.DaftarMahasiswa)
                .HasForeignKey(m => m.DosenId)
                .OnDelete(DeleteBehavior.Restrict);

            // Data Seeding
            modelBuilder.Entity<Dosen>().HasData(
                new Dosen { DosenId = 1, Nidn = "0512345678", NamaDosen = "Dr. Anita Wijaya", Email = "anita@kampus.ac.id", Prodi = "Sistem Informasi" },
                new Dosen { DosenId = 2, Nidn = "0587654321", NamaDosen = "Budi Hartono, M.T.", Email = "budi@kampus.ac.id", Prodi = "Informatika" },
                new Dosen { DosenId = 3, Nidn = "0511223344", NamaDosen = "Siti Rahma, M.Kom.", Email = "siti@kampus.ac.id", Prodi = "Sistem Informasi" }
            );

            modelBuilder.Entity<Mahasiswa>().HasData(
                new Mahasiswa { MahasiswaId = 1, Nim = "72240001", NamaMahasiswa = "Budi Santoso", Email = "budi@student.ac.id", Angkatan = 2023, DosenId = 1 },
                new Mahasiswa { MahasiswaId = 2, Nim = "72240002", NamaMahasiswa = "Citra Dewi", Email = "citra@student.ac.id", Angkatan = 2023, DosenId = 1 },
                new Mahasiswa { MahasiswaId = 3, Nim = "72240003", NamaMahasiswa = "Eko Prasetyo", Email = "eko@student.ac.id", Angkatan = 2024, DosenId = 2 },
                new Mahasiswa { MahasiswaId = 4, Nim = "72240004", NamaMahasiswa = "Fani Kurnia", Email = "fani@student.ac.id", Angkatan = 2022, DosenId = 2 },
                new Mahasiswa { MahasiswaId = 5, Nim = "72240005", NamaMahasiswa = "Gita Gutawa", Email = "gita@student.ac.id", Angkatan = 2025, DosenId = 3 }
            );
        }
    }
}
