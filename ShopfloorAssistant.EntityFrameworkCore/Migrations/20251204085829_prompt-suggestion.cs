using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFrameworkCore.Migrations
{
    /// <inheritdoc />
    public partial class promptsuggestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PromptSuggestions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Score = table.Column<double>(type: "float", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PromptSuggestions", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "PromptSuggestions",
                columns: new[] { "Id", "Prompt", "Description", "Score", "Metadata", "CreationTime", "LastModificationTime" },
                values: new object[,]
                {
                    {
                        new Guid("6b1f3d10-8a2e-4c0a-9f4a-1a2b3c4d5e01"),
                        "Información sobre la maquina OvenHeatMax-500",
                        "Pregunta para identificar causas y acciones rápidas para reducir tiempo de inactividad.",
                        0.95,
                        "{\"source\":\"system\",\"tags\":[\"downtime\",\"production\"]}",
                        new DateTime(2025, 12, 04, 08, 58, 29, DateTimeKind.Utc),
                        null
                    },
                    {
                        new Guid("7c2f4e21-9b3f-5d1b-af5b-2b3c4d5e6f02"),
                        "Listado de tareas JIRA",
                        "Sugerencia orientada a inspecciones y mantenimiento preventivo.",
                        0.82,
                        "{\"source\":\"curated\",\"tags\":[\"maintenance\",\"conveyor\"]}",
                        new DateTime(2025, 12, 04, 08, 58, 29, DateTimeKind.Utc),
                        null
                    },
                    {
                        new Guid("8d3f5f32-ac4a-6e2c-bf6c-3c4d5e6f7a03"),
                        "Envíame un mail con el resumen de la conversación",
                        "Sugerencia para obtener métricas clave y acciones de mejora.",
                        0.88,
                        "{\"source\":\"user\",\"tags\":[\"kpi\",\"throughput\"]}",
                        new DateTime(2025, 12, 04, 08, 58, 29, DateTimeKind.Utc),
                        null
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PromptSuggestions");
        }
    }
}
