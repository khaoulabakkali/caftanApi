using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mkBoutiqueCaftan.Migrations
{
    /// <inheritdoc />
    public partial class AddIdSocieteToRoles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Roles_NomRole",
                table: "Roles");

            migrationBuilder.RenameColumn(
                name: "IdSociete",
                table: "Roles",
                newName: "id_societe");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_NomRole_IdSociete",
                table: "Roles",
                columns: new[] { "nom_role", "id_societe" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Roles_NomRole_IdSociete",
                table: "Roles");

            migrationBuilder.RenameColumn(
                name: "id_societe",
                table: "Roles",
                newName: "IdSociete");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_NomRole",
                table: "Roles",
                column: "nom_role",
                unique: true);
        }
    }
}
