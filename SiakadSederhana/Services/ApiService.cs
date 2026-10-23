using System.Net.Http.Json;
using System.Text.Json;

namespace SiakadSederhana.Services
{
    /// <summary>
    /// Kelas dasar semua service API. Menangani komunikasi HttpClient,
    /// status HTTP non-sukses, respons error JSON dari API, dan koneksi
    /// yang gagal/timeout sehingga halaman cukup menampilkan pesan hasil.
    /// </summary>
    public abstract class ApiService(HttpClient http)
    {
        protected readonly HttpClient Http = http;

        /// <summary>GET: mengambil data dari API dan mengubahnya menjadi objek T.</summary>
        protected async Task<ApiResult<T>> DapatkanAsync<T>(string url)
        {
            try
            {
                var response = await Http.GetAsync(url);
                if (!response.IsSuccessStatusCode)
                    return await HasilGagalAsync<T>(response);

                var data = await response.Content.ReadFromJsonAsync<T>();
                if (data is null)
                    return ApiResult<T>.Gagal("API tidak mengembalikan data.");

                return ApiResult<T>.SuksesData(data);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return ApiResult<T>.Gagal(PesanKoneksi(ex));
            }
            catch (JsonException)
            {
                return ApiResult<T>.Gagal("Respons API tidak dapat dibaca.");
            }
        }

        /// <summary>POST/PUT/DELETE: mengirim data tanpa mengharapkan balasan data.</summary>
        protected async Task<ApiResult> KirimAsync(Func<Task<HttpResponseMessage>> kirim)
        {
            try
            {
                var response = await kirim();
                if (!response.IsSuccessStatusCode)
                    return await HasilGagalAsync(response);

                return ApiResult.Sukses();
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                return ApiResult.Gagal(PesanKoneksi(ex));
            }
        }

        private static async Task<ApiResult<T>> HasilGagalAsync<T>(HttpResponseMessage response) =>
            ApiResult<T>.Gagal(await BacaPesanErrorAsync(response), (int)response.StatusCode);

        private static async Task<ApiResult> HasilGagalAsync(HttpResponseMessage response) =>
            ApiResult.Gagal(await BacaPesanErrorAsync(response), (int)response.StatusCode);

        /// <summary>
        /// Pesan koneksi menyertakan alamat API yang dipakai dan alasan asli
        /// exception (mis. "Connection refused (127.0.0.1:5103)") agar kegagalan
        /// di HP dapat langsung didiagnosis dari panel merah tanpa menebak-nebak.
        /// </summary>
        private string PesanKoneksi(Exception ex)
        {
            var alasan = ex is TaskCanceledException
                ? "melebihi batas waktu (timeout)"
                : ex.Message;

            if (ex.InnerException is not null)
                alasan += $" - {ex.InnerException.Message}";

            return $"Tidak dapat terhubung ke API di {Http.BaseAddress} ({alasan}). Pastikan layanan API sedang berjalan.";
        }

        /// <summary>
        /// Membaca pesan error dari body respons API.
        /// Mendukung { "message": "..." } dan ValidationProblemDetails { "errors": { ... } }.
        /// </summary>
        private static async Task<string> BacaPesanErrorAsync(HttpResponseMessage response)
        {
            var kode = (int)response.StatusCode;

            try
            {
                var body = await response.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(body))
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Object &&
                        root.TryGetProperty("message", out var pesan) &&
                        pesan.ValueKind == JsonValueKind.String &&
                        !string.IsNullOrWhiteSpace(pesan.GetString()))
                    {
                        return pesan.GetString()!;
                    }

                    if (root.ValueKind == JsonValueKind.Object &&
                        root.TryGetProperty("errors", out var errors) &&
                        errors.ValueKind == JsonValueKind.Object)
                    {
                        var daftar = new List<string>();
                        foreach (var field in errors.EnumerateObject())
                        {
                            var detail = string.Join("; ", field.Value.EnumerateArray()
                                .Select(p => p.GetString())
                                .Where(s => !string.IsNullOrWhiteSpace(s)));
                            if (!string.IsNullOrWhiteSpace(detail))
                                daftar.Add($"{field.Name}: {detail}");
                        }
                        if (daftar.Count > 0)
                            return string.Join(" | ", daftar);
                    }
                }
            }
            catch (JsonException)
            {
                // Body bukan JSON valid (mis. halaman HTML) -> gunakan pesan default.
            }

            return kode switch
            {
                400 => "Data tidak valid.",
                404 => "Data tidak ditemukan.",
                409 => "Terjadi konflik data (kemungkinan duplikat).",
                >= 500 => "Server mengalami gangguan. Coba lagi nanti.",
                _ => $"Permintaan gagal (HTTP {kode})."
            };
        }
    }
}
