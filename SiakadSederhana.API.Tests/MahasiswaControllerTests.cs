using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc;
using SiakadSederhana.API.DTOs;

namespace SiakadSederhana.API.Tests
{
    /// <summary>
    /// Integration test endpoint /api/mahasiswa: proyeksi DTO namaDosenWali,
    /// POST valid/duplikat/validasi, relasi DosenId (400 bila tidak ada),
    /// PUT, dan DELETE. Seed per test: 3 dosen + 5 mahasiswa.
    /// </summary>
    public class MahasiswaControllerTests : IAsyncLifetime
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

        /// <summary>DTO mahasiswa valid dengan NIM unik; wali default = dosen seed 1.</summary>
        private static CreateMahasiswaDto MahasiswaBaru(string? nim = null, int dosenId = 1) => new()
        {
            Nim = nim ?? $"7229{Guid.NewGuid():N}"[..12],
            NamaMahasiswa = "Hendra Wijaya",
            Email = "hendra@student.ac.id",
            Angkatan = 2024,
            DosenId = dosenId
        };

        private static async Task<string> BacaPesan(HttpResponseMessage respons)
        {
            var body = await respons.Content.ReadFromJsonAsync<JsonObject>();
            return body?["message"]?.GetValue<string>() ?? string.Empty;
        }

        // ---------- GET ----------

        [Fact]
        public async Task GetAll_KetikaDatabaseTerisi_Mengembalikan5Mahasiswa()
        {
            var daftar = await _klien.GetFromJsonAsync<List<MahasiswaDto>>("api/mahasiswa");

            Assert.NotNull(daftar);
            Assert.Equal(5, daftar.Count);
        }

        [Fact]
        public async Task GetAll_MemuatNamaDosenWaliMelaluiProyeksiDto()
        {
            var daftar = await _klien.GetFromJsonAsync<List<MahasiswaDto>>("api/mahasiswa");

            Assert.NotNull(daftar);
            Assert.Equal("Dr. Anita Wijaya", daftar.Single(m => m.Nim == "72240001").NamaDosenWali);
            Assert.Equal("Budi Hartono, M.T.", daftar.Single(m => m.Nim == "72240003").NamaDosenWali);
            Assert.Equal("Siti Rahma, M.Kom.", daftar.Single(m => m.Nim == "72240005").NamaDosenWali);
        }

        [Fact]
        public async Task GetById_DataMahasiswaAda_Mengembalikan200()
        {
            var respons = await _klien.GetAsync("api/mahasiswa/3");

            Assert.Equal(HttpStatusCode.OK, respons.StatusCode);
            var mhs = await respons.Content.ReadFromJsonAsync<MahasiswaDto>();
            Assert.NotNull(mhs);
            Assert.Equal("Eko Prasetyo", mhs.NamaMahasiswa);
            Assert.Equal("Budi Hartono, M.T.", mhs.NamaDosenWali);
        }

        [Fact]
        public async Task GetById_DataMahasiswaTidakAda_Mengembalikan404()
        {
            var respons = await _klien.GetAsync("api/mahasiswa/9999");

            Assert.Equal(HttpStatusCode.NotFound, respons.StatusCode);
            Assert.Equal("Mahasiswa tidak ditemukan.", await BacaPesan(respons));
        }

        // ---------- POST ----------

        [Fact]
        public async Task Create_DataValid_Mengembalikan201DenganNamaDosenWali()
        {
            var dto = MahasiswaBaru(dosenId: 2);

            var respons = await _klien.PostAsJsonAsync("api/mahasiswa", dto);

            Assert.Equal(HttpStatusCode.Created, respons.StatusCode);
            Assert.NotNull(respons.Headers.Location);

            var hasil = await respons.Content.ReadFromJsonAsync<MahasiswaDto>();
            Assert.NotNull(hasil);
            Assert.True(hasil.MahasiswaId > 0);
            Assert.Equal(dto.Nim, hasil.Nim);
            Assert.Equal("Budi Hartono, M.T.", hasil.NamaDosenWali); // ikut terproyeksi

            var daftar = await _klien.GetFromJsonAsync<List<MahasiswaDto>>("api/mahasiswa");
            Assert.Equal(6, daftar!.Count);
        }

        [Fact]
        public async Task Create_NimSudahTerdaftar_Mengembalikan409DanDataTidakBertambah()
        {
            var dto = MahasiswaBaru(nim: "72240001"); // milik mahasiswa seed

            var respons = await _klien.PostAsJsonAsync("api/mahasiswa", dto);

            Assert.Equal(HttpStatusCode.Conflict, respons.StatusCode);
            Assert.Equal("NIM sudah terdaftar.", await BacaPesan(respons));

            var daftar = await _klien.GetFromJsonAsync<List<MahasiswaDto>>("api/mahasiswa");
            Assert.Equal(5, daftar!.Count);
        }

