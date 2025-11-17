using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ElegantnailsstudioSystemManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "cupos",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    turno = table.Column<string>(type: "text", nullable: false),
                    cupo_maximo = table.Column<int>(type: "integer", nullable: false),
                    cupo_actual = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cupos", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    permisos = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_roles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "servicios",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    nombre = table.Column<string>(type: "text", nullable: false),
                    precio = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    duracion = table.Column<int>(type: "integer", nullable: false),
                    imagen = table.Column<string>(type: "text", nullable: true),
                    estado = table.Column<bool>(type: "boolean", nullable: false),
                    categoria_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_servicios", x => x.id);
                    table.ForeignKey(
                        name: "FK_servicios_categorias_categoria_id",
                        column: x => x.categoria_id,
                        principalTable: "categorias",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Password = table.Column<string>(type: "text", nullable: false),
                    Telefono = table.Column<string>(type: "text", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Estado = table.Column<bool>(type: "boolean", nullable: false),
                    RolId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_usuarios_roles_RolId",
                        column: x => x.RolId,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "citas",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    turno = table.Column<string>(type: "text", nullable: false),
                    estado = table.Column<string>(type: "text", nullable: false),
                    notas = table.Column<string>(type: "text", nullable: true),
                    fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    cliente_id = table.Column<int>(type: "integer", nullable: false),
                    servicio_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_citas", x => x.id);
                    table.ForeignKey(
                        name: "FK_citas_servicios_servicio_id",
                        column: x => x.servicio_id,
                        principalTable: "servicios",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_citas_usuarios_cliente_id",
                        column: x => x.cliente_id,
                        principalTable: "usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "categorias",
                columns: new[] { "id", "estado", "nombre" },
                values: new object[,]
                {
                    { 1, "Activo", "Uñas" },
                    { 2, "Activo", "Pestañas" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "nombre", "permisos" },
                values: new object[,]
                {
                    { 1, "Administrador", "gestionar_servicios,gestionar_categorias,gestionar_citas,gestionar_cupos" },
                    { 2, "Cliente", "agendar_cita,cancelar_cita,ver_servicios" }
                });

            migrationBuilder.InsertData(
                table: "servicios",
                columns: new[] { "id", "categoria_id", "duracion", "estado", "imagen", "nombre", "precio" },
                values: new object[,]
                {
                    { 1, 1, 30, true, null, "Manicure Básico", 8.00m },
                    { 2, 1, 45, true, null, "Pedicure Spa", 12.00m },
                    { 3, 2, 90, true, null, "Extensiones de Pestañas", 25.00m }
                });

            migrationBuilder.InsertData(
                table: "usuarios",
                columns: new[] { "Id", "Email", "Estado", "FechaRegistro", "Nombre", "Password", "RolId", "Telefono" },
                values: new object[] { 1, "admin@elegantnails.com", true, new DateTime(2025, 11, 13, 22, 53, 31, 395, DateTimeKind.Utc).AddTicks(3277), "Administrador", "$2a$11$XysbeHpSlbB7dpckrw9MguXDpPHKO.o1w3joM1U22ym.P5lSrtPpC", 1, "76872677" });

            migrationBuilder.CreateIndex(
                name: "IX_citas_cliente_id",
                table: "citas",
                column: "cliente_id");

            migrationBuilder.CreateIndex(
                name: "IX_citas_servicio_id",
                table: "citas",
                column: "servicio_id");

            migrationBuilder.CreateIndex(
                name: "IX_servicios_categoria_id",
                table: "servicios",
                column: "categoria_id");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_RolId",
                table: "usuarios",
                column: "RolId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "citas");

            migrationBuilder.DropTable(
                name: "cupos");

            migrationBuilder.DropTable(
                name: "servicios");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "roles");
        }
    }
}
