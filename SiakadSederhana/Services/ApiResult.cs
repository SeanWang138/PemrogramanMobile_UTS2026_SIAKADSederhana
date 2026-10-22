namespace SiakadSederhana.Services
{
    /// <summary>Hasil operasi API tanpa data balasan (tambah/ubah/hapus).</summary>
    public class ApiResult
    {
        public bool Berhasil { get; init; }
        public string? Pesan { get; init; }
        public int? StatusCode { get; init; }

        public static ApiResult Sukses(string? pesan = null) =>
            new() { Berhasil = true, Pesan = pesan };

        public static ApiResult Gagal(string pesan, int? statusCode = null) =>
            new() { Berhasil = false, Pesan = pesan, StatusCode = statusCode };
    }

    /// <summary>Hasil operasi API yang menyertakan data balasan (GET).</summary>
    public sealed class ApiResult<T> : ApiResult
    {
        public T? Data { get; init; }

        public static ApiResult<T> SuksesData(T data, string? pesan = null) =>
            new() { Berhasil = true, Data = data, Pesan = pesan };

        public new static ApiResult<T> Gagal(string pesan, int? statusCode = null) =>
            new() { Berhasil = false, Pesan = pesan, StatusCode = statusCode };
    }
}
