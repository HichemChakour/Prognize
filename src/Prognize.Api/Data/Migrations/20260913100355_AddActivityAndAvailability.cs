using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prognize.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddActivityAndAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "availability",
                table: "resource",
                type: "jsonb",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "activity",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    organization_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    duration_minutes = table.Column<int>(type: "integer", nullable: false),
                    priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 3),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "now()"),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    requirements = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_activity", x => x.id);
                    table.CheckConstraint("ck_activity_duration_minutes", "duration_minutes > 0");
                    table.CheckConstraint("ck_activity_priority", "priority BETWEEN 1 AND 5");
                });

            migrationBuilder.CreateIndex(
                name: "ix_activity_organization_id_name",
                table: "activity",
                columns: new[] { "organization_id", "name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "activity");

            migrationBuilder.DropColumn(
                name: "availability",
                table: "resource");
        }
    }
}
