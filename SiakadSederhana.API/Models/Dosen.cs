using System.ComponentModel.DataAnnotations;

namespace SiakadSederhana.API.Models
{
    public class Dosen
    {
        public int DosenId { get; set; }

        [Required, StringLength(20)]
        public string Nidn { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string NamaDosen { get; set; } = string.Empty;

        [Required, EmailAddress, RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$"), StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Prodi { get; set; } = string.Empty;

        public ICollection<Mahasiswa> DaftarMahasiswa { get; set; } = new List<Mahasiswa>();
    }
}