using System.Diagnostics;

namespace SiakadSederhana.API
{
    /// <summary>
    /// Menjalankan "adb reverse" secara otomatis agar aplikasi di HP Android fisik
    /// dapat mengakses API melalui http://localhost:5103 tanpa perintah terminal
    /// manual. Selalu best-effort: bila adb atau HP tidak tersedia, API tetap
    /// berjalan normal (jalur emulator 10.0.2.2 dan Windows juga tidak terpengaruh).
    /// </summary>
    public static class UsbForwarding
    {
        // Port yang sama dengan launchSettings.json (applicationUrl) dan MauiProgram.cs.
        private const int Port = 5103;

        /// <summary>Mencoba membuat forwarding USB. Tidak pernah melempar exception.</summary>
        public static void CobaAktifkan()
        {
            try
            {
                var adb = CariAdb();
                if (adb is null)
                {
                    Console.WriteLine("  Info: USB forwarding dilewati - adb tidak ditemukan (wajar bila memakai emulator).");
                    return;
                }

                var psi = new ProcessStartInfo
                {
                    FileName = adb,
                    Arguments = $"reverse tcp:{Port} tcp:{Port}",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                };

                using var proses = Process.Start(psi);
                if (proses is null)
                    return;

                var stderr = proses.StandardError.ReadToEnd();
                _ = proses.StandardOutput.ReadToEnd();

                if (!proses.WaitForExit(5000))
                {
                    try { proses.Kill(entireProcessTree: true); } catch { /* abaikan */ }
                    Console.WriteLine("  Info: USB forwarding dilewati - adb tidak merespons dalam 5 detik.");
                    return;
                }

                if (proses.ExitCode == 0)
                    Console.WriteLine($"  OK: USB forwarding aktif - HP localhost:{Port} -> PC (adb reverse).");
                else
                    Console.WriteLine($"  Info: USB forwarding tidak aktif - {Bersih(stderr)} (biasanya HP belum terpasang; API tetap jalan).");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Info: USB forwarding dilewati - {ex.Message}");
            }
        }

        /// <summary>
        /// Mencari lokasi adb.exe. Urutan: lokasi default Visual Studio lebih dahulu
        /// (disengaja agar versi adb sama dengan milik VS dan tidak saling me-restart
        /// server adb), lalu environment variable SDK, lalu Android Studio, terakhir PATH.
        /// </summary>
        private static string? CariAdb()
        {
            var namaFile = OperatingSystem.IsWindows() ? "adb.exe" : "adb";

            // 1. Lokasi default Visual Studio
            var kandidatVs = new[]
            {
                Path.Combine(@"C:\Program Files (x86)\Android\android-sdk\platform-tools", namaFile),
                Path.Combine(@"C:\Program Files\Android\android-sdk\platform-tools", namaFile),
            };
            foreach (var k in kandidatVs)
                if (File.Exists(k)) return k;

            // 2. Environment variable konfigurasi SDK Android
            foreach (var nama in new[] { "ANDROID_HOME", "ANDROID_SDK_ROOT" })
            {
                var akar = Environment.GetEnvironmentVariable(nama);
                if (!string.IsNullOrWhiteSpace(akar))
                {
                    var p = Path.Combine(akar, "platform-tools", namaFile);
                    if (File.Exists(p)) return p;
                }
            }

            // 3. Lokasi default Android Studio
            var androidStudio = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Android", "Sdk", "platform-tools", namaFile);
            if (File.Exists(androidStudio)) return androidStudio;

            // 4. PATH (paling akhir agar tidak bentrok dengan versi adb milik VS)
            foreach (var dir in (Environment.GetEnvironmentVariable("PATH") ?? string.Empty)
                .Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries))
            {
                try
                {
                    var p = Path.Combine(dir.Trim(), namaFile);
                    if (File.Exists(p)) return p;
                }
                catch
                {
                    // Entri PATH tidak valid - abaikan.
                }
            }

            return null;
        }

        private static string Bersih(string teks) =>
            string.IsNullOrWhiteSpace(teks) ? "(tanpa pesan)" : teks.Replace('\r', ' ').Replace('\n', ' ').Trim();
    }
}
