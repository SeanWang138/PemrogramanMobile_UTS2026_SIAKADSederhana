namespace SiakadSederhana.API.DTOs
{
    public record DosenDto(int DosenId, string Nidn, string NamaDosen, string Email, string Prodi);
    public record CreateDosenDto(string Nidn, string NamaDosen, string Email, string Prodi);

    public record MahasiswaDto(
        int MahasiswaId,
        string Nim,
        string NamaMahasiswa,
        string Email,
        int Angkatan,
        int DosenId,
        string NamaDosenWali
    );

    public record CreateMahasiswaDto(string Nim, string NamaMahasiswa, string Email, int Angkatan, int DosenId);
}
