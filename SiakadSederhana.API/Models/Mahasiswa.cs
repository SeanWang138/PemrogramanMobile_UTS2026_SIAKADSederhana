using System.ComponentModel.DataAnnotations;

namespace SiakadSederhana.API.Models
{
    public class Mahasiswa
    {
        public int MahasiswaId { get; set; }

        [Required, StringLength(20)]
        public string Nim { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string NamaMahasiswa { get; set; } = string.Empty;

        [Required, EmailAddress, RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$"), StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Range(2020, 2026)]
        public int Angkatan { get; set; }

        [Required]
        public int DosenId { get; set; }

        public Dosen? DosenWali { get; set; }
    }
}
