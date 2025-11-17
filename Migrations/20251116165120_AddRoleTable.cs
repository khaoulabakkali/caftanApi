using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mkBoutiqueCaftan.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Créer d'abord la table Roles
            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    id_role = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nom_role = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    actif = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.id_role);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_NomRole",
                table: "Roles",
                column: "nom_role",
                unique: true);

            // Insérer les rôles par défaut
            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "nom_role", "description", "actif" },
                values: new object[,]
                {
                    { "ADMIN", "Administrateur avec tous les droits", true },
                    { "MANAGER", "Gestionnaire avec droits de gestion", true },
                    { "STAFF", "Employé avec droits de base", true }
                });

            // Modifier la table Users
            migrationBuilder.DropIndex(
                name: "IX_Users_Email",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Username",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "Username",
                table: "Users",
                newName: "login");

            migrationBuilder.RenameColumn(
                name: "Password",
                table: "Users",
                newName: "mot_de_passe_hash");

            migrationBuilder.RenameColumn(
                name: "Id",
                table: "Users",
                newName: "id_utilisateur");

            migrationBuilder.AddColumn<bool>(
                name: "actif",
                table: "Users",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "date_creation_compte",
                table: "Users",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddColumn<string>(
                name: "nom_complet",
                table: "Users",
                type: "varchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "id_role",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 1); // Valeur par défaut = STAFF (id 1 après insertion)

            migrationBuilder.AddColumn<string>(
                name: "telephone",
                table: "Users",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Modifier la colonne login pour avoir la bonne longueur
            migrationBuilder.AlterColumn<string>(
                name: "login",
                table: "Users",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Users_id_role",
                table: "Users",
                column: "id_role");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Login",
                table: "Users",
                column: "login",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_id_role",
                table: "Users",
                column: "id_role",
                principalTable: "Roles",
                principalColumn: "id_role",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_id_role",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Users_id_role",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_Login",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "actif",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "date_creation_compte",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "id_role",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "telephone",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "nom_complet",
                table: "Users");

            migrationBuilder.RenameColumn(
                name: "login",
                table: "Users",
                newName: "Username");

            migrationBuilder.RenameColumn(
                name: "mot_de_passe_hash",
                table: "Users",
                newName: "Password");

            migrationBuilder.RenameColumn(
                name: "id_utilisateur",
                table: "Users",
                newName: "Id");

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Users",
                type: "varchar(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Username",
                table: "Users",
                column: "Username",
                unique: true);
        }
    }
}
