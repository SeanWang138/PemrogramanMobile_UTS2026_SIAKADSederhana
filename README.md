# SiAkademik — Sistem Informasi Akademik Sederhana (SIAKAD)

### Proyek UTS Pemrograman Mobile — .NET MAUI Blazor Hybrid & ASP.NET Core Web API (.NET 10)

Aplikasi Sistem Informasi Akademik Sederhana berbasis arsitektur *client-server* terpisah:

1. **`SiakadSederhana.API`** — Backend RESTful API: **ASP.NET Core Web API (.NET 10)** + **EF Core (SQLite)** + Swagger.
2. **`SiakadSederhana`** — Frontend lintas platform: **.NET MAUI Blazor Hybrid (.NET 10)**, berjalan di Windows Desktop dan Android.
3. **`SiakadSederhana.API.Tests`** — Proyek integration test (xUnit + `WebApplicationFactory`), **29 test**.

```text
┌──────────────────────────┐        HTTP (REST/JSON)        ┌───────────────────────────┐
│  SiakadSederhana (MAUI)  │  ◄──────────────────────────►  │  SiakadSederhana.API      │
│  Blazor Hybrid + Services│     http://localhost:5103      │  Controllers + EF Core    │
└──────────────────────────┘                                └─────────────┬─────────────┘
                                                                          ▼
                                                                   siakad.db (SQLite)
```

---

## 🛠️ Prasyarat & Teknologi

| Komponen | Keterangan |
|---|---|
| **.NET 10 SDK** | Wajib. Unduh: <https://dotnet.microsoft.com/download/dotnet/10.0> |
| **Workload MAUI** | Wajib untuk build frontend. Cek/instal: `dotnet workload install maui` |
| **Visual Studio 2026** (opsional) | Edisi apa pun, dengan workload *.NET Multi-platform App UI development* — dipakai untuk F5/debug |
| **Android SDK + Emulator** (opsional) | Hanya jika ingin menjalankan target Android |
| **Database** | SQLite — **tidak perlu instal apa pun**, file `siakad.db` dibuat otomatis |

**Stack:** ASP.NET Core Web API · EF Core (SQLite) · Swagger/OpenAPI · .NET MAUI Blazor Hybrid (Razor + Bootstrap) · xUnit.

**Paket NuGet utama:**

* Backend: `Microsoft.AspNetCore.OpenApi`, `Microsoft.EntityFrameworkCore.Sqlite`, `Microsoft.EntityFrameworkCore.Tools`, `Swashbuckle.AspNetCore`
* Test: `Microsoft.AspNetCore.Mvc.Testing`, `xunit`, `xunit.runner.visualstudio`, `coverlet.collector`

> Semua versi paket sudah dikunci di file `.csproj` masing-masing proyek — cukup jalankan `dotnet restore` (otomatis saat build pertama).

---

## 🚀 Cara Menjalankan (Langkah Demi Langkah)

### Langkah 0 — Clone repositori

```bash
git clone https://github.com/SeanWang138/PemrogramanMobile_UTS2026_SIAKADSederhana.git
cd PemrogramanMobile_UTS2026_SIAKADSederhana
```

### Langkah 1 — Jalankan Backend API

```bash
cd SiakadSederhana.API
dotnet run --launch-profile http
```

Perhatikan pesan di terminal — ini tanda API hidup:

```text
Now listening on: http://localhost:5103
```

* Saat pertama dijalankan, database `siakad.db` + migrasi + **data seed** (3 dosen, 5 mahasiswa) dibuat **otomatis** — tidak ada langkah manual sama sekali.
* Cek di browser: **<http://localhost:5103/swagger>** — seluruh endpoint bisa dilihat dan dicoba langsung dari Swagger.
* **Biarkan terminal API ini tetap terbuka** selama memakai aplikasi.
* *(Alternatif: buka `SiakadSederhana.slnx` di Visual Studio, jadikan `SiakadSederhana.API` startup project, lalu tekan F5.)*

