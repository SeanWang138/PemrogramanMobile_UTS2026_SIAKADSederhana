using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using SiakadSederhana.API.DTOs;

namespace SiakadSederhana.API.Tests
{
    /// <summary>
    /// Integration test endpoint /api/dosen terhadap skenario wajib T01-T08:
    /// GET, GET by id, POST valid, POST duplikat (409), validasi (400),
    /// PUT (200/404/409), dan DELETE (204/409/404).
    /// Setiap test memakai database in-memory sendiri dengan seed: 3 dosen + 5 mahasiswa.
    /// </summary>
    public class DosenControllerTests : IAsyncLifetime
    {
        private ApiTestFactory _pabrik = null!;
        private HttpClient _klien = null!;

        public Task InitializeAsync()
        {
            _pabrik = new ApiTestFactory();
            _klien = _pabrik.CreateClient();
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            _klien.Dispose();
            await _pabrik.DisposeAsync();
        }

        /// <summary>DTO dosen valid dengan NIDN unik supaya aman dipakai berulang kali.</summary>
        private static CreateDosenDto DosenBaru(string? nidn = null) => new()
        {
            Nidn = nidn ?? $"9{Guid.NewGuid():N}"[..12],
            NamaDosen = "Diana Putri, M.Kom.",
            Email = "diana@kampus.ac.id",
            Prodi = "Informatika"
        };

        private static async Task<string> BacaPesan(HttpResponseMessage respons)
        {
            var body = await respons.Content.ReadFromJsonAsync<JsonObject>();
            return body?["message"]?.GetValue<string>() ?? string.Empty;
        }

        // ---------- GET ----------

        [Fact]
        public async Task GetAll_KetikaDatabaseTerisi_Mengembalikan3Dosen()
        {
            var daftar = await _klien.GetFromJsonAsync<List<DosenDto>>("api/dosen");

            Assert.NotNull(daftar);
            Assert.Equal(3, daftar.Count);
            Assert.Contains(daftar, d => d.NamaDosen == "Dr. Anita Wijaya");
        }

        [Fact]
        public async Task GetById_DataDosenAda_Mengembalikan200IsiLengkap()
        {
            var respons = await _klien.GetAsync("api/dosen/1");

            Assert.Equal(HttpStatusCode.OK, respons.StatusCode);
            var dosen = await respons.Content.ReadFromJsonAsync<DosenDto>();
            Assert.NotNull(dosen);
            Assert.Equal("0512345678", dosen.Nidn);
            Assert.Equal("Dr. Anita Wijaya", dosen.NamaDosen);
            Assert.Equal("Sistem Informasi", dosen.Prodi);
        }

        [Fact]
        public async Task GetById_DataDosenTidakAda_Mengembalikan404()
        {
            var respons = await _klien.GetAsync("api/dosen/9999");

            Assert.Equal(HttpStatusCode.NotFound, respons.StatusCode);
            Assert.Equal("Dosen tidak ditemukan.", await BacaPesan(respons));
        }

        // ---------- POST ----------

        [Fact]
        public async Task Create_DataValid_Mengembalikan201DanDtoBaru()
        {
            var dto = DosenBaru();

            var respons = await _klien.PostAsJsonAsync("api/dosen", dto);

            Assert.Equal(HttpStatusCode.Created, respons.StatusCode);
            Assert.NotNull(respons.Headers.Location);

            var hasil = await respons.Content.ReadFromJsonAsync<DosenDto>();
            Assert.NotNull(hasil);
            Assert.True(hasil.DosenId > 0);
            Assert.Equal(dto.Nidn, hasil.Nidn);
            Assert.Equal("Diana Putri, M.Kom.", hasil.NamaDosen);

            var daftar = await _klien.GetFromJsonAsync<List<DosenDto>>("api/dosen");
            Assert.Equal(4, daftar!.Count);
        }

        [Fact]
        public async Task Create_NidnSudahTerdaftar_Mengembalikan409DanDataTidakBertambah()
        {
            var dto = DosenBaru(nidn: "0512345678"); // milik dosen seed

            var respons = await _klien.PostAsJsonAsync("api/dosen", dto);

            Assert.Equal(HttpStatusCode.Conflict, respons.StatusCode);
            Assert.Equal("NIDN sudah terdaftar.", await BacaPesan(respons));

            var daftar = await _klien.GetFromJsonAsync<List<DosenDto>>("api/dosen");
            Assert.Equal(3, daftar!.Count);
        }

        [Fact]
        public async Task Create_EmailTidakValid_Mengembalikan400BesertaErrornya()
        {
            var dto = DosenBaru();
            dto.Email = "bukan-email";

            var respons = await _klien.PostAsJsonAsync("api/dosen", dto);

            Assert.Equal(HttpStatusCode.BadRequest, respons.StatusCode);
            var problem = await respons.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.NotNull(problem);
            Assert.True(problem.Errors.ContainsKey("Email"));
        }

