using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SiakadSederhana.API.Data;

namespace SiakadSederhana.API.Tests
{
    /// <summary>
    /// Factory integrasi: menjalankan seluruh pipeline API (routing, model binding,
    /// validasi, controller, EF Core) dengan database SQLite in-memory.
    /// Setiap instance membuat database bersih yang otomatis dimigrasi + di-seed
    /// (3 dosen, 5 mahasiswa) oleh Program.cs — tanpa menyentuh file siakad.db produksi.
    /// </summary>
    public sealed class ApiTestFactory : WebApplicationFactory<Program>
    {
        // Koneksi SQLite in-memory: data hidup selama koneksi terbuka
        // (koneksi dibuka saat host dibangun dan ditutup saat factory dibuang).
        private readonly SqliteConnection _koneksi = new("DataSource=:memory:");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            _koneksi.Open();

            builder.ConfigureServices(services =>
            {
                // Ganti konfigurasi DbContext bawaan (Data Source=siakad.db)
                // dengan SQLite in-memory milik test ini.
                services.RemoveAll<DbContextOptions<SiakadDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<SiakadDbContext>>();
                services.AddDbContext<SiakadDbContext>(options => options.UseSqlite(_koneksi));
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _koneksi.Dispose();
            }
        }
    }
}