### Langkah 2 — Jalankan Frontend MAUI Blazor Hybrid

**Jalur A — Visual Studio (paling mudah):**

1. Buka `SiakadSederhana.slnx` di Visual Studio.
2. Pilih startup project **`SiakadSederhana`** (bukan `.API`).
3. Pilih target **Windows Machine** (toolbar atas), lalu **F5**.
4. Jendela aplikasi **SiAkademik** terbuka.

**Jalur B — Terminal:**

```bash
# dari folder root repo
dotnet run --project SiakadSederhana/SiakadSederhana.csproj -f net10.0-windows10.0.19041.0
```

### Langkah 3 — Verifikasi cepat

| Tanda | Arti |
|---|---|
| Dashboard menampilkan **angka statistik** (jumlah dosen, mahasiswa, dsb.) | ✅ Frontend ↔ API tersambung — semuanya siap |
| Dashboard menampilkan **panel merah "API tidak dapat dihubungi"** | ❌ Backend belum jalan / salah alamat — kembali ke Langkah 1 |

Setelah itu silakan mencoba: **Beranda** (dashboard) → **Dosen** (tambah/edit/hapus) → **Mahasiswa** (tambah dengan dropdown dosen wali, pencarian, pagination).

---

## 📱 Yang Perlu Disesuaikan di Device Anda

Alamat API dibedakan **otomatis per platform** lewat kode kondisional di `SiakadSederhana/MauiProgram.cs`:

```csharp
#if ANDROID
var apiBaseAddress = "http://10.0.2.2:5103";   // emulator Android → host PC
#else
var apiBaseAddress = "http://localhost:5103";  // Windows Desktop
#endif
builder.Services.AddSingleton(_ => new HttpClient
{
    BaseAddress = new Uri(apiBaseAddress),
    Timeout = TimeSpan.FromSeconds(15)
});
```

Tabel penyesuaian — **cek kondisi Anda**:

| Kondisi di komputer/perangkat Anda | Yang harus dilakukan |
|---|---|
| **Windows Desktop** (jalur utama) | **Tidak ada perubahan apa pun.** Jika Windows Firewall menampilkan dialog izin jaringan saat pertama kali API dijalankan, klik **Izinkan** (port 5103). |
| **Android Emulator** (AVD) | **Tidak ada perubahan** — kode otomatis memakai `10.0.2.2` (alias host PC dari dalam emulator). Pastikan API berjalan di PC sebelum membuka app. |
| **HP Android fisik (USB)** | Jalankan `adb reverse tcp:5103 tcp:5103` sekali setelah HP terdeteksi, **lalu ubah** `10.0.2.2` pada blok `#if ANDROID` di `MauiProgram.cs` menjadi `localhost`. |
| **HP Android fisik (satu Wi-Fi)** | Cek IP PC (`ipconfig` → IPv4, mis. `192.168.1.5`), **lalu ubah** `10.0.2.2` menjadi IP tersebut → `http://192.168.1.5:5103`. PC dan HP harus satu jaringan; izinkan firewall port 5103 untuk jaringan privat. |
| **Port 5103 sudah terpakai program lain** | Ubah di **dua tempat agar selalu sinkron**: (1) `SiakadSederhana.API/Properties/launchSettings.json` → `applicationUrl`; (2) `MauiProgram.cs` → `apiBaseAddress`. Contoh port 5200: `http://localhost:5200`. |
| **Build frontend gagal: "file is being used by another process"** | Aplikasi MAUI masih terbuka — **tutup jendela app dulu**, lalu build ulang. |
| **Ingin reset data ke bawaan** | Tutup API, hapus file `SiakadSederhana.API/siakad.db`, jalankan ulang API — migrasi + seed dibuat kembali otomatis. |