        [Fact]
        public async Task Create_BidangWajibKosong_Mengembalikan400()
        {
            var dto = new CreateDosenDto { Nidn = "", NamaDosen = "", Email = "", Prodi = "" };

            var respons = await _klien.PostAsJsonAsync("api/dosen", dto);

            Assert.Equal(HttpStatusCode.BadRequest, respons.StatusCode);
            var problem = await respons.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.NotNull(problem);
            Assert.True(problem.Errors.ContainsKey("Nidn"));
            Assert.True(problem.Errors.ContainsKey("NamaDosen"));
        }

        // ---------- PUT ----------

        [Fact]
        public async Task Update_DataValid_Mengembalikan204DanPerubahanTersimpan()
        {
            // Buat dosen khusus untuk test ini agar tidak mengubah data seed.
            var buat = await _klien.PostAsJsonAsync("api/dosen", DosenBaru());
            var dosen = await buat.Content.ReadFromJsonAsync<DosenDto>();
            Assert.NotNull(dosen);

            var dto = new CreateDosenDto
            {
                Nidn = dosen.Nidn,
                NamaDosen = "Diana Putri, Ph.D.",
                Email = "diana.baru@kampus.ac.id",
                Prodi = "Sistem Informasi"
            };
            var respons = await _klien.PutAsJsonAsync($"api/dosen/{dosen.DosenId}", dto);

            Assert.Equal(HttpStatusCode.NoContent, respons.StatusCode);

            var hasil = await _klien.GetFromJsonAsync<DosenDto>($"api/dosen/{dosen.DosenId}");
            Assert.Equal("Diana Putri, Ph.D.", hasil!.NamaDosen);
            Assert.Equal("Sistem Informasi", hasil.Prodi);
        }

        [Fact]
        public async Task Update_DataTidakAda_Mengembalikan404()
        {
            var respons = await _klien.PutAsJsonAsync("api/dosen/9999", DosenBaru());

            Assert.Equal(HttpStatusCode.NotFound, respons.StatusCode);
        }

        [Fact]
        public async Task Update_NidnMilikDosenLain_Mengembalikan409()
        {
            var buat = await _klien.PostAsJsonAsync("api/dosen", DosenBaru());
            var dosen = await buat.Content.ReadFromJsonAsync<DosenDto>();

            var dto = DosenBaru(nidn: "0587654321"); // milik dosen seed lain
            var respons = await _klien.PutAsJsonAsync($"api/dosen/{dosen!.DosenId}", dto);

            Assert.Equal(HttpStatusCode.Conflict, respons.StatusCode);
            Assert.Equal("NIDN sudah digunakan oleh dosen lain.", await BacaPesan(respons));
        }

        [Fact]
        public async Task Update_MempertahankanNidnSendiri_TidakKonflik()
        {
            var buat = await _klien.PostAsJsonAsync("api/dosen", DosenBaru());
            var dosen = await buat.Content.ReadFromJsonAsync<DosenDto>();

            var dto = DosenBaru(nidn: dosen!.Nidn); // NIDN-nya sendiri, nama berubah
            dto.NamaDosen = "Diana Putri, S.Kom.";

            var respons = await _klien.PutAsJsonAsync($"api/dosen/{dosen.DosenId}", dto);

            Assert.Equal(HttpStatusCode.NoContent, respons.StatusCode);
        }

        // ---------- DELETE ----------

        [Fact]
        public async Task Delete_DosenMasihMenjadiWali_Mengembalikan409DanDataTetapAda()
        {
            // Dosen seed 1 (Dr. Anita) masih menjadi wali 2 mahasiswa.
            var respons = await _klien.DeleteAsync("api/dosen/1");

            Assert.Equal(HttpStatusCode.Conflict, respons.StatusCode);
            Assert.Contains("masih menjadi dosen wali", await BacaPesan(respons));

            var daftar = await _klien.GetFromJsonAsync<List<DosenDto>>("api/dosen");
            Assert.Equal(3, daftar!.Count); // tidak ada yang terhapus
        }

        [Fact]
        public async Task Delete_DosenTidakMenjadiWali_Mengembalikan204()
        {
            var buat = await _klien.PostAsJsonAsync("api/dosen", DosenBaru());
            var dosen = await buat.Content.ReadFromJsonAsync<DosenDto>();

            var respons = await _klien.DeleteAsync($"api/dosen/{dosen!.DosenId}");

            Assert.Equal(HttpStatusCode.NoContent, respons.StatusCode);

            var daftar = await _klien.GetFromJsonAsync<List<DosenDto>>("api/dosen");
            Assert.Equal(3, daftar!.Count); // kembali ke jumlah seed
        }

        [Fact]
        public async Task Delete_DataTidakAda_Mengembalikan404()
        {
            var respons = await _klien.DeleteAsync("api/dosen/9999");

            Assert.Equal(HttpStatusCode.NotFound, respons.StatusCode);
        }
    }
}
