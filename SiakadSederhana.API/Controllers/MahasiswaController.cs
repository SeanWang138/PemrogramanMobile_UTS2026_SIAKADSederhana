using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SiakadSederhana.API.Data;
using SiakadSederhana.API.DTOs;
using SiakadSederhana.API.Models;

namespace SiakadSederhana.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MahasiswaController : ControllerBase
    {
        private readonly SiakadDbContext _context;
        public MahasiswaController(SiakadDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MahasiswaDto>>> GetAll()
        {
            return await _context.Mahasiswa
                .Include(m => m.DosenWali)
                .Select(m => new MahasiswaDto(
                    m.MahasiswaId, m.Nim, m.NamaMahasiswa, m.Email, m.Angkatan, m.DosenId,
                    m.DosenWali != null ? m.DosenWali.NamaDosen : "-"
                )).ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MahasiswaDto>> GetById(int id)
        {
            var m = await _context.Mahasiswa.Include(x => x.DosenWali).FirstOrDefaultAsync(x => x.MahasiswaId == id);
            if (m == null) return NotFound(new { message = "Mahasiswa tidak ditemukan." });
            return new MahasiswaDto(m.MahasiswaId, m.Nim, m.NamaMahasiswa, m.Email, m.Angkatan, m.DosenId, m.DosenWali?.NamaDosen ?? "-");
        }

        [HttpPost]
        public async Task<ActionResult> Create(CreateMahasiswaDto dto)
        {
            if (await _context.Mahasiswa.AnyAsync(m => m.Nim == dto.Nim))
                return Conflict(new { message = "NIM sudah terdaftar." });

            var mhs = new Mahasiswa { Nim = dto.Nim, NamaMahasiswa = dto.NamaMahasiswa, Email = dto.Email, Angkatan = dto.Angkatan, DosenId = dto.DosenId };
            _context.Mahasiswa.Add(mhs);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = mhs.MahasiswaId }, mhs);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateMahasiswaDto dto)
        {
            var m = await _context.Mahasiswa.FindAsync(id);
            if (m == null) return NotFound(new { message = "Mahasiswa tidak ditemukan." });

            if (await _context.Mahasiswa.AnyAsync(x => x.Nim == dto.Nim && x.MahasiswaId != id))
                return Conflict(new { message = "NIM sudah digunakan mahasiswa lain." });

            m.Nim = dto.Nim; m.NamaMahasiswa = dto.NamaMahasiswa; m.Email = dto.Email; m.Angkatan = dto.Angkatan; m.DosenId = dto.DosenId;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var m = await _context.Mahasiswa.FindAsync(id);
            if (m == null) return NotFound(new { message = "Mahasiswa tidak ditemukan." });

            _context.Mahasiswa.Remove(m);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