> Catatan: API sengaja memakai **HTTP** (profile `http`, port 5103) agar tidak ada kendala sertifikat *self-signed* HTTPS di emulator/perangkat. Profile `https` (port 7005) tetap tersedia tetapi tidak diperlukan.

---

## 🏗️ Struktur Proyek

```text
SiakadSederhana/                        # Root repository
├── .gitignore
├── .gitattributes
├── SiakadSederhana.slnx                # Solution file (.NET 10, format XML)
│
├── SiakadSederhana.API/                # ── Backend Web API ──
│   ├── Controllers/
│   │   ├── DosenController.cs          # CRUD Dosen + proteksi relasi wali
│   │   └── MahasiswaController.cs      # CRUD Mahasiswa + DTO proyeksi NamaDosenWali
│   ├── Models/
│   │   ├── Dosen.cs                    # Entitas Dosen
│   │   └── Mahasiswa.cs                # Entitas Mahasiswa (relasi ke Dosen)
│   ├── DTOs/
│   │   └── AkademikDtos.cs             # DTO request/response + DataAnnotations
│   ├── Data/
│   │   ├── SiakadDbContext.cs           # DbContext, relasi 1-N, seeding
│   │   └── Migrations/                 # Migrasi EF Core (InitialCreate)
│   ├── Properties/launchSettings.json  # Port API: http://localhost:5103
│   ├── appsettings.json
│   └── Program.cs                      # Pipeline: Swagger + migrasi + seed otomatis
│
├── SiakadSederhana/                    # ── Frontend MAUI Blazor Hybrid ──
│   ├── MauiProgram.cs                  # DI + BaseAddress HttpClient per platform
│   ├── Components/
│   │   ├── Layout/
│   │   │   ├── MainLayout.razor        # Kerangka layout + ToastHost
│   │   │   └── NavMenu.razor           # Navbar "SiAkademik" + logo + menu modul
│   │   ├── Pages/
│   │   │   ├── Home.razor              # Dashboard statistik + widget analitik
│   │   │   ├── Dosen/
│   │   │   │   ├── DosenList.razor     # Tabel dosen + pagination/sorting
│   │   │   │   └── DosenForm.razor     # Form tambah/edit dosen
│   │   │   └── Mahasiswa/
│   │   │       ├── MahasiswaList.razor # Tabel mahasiswa + pencarian
│   │   │       └── MahasiswaForm.razor # Form mahasiswa + dropdown dosen wali
│   │   └── Shared/
│   │       ├── ConfirmDialog.razor     # Konfirmasi aksi hapus
│   │       ├── ToastHost.razor         # Notifikasi sukses/gagal
│   │       ├── LoadingSpinner.razor    # Indikator loading
│   │       ├── ErrorAlert.razor        # Panel error API
│   │       └── EmptyState.razor        # Tampilan data kosong
│   ├── Models/
│   │   ├── Dosen.cs                    # Model client + validasi UI
│   │   └── Mahasiswa.cs                # Model client + validasi UI
│   ├── Services/
│   │   ├── ApiService.cs               # Helper HTTP dasar (GET/POST/PUT/DELETE + pesan error)
│   │   ├── ApiResult.cs                # Bentuk hasil generic (sukses/gagal + payload)
│   │   ├── DosenService.cs             # Aksi CRUD dosen via HttpClient
│   │   ├── MahasiswaService.cs         # Aksi CRUD mahasiswa via HttpClient
│   │   └── ToastService.cs             # State notifikasi antar-halaman
│   └── wwwroot/
│       ├── index.html                  # <title>SiAkademik</title>
│       └── app.css                     # Gaya global + widget dashboard
│
└── SiakadSederhana.API.Tests/          # ── Integration Test ──
    ├── ApiTestFactory.cs               # WebApplicationFactory + SQLite in-memory
    ├── DosenControllerTests.cs         # 14 test endpoint dosen
    └── MahasiswaControllerTests.cs     # 15 test endpoint mahasiswa
```

---

