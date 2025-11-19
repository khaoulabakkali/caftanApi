using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mkBoutiqueCaftan.Migrations
{
    /// <inheritdoc />
    public partial class RemoveIdSocieteFromAllTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email_Societe",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Login_Societe",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Tailles_Taille_Societe",
                table: "Tailles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_NomRole_Societe",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Telephone_Societe",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Categories_NomCategorie_Societe",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "id_societe",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "id_societe",
                table: "Tailles");

            migrationBuilder.DropColumn(
                name: "id_societe",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "id_societe",
                table: "Reservations");

            migrationBuilder.DropColumn(
                name: "id_societe",
                table: "Paiements");

            migrationBuilder.DropColumn(
                name: "id_societe",
                table: "Clients");

            migrationBuilder.DropColumn(
                name: "id_societe",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "id_societe",
                table: "Articles");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Login",
                table: "Users",
                column: "login",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tailles_Taille",
                table: "Tailles",
                column: "taille",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_NomRole",
                table: "Roles",
                column: "nom_role",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Telephone",
                table: "Clients",
                column: "telephone",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_NomCategorie",
                table: "Categories",
                column: "nom_categorie",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Login",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Tailles_Taille",
                table: "Tailles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_NomRole",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Clients_Telephone",
                table: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Categories_NomCategorie",
                table: "Categories");

            migrationBuilder.AddColumn<int>(
                name: "id_societe",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "id_societe",
                table: "Tailles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "id_societe",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "id_societe",
                table: "Reservations",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "id_societe",
                table: "Paiements",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "id_societe",
                table: "Clients",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "id_societe",
                table: "Categories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "id_societe",
                table: "Articles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email_Societe",
                table: "Users",
                columns: new[] { "email", "id_societe" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Login_Societe",
                table: "Users",
                columns: new[] { "login", "id_societe" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tailles_Taille_Societe",
                table: "Tailles",
                columns: new[] { "taille", "id_societe" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_NomRole_Societe",
                table: "Roles",
                columns: new[] { "nom_role", "id_societe" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clients_Telephone_Societe",
                table: "Clients",
                columns: new[] { "telephone", "id_societe" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_NomCategorie_Societe",
                table: "Categories",
                columns: new[] { "nom_categorie", "id_societe" },
                unique: true);
        }
    }
}