        [Fact]
        public async Task Create_AngkatanDiLuarRentang_Mengembalikan400()
        {
            var dto = MahasiswaBaru();
            dto.Angkatan = 2019; // di luar rentang 2020-2026

            var respons = await _klien.PostAsJsonAsync("api/mahasiswa", dto);

            Assert.Equal(HttpStatusCode.BadRequest, respons.StatusCode);
            var problem = await respons.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.NotNull(problem);
            Assert.True(problem.Errors.ContainsKey("Angkatan"));
        }

        [Fact]
        public async Task Create_EmailTidakValid_Mengembalikan400()
        {
            var dto = MahasiswaBaru();
            dto.Email = "hendra@";

            var respons = await _klien.PostAsJsonAsync("api/mahasiswa", dto);

            Assert.Equal(HttpStatusCode.BadRequest, respons.StatusCode);
            var problem = await respons.Content.ReadFromJsonAsync<ValidationProblemDetails>();
            Assert.NotNull(problem);
            Assert.True(problem.Errors.ContainsKey("Email"));
        }

        [Fact]
        public async Task Create_DosenWaliTidakAda_Mengembalikan400()
        {
            var dto = MahasiswaBaru(dosenId: 9999);

            var respons = await _klien.PostAsJsonAsync("api/mahasiswa", dto);

            Assert.Equal(HttpStatusCode.BadRequest, respons.StatusCode);
            Assert.Contains("tidak ditemukan", await BacaPesan(respons));
        }

        // ---------- PUT ----------

        [Fact]
        public async Task Update_DataValid_Mengembalikan204DanGantiDosenWali()
        {
            // Mahasiswa khusus test ini, berawal sebagai wali dosen seed 1.
            var buat = await _klien.PostAsJsonAsync("api/mahasiswa", MahasiswaBaru());
            var mhs = await buat.Content.ReadFromJsonAsync<MahasiswaDto>();
            Assert.NotNull(mhs);

            var dto = MahasiswaBaru(nim: mhs.Nim, dosenId: 3); // pindah ke Siti Rahma
            dto.NamaMahasiswa = "Hendra G. Wijaya";
            var respons = await _klien.PutAsJsonAsync($"api/mahasiswa/{mhs.MahasiswaId}", dto);

            Assert.Equal(HttpStatusCode.NoContent, respons.StatusCode);

            var hasil = await _klien.GetFromJsonAsync<MahasiswaDto>($"api/mahasiswa/{mhs.MahasiswaId}");
            Assert.Equal("Hendra G. Wijaya", hasil!.NamaMahasiswa);
            Assert.Equal("Siti Rahma, M.Kom.", hasil.NamaDosenWali);
        }

        [Fact]
        public async Task Update_DataTidakAda_Mengembalikan404()
        {
            var respons = await _klien.PutAsJsonAsync("api/mahasiswa/9999", MahasiswaBaru());

            Assert.Equal(HttpStatusCode.NotFound, respons.StatusCode);
        }

        [Fact]
        public async Task Update_NimMilikMahasiswaLain_Mengembalikan409()
        {
            var buat = await _klien.PostAsJsonAsync("api/mahasiswa", MahasiswaBaru());
            var mhs = await buat.Content.ReadFromJsonAsync<MahasiswaDto>();

            var dto = MahasiswaBaru(nim: "72240002"); // milik mahasiswa seed lain
            var respons = await _klien.PutAsJsonAsync($"api/mahasiswa/{mhs!.MahasiswaId}", dto);

            Assert.Equal(HttpStatusCode.Conflict, respons.StatusCode);
            Assert.Equal("NIM sudah digunakan mahasiswa lain.", await BacaPesan(respons));
        }

        [Fact]
        public async Task Update_DosenWaliTidakAda_Mengembalikan400()
        {
            var buat = await _klien.PostAsJsonAsync("api/mahasiswa", MahasiswaBaru());
            var mhs = await buat.Content.ReadFromJsonAsync<MahasiswaDto>();

            var dto = MahasiswaBaru(nim: mhs!.Nim, dosenId: 9999);
            var respons = await _klien.PutAsJsonAsync($"api/mahasiswa/{mhs.MahasiswaId}", dto);

            Assert.Equal(HttpStatusCode.BadRequest, respons.StatusCode);
            Assert.Contains("tidak ditemukan", await BacaPesan(respons));
        }

        // ---------- DELETE ----------

        [Fact]
        public async Task Delete_DataMahasiswaAda_Mengembalikan204()
        {
            var buat = await _klien.PostAsJsonAsync("api/mahasiswa", MahasiswaBaru());
            var mhs = await buat.Content.ReadFromJsonAsync<MahasiswaDto>();

            var respons = await _klien.DeleteAsync($"api/mahasiswa/{mhs!.MahasiswaId}");

            Assert.Equal(HttpStatusCode.NoContent, respons.StatusCode);

            var daftar = await _klien.GetFromJsonAsync<List<MahasiswaDto>>("api/mahasiswa");
            Assert.Equal(5, daftar!.Count); // kembali ke jumlah seed
        }

        [Fact]
        public async Task Delete_DataTidakAda_Mengembalikan404()
        {
            var respons = await _klien.DeleteAsync("api/mahasiswa/9999");

            Assert.Equal(HttpStatusCode.NotFound, respons.StatusCode);
        }
    }
}
