using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SiakadSederhana.API.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Dosen",
                columns: table => new
                {
                    DosenId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nidn = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    NamaDosen = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Prodi = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dosen", x => x.DosenId);
                });

            migrationBuilder.CreateTable(
                name: "Mahasiswa",
                columns: table => new
                {
                    MahasiswaId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nim = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    NamaMahasiswa = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Angkatan = table.Column<int>(type: "INTEGER", nullable: false),
                    DosenId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mahasiswa", x => x.MahasiswaId);
                    table.ForeignKey(
                        name: "FK_Mahasiswa_Dosen_DosenId",
                        column: x => x.DosenId,
                        principalTable: "Dosen",
                        principalColumn: "DosenId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Dosen",
                columns: new[] { "DosenId", "Email", "NamaDosen", "Nidn", "Prodi" },
                values: new object[,]
                {
                    { 1, "anita@kampus.ac.id", "Dr. Anita Wijaya", "0512345678", "Sistem Informasi" },
                    { 2, "budi@kampus.ac.id", "Budi Hartono, M.T.", "0587654321", "Informatika" },
                    { 3, "siti@kampus.ac.id", "Siti Rahma, M.Kom.", "0511223344", "Sistem Informasi" }
                });

            migrationBuilder.InsertData(
                table: "Mahasiswa",
                columns: new[] { "MahasiswaId", "Angkatan", "DosenId", "Email", "NamaMahasiswa", "Nim" },
                values: new object[,]
                {
                    { 1, 2023, 1, "budi@student.ac.id", "Budi Santoso", "72240001" },
                    { 2, 2023, 1, "citra@student.ac.id", "Citra Dewi", "72240002" },
                    { 3, 2024, 2, "eko@student.ac.id", "Eko Prasetyo", "72240003" },
                    { 4, 2022, 2, "fani@student.ac.id", "Fani Kurnia", "72240004" },
                    { 5, 2025, 3, "gita@student.ac.id", "Gita Gutawa", "72240005" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Dosen_Nidn",
                table: "Dosen",
                column: "Nidn",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Mahasiswa_DosenId",
                table: "Mahasiswa",
                column: "DosenId");

            migrationBuilder.CreateIndex(
                name: "IX_Mahasiswa_Nim",
                table: "Mahasiswa",
                column: "Nim",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Mahasiswa");

            migrationBuilder.DropTable(
                name: "Dosen");
        }
    }
}
