using System.Net.Http.Json;
using SiakadSederhana.Models;

namespace SiakadSederhana.Services
{
    /// <summary>Service akses endpoint /api/dosen melalui HttpClient.</summary>
    public class DosenService(HttpClient http) : ApiService(http)
    {
        private const string Url = "api/dosen";

        public Task<ApiResult<List<Dosen>>> GetSemuaAsync() =>
            DapatkanAsync<List<Dosen>>(Url);

        public Task<ApiResult<Dosen>> GetByIdAsync(int id) =>
            DapatkanAsync<Dosen>($"{Url}/{id}");

        public Task<ApiResult> TambahAsync(CreateDosenDto dto) =>
            KirimAsync(() => Http.PostAsJsonAsync(Url, dto));

        public Task<ApiResult> UbahAsync(int id, CreateDosenDto dto) =>
            KirimAsync(() => Http.PutAsJsonAsync($"{Url}/{id}", dto));

        public Task<ApiResult> HapusAsync(int id) =>
            KirimAsync(() => Http.DeleteAsync($"{Url}/{id}"));
    }
}
