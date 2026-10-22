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
            var nim = dto.Nim.Trim();
            if (await _context.Mahasiswa.AnyAsync(m => m.Nim.ToLower() == nim.ToLower()))
                return Conflict(new { message = "NIM sudah terdaftar." });

            var dosen = await _context.Dosen.FindAsync(dto.DosenId);
            if (dosen == null)
                return BadRequest(new { message = "Dosen wali dengan ID tersebut tidak ditemukan. Silakan pilih dosen yang tersedia." });

            var mhs = new Mahasiswa
            {
                Nim = nim,
                NamaMahasiswa = dto.NamaMahasiswa.Trim(),
                Email = dto.Email.Trim(),
                Angkatan = dto.Angkatan,
                DosenId = dosen.DosenId
            };
            _context.Mahasiswa.Add(mhs);
            await _context.SaveChangesAsync();

            var result = new MahasiswaDto(mhs.MahasiswaId, mhs.Nim, mhs.NamaMahasiswa, mhs.Email, mhs.Angkatan, mhs.DosenId, dosen.NamaDosen);
            return CreatedAtAction(nameof(GetById), new { id = mhs.MahasiswaId }, result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateMahasiswaDto dto)
        {
            var m = await _context.Mahasiswa.FindAsync(id);
            if (m == null) return NotFound(new { message = "Mahasiswa tidak ditemukan." });

            var nim = dto.Nim.Trim();
            if (await _context.Mahasiswa.AnyAsync(x => x.Nim.ToLower() == nim.ToLower() && x.MahasiswaId != id))
                return Conflict(new { message = "NIM sudah digunakan mahasiswa lain." });

            if (!await _context.Dosen.AnyAsync(d => d.DosenId == dto.DosenId))
                return BadRequest(new { message = "Dosen wali dengan ID tersebut tidak ditemukan. Silakan pilih dosen yang tersedia." });

            m.Nim = nim;
            m.NamaMahasiswa = dto.NamaMahasiswa.Trim();
            m.Email = dto.Email.Trim();
            m.Angkatan = dto.Angkatan;
            m.DosenId = dto.DosenId;
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