## 🌐 Spesifikasi API

Base URL: `http://localhost:5103` — interaktif via **Swagger**: <http://localhost:5103/swagger>

### Endpoint

**Dosen — `/api/dosen`**

| Method | Route | Sukses | Kegagalan |
|---|---|---|---|
| GET | `/api/dosen` | `200 OK` | — |
| GET | `/api/dosen/{id}` | `200 OK` | `404 Not Found` |
| POST | `/api/dosen` | `201 Created` | `400 Bad Request` (validasi) · `409 Conflict` (NIDN duplikat) |
| PUT | `/api/dosen/{id}` | `204 No Content` | `400` · `404` · `409` (NIDN dipakai dosen lain) |
| DELETE | `/api/dosen/{id}` | `204 No Content` | `404` · `409 Conflict` (masih menjadi dosen wali mahasiswa) |

**Mahasiswa — `/api/mahasiswa`**

| Method | Route | Sukses | Kegagalan |
|---|---|---|---|
| GET | `/api/mahasiswa` | `200 OK` (tiap item memuat `namaDosenWali`) | — |
| GET | `/api/mahasiswa/{id}` | `200 OK` | `404 Not Found` |
| POST | `/api/mahasiswa` | `201 Created` | `400` · `409 Conflict` (NIM duplikat) |
| PUT | `/api/mahasiswa/{id}` | `204 No Content` | `400` · `404` · `409` (NIM dipakai mahasiswa lain) |
| DELETE | `/api/mahasiswa/{id}` | `204 No Content` | `404` |

### Aturan Bisnis & Validasi (server-side, `AkademikDtos.cs`)

| Field | Aturan | Respons gagal |
|---|---|---|
| NIDN / NIM | Wajib, maks. 20 karakter, **unik** (case-insensitive) | `400` / `409` |
| Nama, Email, Prodi | Wajib, maks. 100 karakter | `400 Bad Request` |
| Email | Harus format email valid (`EmailAddress` + regex) | `400 Bad Request` |
| Angkatan | Wajib, rentang **2020–2026** | `400 Bad Request` |
| Dosen wali | Wajib dipilih dan harus merujuk dosen yang ada | `400 Bad Request` |
| Hapus dosen | **Ditolak** selama masih menjadi dosen wali | `409 Conflict` + pesan jelas |
| `MahasiswaDto` | Proyeksi LINQ berisi `namaDosenWali` (string) — **tanpa circular JSON** | — |

> Validasi form di frontend hanya pelengkap UX; **otoritas validasi tetap di server**.

---

## ⭐ Fitur Bonus (10 poin)

| Bonus | Nilai | Penerapan di aplikasi |
|---|---|---|
| **Pencarian data mahasiswa** | 3 | Kotak pencarian di `MahasiswaList` — filter instan case-insensitive berdasarkan **NIM atau nama mahasiswa** |
| **Dashboard menampilkan data aktual** | 2 | `Home.razor` menghitung statistik langsung dari API: 3 kartu ringkasan, widget *Analisis Beban Dosen Wali* (dengan badge "Belum Ada Bimbingan"), widget *Distribusi Angkatan* 2020–2026 |
| **Pagination & sorting** | 2 | Pagination (**5 baris per halaman**) + sorting kolom pada tabel dosen & mahasiswa — dihitung di client dari data API: dosen (NIDN, nama, email, prodi) dan mahasiswa (NIM, nama, email, angkatan, dosen wali) |
| **Unit/integration test** | 3 | **29 integration test** (xUnit + `WebApplicationFactory` + SQLite in-memory) di `SiakadSederhana.API.Tests` |

---

## 🧪 Pengujian

### Matriks Skenario Manual (T01–T09)

