using System.ComponentModel.DataAnnotations;

namespace SiakadSederhana.Models
{
    /// <summary>Model dosen untuk menampilkan data hasil GET dari API.</summary>
    public class Dosen
    {
        public int DosenId { get; set; }

        public string Nidn { get; set; } = string.Empty;

        public string NamaDosen { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Prodi { get; set; } = string.Empty;
    }

    /// <summary>DTO untuk form tambah/ubah dosen. Validasi identik dengan backend.</summary>
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
}
