using Microsoft.EntityFrameworkCore;
using SiakadSederhana.API.Data;

namespace SiakadSederhana.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddDbContext<SiakadDbContext>(options =>
                options.UseSqlite("Data Source=siakad.db"));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // UseHttpsRedirection sengaja TIDAK dipasang: semua klien proyek ini
            // (app MAUI di Windows, emulator Android, dan HP via "adb reverse")
            // memakai http://localhost:5103. Redirect ke HTTPS (port 7005) akan
            // merusak jalur adb reverse yang hanya meneruskan port 5103.
            app.UseAuthorization();
            app.MapControllers();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<SiakadDbContext>();
                db.Database.Migrate();
            }

            // USB forwarding otomatis agar HP dapat mengakses API via localhost:5103
            // tanpa perintah "adb reverse" manual (best-effort, lihat UsbForwarding.cs).
            UsbForwarding.CobaAktifkan();

            app.Run();
        }
    }
}