| ID | Skenario | Hasil yang diharapkan | Status |
|---|---|---|---|
| **T01** | Tambah dosen valid | Data tersimpan & muncul di daftar dosen | ✅ Pass |
| **T02** | Tambah dosen dengan email invalid | Ditolak validasi email (`400 Bad Request`) | ✅ Pass |
| **T03** | Tambah dosen dengan NIDN duplikat | Ditolak API (`409 Conflict`) | ✅ Pass |
| **T04** | Tambah mahasiswa + pilih dosen wali | Tersimpan dengan relasi dosen wali yang dipilih | ✅ Pass |
| **T05** | Mengubah dosen wali mahasiswa | Nama dosen wali pada daftar mahasiswa ikut terbarui | ✅ Pass |
| **T06** | Hapus data mahasiswa | Data terhapus dari UI & database | ✅ Pass |
| **T07** | Hapus dosen yang masih menjadi wali | Ditolak API (`409 Conflict`) dengan pesan jelas | ✅ Pass |
| **T08** | Akses ID yang tidak ada | API merespons `404 Not Found` | ✅ Pass |
| **T09** | Backend offline/mati | Frontend menampilkan panel error yang jelas, aplikasi **tidak crash** | ✅ Pass |

### Integration Test Otomatis (29 test)

```bash
dotnet test SiakadSederhana.API.Tests/SiakadSederhana.API.Tests.csproj
```

* Memakai `WebApplicationFactory` — API dijalankan **in-process**, jadi **tidak perlu menyalakan API manual** dan tidak mengubah `siakad.db` (memakai SQLite *in-memory* per test class).
* Cakupan: CRUD dosen & mahasiswa, semua kode status (200/201/204/400/404/409), uniknya NIDN/NIM, proteksi hapus dosen wali, dan proyeksi `namaDosenWali`.
* Hasil terakhir: **29/29 lulus, 0 gagal**.

---

## ❓ Troubleshooting

| Gejala | Penyebab & solusi |
|---|---|
| Halaman dashboard menampilkan panel merah "API tidak dapat dihubungi" | API belum dijalankan, atau port berubah. Pastikan terminal API menampilkan `Now listening on: http://localhost:5103`. |
| `error MSB3027 ... file is being used by another process` | Aplikasi MAUI masih terbuka — tutup dulu, lalu build ulang. |
| `error NETSDK1147 ... workload maui not installed` | Jalankan `dotnet workload install maui`, atau instal workload *.NET Multi-platform App UI development* lewat Visual Studio Installer. |
| Build frontend gagal target Android | Pastikan Android SDK + Emulator terpasang via Visual Studio Device Manager, dan emulator sudah **menyala** sebelum menekan Run. |
| Port 5103 sudah dipakai program lain | Lihat tabel penyesuaian di atas (ubah di `launchSettings.json` **dan** `MauiProgram.cs`). |
| Data aneh/sudah berubah saat demo | Reset ke bawaan: tutup API → hapus `siakad.db` → jalankan API lagi (seed 3 dosen + 5 mahasiswa dibuat ulang). |

---

## 📦 Tautan Deliverable

* **Source code:** <https://github.com/SeanWang138/PemrogramanMobile_UTS2026_SIAKADSederhana>
* **Video demo:** *(tautan menyusul / lihat deskripsi repositori)*

---

## 🤖 Deklarasi Penggunaan Alat AI & Integritas Akademik

Sesuai dengan ketentuan integritas akademik tugas UTS, alat bantu AI digunakan untuk mendukung proses pengembangan aplikasi dengan rincian:

* **ChatGPT:** Digunakan untuk *brainstorming* konsep arsitektur *Client-Server*, perancangan struktur DTO, dan penyusunan draf dokumentasi/README.
* **Gemini / Cursor:** Digunakan sebagai asisten koding dalam pembuatan instruksi prompt requirement, generasi sintaks C# / Razor Blazor, validasi query LINQ, serta debugging penanganan error HTTP.

*Seluruh kode, logika bisnis, dan pengujian aplikasi telah dipahami, diverifikasi, dan diuji secara mandiri.*
