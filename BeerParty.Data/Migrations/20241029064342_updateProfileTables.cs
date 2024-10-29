using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BeerParty.Data.Migrations
{
    /// <inheritdoc />
    public partial class updateProfileTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProfileInteraction",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FromProfileId = table.Column<long>(type: "bigint", nullable: false),
                    ToProfileId = table.Column<long>(type: "bigint", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProfileInteraction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProfileInteraction_Profiles_FromProfileId",
                        column: x => x.FromProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProfileInteraction_Profiles_ToProfileId",
                        column: x => x.ToProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProfileInteraction_FromProfileId",
                table: "ProfileInteraction",
                column: "FromProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ProfileInteraction_ToProfileId",
                table: "ProfileInteraction",
                column: "ToProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProfileInteraction");
        }
    }
}
