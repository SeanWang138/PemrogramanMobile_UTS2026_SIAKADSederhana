using System.Net.Http.Json;
using SiakadSederhana.Models;

namespace SiakadSederhana.Services
{
    /// <summary>Service akses endpoint /api/mahasiswa melalui HttpClient.</summary>
    public class MahasiswaService : ApiService
    {
        private const string Url = "api/mahasiswa";

        public MahasiswaService(HttpClient http) : base(http) { }

        public Task<ApiResult<List<Mahasiswa>>> GetSemuaAsync() =>
            DapatkanAsync<List<Mahasiswa>>(Url);

        public Task<ApiResult<Mahasiswa>> GetByIdAsync(int id) =>
            DapatkanAsync<Mahasiswa>($"{Url}/{id}");

        public Task<ApiResult> TambahAsync(CreateMahasiswaDto dto) =>
            KirimAsync(() => Http.PostAsJsonAsync(Url, dto));

        public Task<ApiResult> UbahAsync(int id, CreateMahasiswaDto dto) =>
            KirimAsync(() => Http.PutAsJsonAsync($"{Url}/{id}", dto));

        public Task<ApiResult> HapusAsync(int id) =>
            KirimAsync(() => Http.DeleteAsync($"{Url}/{id}"));
    }
}
