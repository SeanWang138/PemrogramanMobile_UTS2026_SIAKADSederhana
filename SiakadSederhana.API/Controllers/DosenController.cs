using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiakadSederhana.API.Data;
using SiakadSederhana.API.DTOs;
using SiakadSederhana.API.Models;

namespace SiakadSederhana.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DosenController : ControllerBase
    {
        private readonly SiakadDbContext _context;
        public DosenController(SiakadDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<DosenDto>>> GetAll()
        {
            return await _context.Dosen
                .Select(d => new DosenDto(d.DosenId, d.Nidn, d.NamaDosen, d.Email, d.Prodi))
                .ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<DosenDto>> GetById(int id)
        {
            var d = await _context.Dosen.FindAsync(id);
            if (d == null) return NotFound(new { message = "Dosen tidak ditemukan." });
            return new DosenDto(d.DosenId, d.Nidn, d.NamaDosen, d.Email, d.Prodi);
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateDosenDto dto)
        {
            if (await _context.Dosen.AnyAsync(d => d.Nidn == dto.Nidn))
                return Conflict(new { message = "NIDN sudah terdaftar." });

            var dosen = new Dosen { Nidn = dto.Nidn, NamaDosen = dto.NamaDosen, Email = dto.Email, Prodi = dto.Prodi };
            _context.Dosen.Add(dosen);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = dosen.DosenId }, dosen);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateDosenDto dto)
        {
            var d = await _context.Dosen.FindAsync(id);
            if (d == null) return NotFound(new { message = "Dosen tidak ditemukan." });

            if (await _context.Dosen.AnyAsync(x => x.Nidn == dto.Nidn && x.DosenId != id))
                return Conflict(new { message = "NIDN sudah digunakan oleh dosen lain." });

            d.Nidn = dto.Nidn; d.NamaDosen = dto.NamaDosen; d.Email = dto.Email; d.Prodi = dto.Prodi;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var d = await _context.Dosen.Include(x => x.DaftarMahasiswa).FirstOrDefaultAsync(x => x.DosenId == id);
            if (d == null) return NotFound(new { message = "Dosen tidak ditemukan." });

            if (d.DaftarMahasiswa.Any())
                return Conflict(new { message = "Dosen tidak dapat dihapus karena masih menjadi dosen wali dari mahasiswa." });

            _context.Dosen.Remove(d);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
