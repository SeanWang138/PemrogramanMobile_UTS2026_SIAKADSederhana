using System.ComponentModel.DataAnnotations;

namespace SiakadSederhana.Models
{
    /// <summary>Model mahasiswa untuk menampilkan data hasil GET dari API.</summary>
    public class Mahasiswa
    {
        public int MahasiswaId { get; set; }

        public string Nim { get; set; } = string.Empty;

        public string NamaMahasiswa { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public int Angkatan { get; set; }

        public int DosenId { get; set; }

        /// <summary>Nama dosen wali, diisi oleh API (bukan relasi objek agar JSON tidak berulang).</summary>
        public string NamaDosenWali { get; set; } = "-";
    }

    /// <summary>DTO untuk form tambah/ubah mahasiswa. Validasi identik dengan backend.</summary>
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

        [Range(1, int.MaxValue, ErrorMessage = "Dosen wali wajib dipilih.")]
        public int DosenId { get; set; }
    }
}
