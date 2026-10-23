using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sisman.Mantenimiento.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sisman");

            migrationBuilder.CreateTable(
                name: "Catalogo",
                schema: "sisman",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Categoria = table.Column<byte>(type: "tinyint", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Catalogo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Correlativo",
                schema: "sisman",
                columns: table => new
                {
                    Nombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    UltimoValor = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Correlativo", x => x.Nombre);
                });

            migrationBuilder.CreateTable(
                name: "Planta",
                schema: "sisman",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Planta", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuario",
                schema: "sisman",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    NombreUsuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ClaveHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Perfil = table.Column<byte>(type: "tinyint", nullable: false),
                    Activo = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Area",
                schema: "sisman",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PlantaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Area", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Area_Planta_PlantaId",
                        column: x => x.PlantaId,
                        principalSchema: "sisman",
                        principalTable: "Planta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Maquina",
                schema: "sisman",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AreaId = table.Column<int>(type: "int", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Maquina", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Maquina_Area_AreaId",
                        column: x => x.AreaId,
                        principalSchema: "sisman",
                        principalTable: "Area",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Sistema",
                schema: "sisman",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaquinaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sistema", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sistema_Maquina_MaquinaId",
                        column: x => x.MaquinaId,
                        principalSchema: "sisman",
                        principalTable: "Maquina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Equipo",
                schema: "sisman",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SistemaId = table.Column<int>(type: "int", nullable: false),
                    Nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipo_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "sisman",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MantenimientoPreventivo",
                schema: "sisman",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Codigo = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaquinaId = table.Column<int>(type: "int", nullable: false),
                    SistemaId = table.Column<int>(type: "int", nullable: true),
                    EquipoId = table.Column<int>(type: "int", nullable: true),
                    TipoTrabajoId = table.Column<int>(type: "int", nullable: false),
                    FuenteId = table.Column<int>(type: "int", nullable: false),
                    OportunidadId = table.Column<int>(type: "int", nullable: false),
                    EspecialidadId = table.Column<int>(type: "int", nullable: false),
                    Frecuencia = table.Column<byte>(type: "tinyint", nullable: false),
                    ValorFrecuencia = table.Column<int>(type: "int", nullable: false),
                    Mes = table.Column<byte>(type: "tinyint", nullable: true),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaProxima = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaUltima = table.Column<DateOnly>(type: "date", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModificadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AprobadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AprobadoPor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MantenimientoPreventivo", x => x.Id);
                    table.CheckConstraint("CK_MP_Frecuencia", "[Frecuencia] BETWEEN 1 AND 4");
                    table.CheckConstraint("CK_MP_Mes", "[Frecuencia] NOT IN (3, 4) OR [Mes] BETWEEN 1 AND 12");
                    table.CheckConstraint("CK_MP_ValorFrecuencia", "[ValorFrecuencia] > 0");
                    table.ForeignKey(
                        name: "FK_MantenimientoPreventivo_Catalogo_EspecialidadId",
                        column: x => x.EspecialidadId,
                        principalSchema: "sisman",
                        principalTable: "Catalogo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MantenimientoPreventivo_Catalogo_FuenteId",
                        column: x => x.FuenteId,
                        principalSchema: "sisman",
                        principalTable: "Catalogo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MantenimientoPreventivo_Catalogo_OportunidadId",
                        column: x => x.OportunidadId,
                        principalSchema: "sisman",
                        principalTable: "Catalogo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MantenimientoPreventivo_Catalogo_TipoTrabajoId",
                        column: x => x.TipoTrabajoId,
                        principalSchema: "sisman",
                        principalTable: "Catalogo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MantenimientoPreventivo_Equipo_EquipoId",
                        column: x => x.EquipoId,
                        principalSchema: "sisman",
                        principalTable: "Equipo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MantenimientoPreventivo_Maquina_MaquinaId",
                        column: x => x.MaquinaId,
                        principalSchema: "sisman",
                        principalTable: "Maquina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MantenimientoPreventivo_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "sisman",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenTrabajo",
                schema: "sisman",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Numero = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<byte>(type: "tinyint", nullable: false),
                    Prioridad = table.Column<byte>(type: "tinyint", nullable: false),
                    Estado = table.Column<byte>(type: "tinyint", nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    MaquinaId = table.Column<int>(type: "int", nullable: false),
                    SistemaId = table.Column<int>(type: "int", nullable: true),
                    EquipoId = table.Column<int>(type: "int", nullable: true),
                    TipoTrabajoId = table.Column<int>(type: "int", nullable: false),
                    FuenteId = table.Column<int>(type: "int", nullable: false),
                    OportunidadId = table.Column<int>(type: "int", nullable: false),
                    EspecialidadId = table.Column<int>(type: "int", nullable: false),
                    MantenimientoPreventivoId = table.Column<int>(type: "int", nullable: false),
                    FechaObjetivo = table.Column<DateOnly>(type: "date", nullable: false),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ModificadoEn = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Version = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenTrabajo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenTrabajo_Catalogo_EspecialidadId",
                        column: x => x.EspecialidadId,
                        principalSchema: "sisman",
                        principalTable: "Catalogo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenTrabajo_Catalogo_FuenteId",
                        column: x => x.FuenteId,
                        principalSchema: "sisman",
                        principalTable: "Catalogo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenTrabajo_Catalogo_OportunidadId",
                        column: x => x.OportunidadId,
                        principalSchema: "sisman",
                        principalTable: "Catalogo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenTrabajo_Catalogo_TipoTrabajoId",
                        column: x => x.TipoTrabajoId,
                        principalSchema: "sisman",
                        principalTable: "Catalogo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenTrabajo_Equipo_EquipoId",
                        column: x => x.EquipoId,
                        principalSchema: "sisman",
                        principalTable: "Equipo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenTrabajo_MantenimientoPreventivo_MantenimientoPreventivoId",
                        column: x => x.MantenimientoPreventivoId,
                        principalSchema: "sisman",
                        principalTable: "MantenimientoPreventivo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenTrabajo_Maquina_MaquinaId",
                        column: x => x.MaquinaId,
                        principalSchema: "sisman",
                        principalTable: "Maquina",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrdenTrabajo_Sistema_SistemaId",
                        column: x => x.SistemaId,
                        principalSchema: "sisman",
                        principalTable: "Sistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FechaProgramada",
                schema: "sisman",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MantenimientoPreventivoId = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: false),
                    OrdenTrabajoId = table.Column<int>(type: "int", nullable: true),
                    CreadoEn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FechaProgramada", x => x.Id);
                    table.CheckConstraint("CK_Fecha_Rango", "[FechaInicio] <= [FechaFin]");
                    table.ForeignKey(
                        name: "FK_FechaProgramada_MantenimientoPreventivo_MantenimientoPreventivoId",
                        column: x => x.MantenimientoPreventivoId,
                        principalSchema: "sisman",
                        principalTable: "MantenimientoPreventivo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FechaProgramada_OrdenTrabajo_OrdenTrabajoId",
                        column: x => x.OrdenTrabajoId,
                        principalSchema: "sisman",
                        principalTable: "OrdenTrabajo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OrdenTrabajoHistorial",
                schema: "sisman",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OrdenTrabajoId = table.Column<int>(type: "int", nullable: false),
                    EstadoAnterior = table.Column<byte>(type: "tinyint", nullable: false),
                    EstadoNuevo = table.Column<byte>(type: "tinyint", nullable: false),
                    Usuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenTrabajoHistorial", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrdenTrabajoHistorial_OrdenTrabajo_OrdenTrabajoId",
                        column: x => x.OrdenTrabajoId,
                        principalSchema: "sisman",
                        principalTable: "OrdenTrabajo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Area_PlantaId",
                schema: "sisman",
                table: "Area",
                column: "PlantaId");

            migrationBuilder.CreateIndex(
                name: "IX_Catalogo_Categoria_Nombre",
                schema: "sisman",
                table: "Catalogo",
                columns: new[] { "Categoria", "Nombre" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipo_SistemaId",
                schema: "sisman",
                table: "Equipo",
                column: "SistemaId");

            migrationBuilder.CreateIndex(
                name: "IX_FechaProgramada_MantenimientoPreventivoId_FechaInicio",
                schema: "sisman",
                table: "FechaProgramada",
                columns: new[] { "MantenimientoPreventivoId", "FechaInicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FechaProgramada_OrdenTrabajoId",
                schema: "sisman",
                table: "FechaProgramada",
                column: "OrdenTrabajoId",
                unique: true,
                filter: "[OrdenTrabajoId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MantenimientoPreventivo_Codigo",
                schema: "sisman",
                table: "MantenimientoPreventivo",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MantenimientoPreventivo_EquipoId",
                schema: "sisman",
                table: "MantenimientoPreventivo",
                column: "EquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_MantenimientoPreventivo_EspecialidadId",
                schema: "sisman",
                table: "MantenimientoPreventivo",
                column: "EspecialidadId");

            migrationBuilder.CreateIndex(
                name: "IX_MantenimientoPreventivo_FuenteId",
                schema: "sisman",
                table: "MantenimientoPreventivo",
                column: "FuenteId");

            migrationBuilder.CreateIndex(
                name: "IX_MantenimientoPreventivo_MaquinaId",
                schema: "sisman",
                table: "MantenimientoPreventivo",
                column: "MaquinaId");

            migrationBuilder.CreateIndex(
                name: "IX_MantenimientoPreventivo_OportunidadId",
                schema: "sisman",
                table: "MantenimientoPreventivo",
                column: "OportunidadId");

            migrationBuilder.CreateIndex(
                name: "IX_MantenimientoPreventivo_SistemaId",
                schema: "sisman",
                table: "MantenimientoPreventivo",
                column: "SistemaId");

            migrationBuilder.CreateIndex(
                name: "IX_MantenimientoPreventivo_TipoTrabajoId",
                schema: "sisman",
                table: "MantenimientoPreventivo",
                column: "TipoTrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_Maquina_AreaId",
                schema: "sisman",
                table: "Maquina",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Maquina_Codigo",
                schema: "sisman",
                table: "Maquina",
                column: "Codigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajo_EquipoId",
                schema: "sisman",
                table: "OrdenTrabajo",
                column: "EquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajo_EspecialidadId",
                schema: "sisman",
                table: "OrdenTrabajo",
                column: "EspecialidadId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajo_FuenteId",
                schema: "sisman",
                table: "OrdenTrabajo",
                column: "FuenteId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajo_MantenimientoPreventivoId",
                schema: "sisman",
                table: "OrdenTrabajo",
                column: "MantenimientoPreventivoId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajo_MaquinaId",
                schema: "sisman",
                table: "OrdenTrabajo",
                column: "MaquinaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajo_Numero",
                schema: "sisman",
                table: "OrdenTrabajo",
                column: "Numero",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajo_OportunidadId",
                schema: "sisman",
                table: "OrdenTrabajo",
                column: "OportunidadId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajo_SistemaId",
                schema: "sisman",
                table: "OrdenTrabajo",
                column: "SistemaId");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajo_TipoTrabajoId",
                schema: "sisman",
                table: "OrdenTrabajo",
                column: "TipoTrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_OT_Estado_Numero",
                schema: "sisman",
                table: "OrdenTrabajo",
                columns: new[] { "Estado", "Numero" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenTrabajoHistorial_OrdenTrabajoId",
                schema: "sisman",
                table: "OrdenTrabajoHistorial",
                column: "OrdenTrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_Sistema_MaquinaId",
                schema: "sisman",
                table: "Sistema",
                column: "MaquinaId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuario_NombreUsuario",
                schema: "sisman",
                table: "Usuario",
                column: "NombreUsuario",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Correlativo",
                schema: "sisman");

            migrationBuilder.DropTable(
                name: "FechaProgramada",
                schema: "sisman");

            migrationBuilder.DropTable(
                name: "OrdenTrabajoHistorial",
                schema: "sisman");

            migrationBuilder.DropTable(
                name: "Usuario",
                schema: "sisman");

            migrationBuilder.DropTable(
                name: "OrdenTrabajo",
                schema: "sisman");

            migrationBuilder.DropTable(
                name: "MantenimientoPreventivo",
                schema: "sisman");

            migrationBuilder.DropTable(
                name: "Catalogo",
                schema: "sisman");

            migrationBuilder.DropTable(
                name: "Equipo",
                schema: "sisman");

            migrationBuilder.DropTable(
                name: "Sistema",
                schema: "sisman");

            migrationBuilder.DropTable(
                name: "Maquina",
                schema: "sisman");

            migrationBuilder.DropTable(
                name: "Area",
                schema: "sisman");

            migrationBuilder.DropTable(
                name: "Planta",
                schema: "sisman");
        }
    }
}
