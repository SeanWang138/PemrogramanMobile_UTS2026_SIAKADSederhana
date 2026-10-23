using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiakadSederhana.API.Data;
using SiakadSederhana.API.DTOs;
using SiakadSederhana.API.Models;

namespace SiakadSederhana.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DosenController(SiakadDbContext context) : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DosenDto>>> GetAll()
        {
            return await context.Dosen
                .Select(d => new DosenDto(d.DosenId, d.Nidn, d.NamaDosen, d.Email, d.Prodi))
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DosenDto>> GetById(int id)
        {
            var d = await context.Dosen.FindAsync(id);
            if (d == null) return NotFound(new { message = "Dosen tidak ditemukan." });
            return new DosenDto(d.DosenId, d.Nidn, d.NamaDosen, d.Email, d.Prodi);
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateDosenDto dto)
        {
            var nidn = dto.Nidn.Trim();
            if (await context.Dosen.AnyAsync(d => d.Nidn.ToLower() == nidn.ToLower()))
                return Conflict(new { message = "NIDN sudah terdaftar." });

            var dosen = new Dosen
            {
                Nidn = nidn,
                NamaDosen = dto.NamaDosen.Trim(),
                Email = dto.Email.Trim(),
                Prodi = dto.Prodi.Trim()
            };
            context.Dosen.Add(dosen);
            await context.SaveChangesAsync();

            var result = new DosenDto(dosen.DosenId, dosen.Nidn, dosen.NamaDosen, dosen.Email, dosen.Prodi);
            return CreatedAtAction(nameof(GetById), new { id = dosen.DosenId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateDosenDto dto)
        {
            var d = await context.Dosen.FindAsync(id);
            if (d == null) return NotFound(new { message = "Dosen tidak ditemukan." });

            var nidn = dto.Nidn.Trim();
            if (await context.Dosen.AnyAsync(x => x.Nidn.ToLower() == nidn.ToLower() && x.DosenId != id))
                return Conflict(new { message = "NIDN sudah digunakan oleh dosen lain." });

            d.Nidn = nidn;
            d.NamaDosen = dto.NamaDosen.Trim();
            d.Email = dto.Email.Trim();
            d.Prodi = dto.Prodi.Trim();
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var d = await context.Dosen.Include(x => x.DaftarMahasiswa).FirstOrDefaultAsync(x => x.DosenId == id);
            if (d == null) return NotFound(new { message = "Dosen tidak ditemukan." });

            if (d.DaftarMahasiswa.Any())
                return Conflict(new { message = "Dosen tidak dapat dihapus karena masih menjadi dosen wali dari mahasiswa." });

            context.Dosen.Remove(d);
            await context.SaveChangesAsync();
            return NoContent();
        }
    }
}
