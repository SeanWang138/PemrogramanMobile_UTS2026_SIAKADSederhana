using System.ComponentModel.DataAnnotations;

namespace SiakadSederhana.API.DTOs
{
    public record DosenDto(int DosenId, string Nidn, string NamaDosen, string Email, string Prodi);

    public class CreateDosenDto
    {
        [Required(ErrorMessage = "NIDN wajib diisi.")]
        [StringLength(20, ErrorMessage = "NIDN maksimal 20 karakter.")]
        public string Nidn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama dosen wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama dosen maksimal 100 karakter.")]
        public string NamaDosen { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Format email tidak valid.")]
        [StringLength(100, ErrorMessage = "Email maksimal 100 karakter.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Program studi wajib diisi.")]
        [StringLength(100, ErrorMessage = "Program studi maksimal 100 karakter.")]
        public string Prodi { get; set; } = string.Empty;
    }

    public record MahasiswaDto(
        int MahasiswaId,
        string Nim,
        string NamaMahasiswa,
        string Email,
        int Angkatan,
        int DosenId,
        string NamaDosenWali
    );

    public class CreateMahasiswaDto
    {
        [Required(ErrorMessage = "NIM wajib diisi.")]
        [StringLength(20, ErrorMessage = "NIM maksimal 20 karakter.")]
        public string Nim { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nama mahasiswa wajib diisi.")]
        [StringLength(100, ErrorMessage = "Nama mahasiswa maksimal 100 karakter.")]
        public string NamaMahasiswa { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email wajib diisi.")]
        [EmailAddress(ErrorMessage = "Format email tidak valid.")]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Format email tidak valid.")]
        [StringLength(100, ErrorMessage = "Email maksimal 100 karakter.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Angkatan wajib diisi.")]
        [Range(2020, 2026, ErrorMessage = "Angkatan harus berada di rentang 2020 - 2026.")]
        public int Angkatan { get; set; }

        [Required(ErrorMessage = "Dosen wali wajib dipilih.")]
        [Range(1, int.MaxValue, ErrorMessage = "Dosen wali harus merujuk ke dosen yang tersedia.")]
        public int DosenId { get; set; }
    }
}
