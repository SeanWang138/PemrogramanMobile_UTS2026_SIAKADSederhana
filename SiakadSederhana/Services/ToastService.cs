namespace SiakadSederhana.Services
{
    public enum ToastJenis { Sukses, Gagal }

    /// <summary>Data satu notifikasi toast.</summary>
    public class ToastItem
    {
        public Guid Id { get; } = Guid.NewGuid();
        public string Pesan { get; init; } = string.Empty;
        public ToastJenis Jenis { get; init; }
    }

    /// <summary>
    /// Penghubung antara halaman dan ToastHost di MainLayout.
    /// Halaman cukup memanggil Sukses()/Gagal(), toast tampil otomatis.
    /// </summary>
    public class ToastService
    {
        public event Action<ToastItem>? AdaToastBaru;

        public void Tampilkan(string pesan, ToastJenis jenis) =>
            AdaToastBaru?.Invoke(new ToastItem { Pesan = pesan, Jenis = jenis });

        public void Sukses(string pesan) => Tampilkan(pesan, ToastJenis.Sukses);

        public void Gagal(string pesan) => Tampilkan(pesan, ToastJenis.Gagal);
    }
}
