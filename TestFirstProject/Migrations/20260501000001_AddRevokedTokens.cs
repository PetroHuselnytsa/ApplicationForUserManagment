using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable enable

namespace TestFirstProject.Migrations
{
    /// <inheritdoc />
    public partial class AddRevokedTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "revoked_tokens",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    jti = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    revoked_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_revoked_tokens", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_revoked_tokens_jti",
                table: "revoked_tokens",
                column: "jti",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_revoked_tokens_expires_at",
                table: "revoked_tokens",
                column: "expires_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "revoked_tokens");
        }
    }
}
