using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mkBoutiqueCaftan.Migrations
{
    /// <inheritdoc />
    public partial class AddReservationsTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Articles",
                columns: table => new
                {
                    id_article = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nom_article = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    prix_location_base = table.Column<decimal>(type: "DECIMAL(10,2)", nullable: false),
                    prix_avance_base = table.Column<decimal>(type: "DECIMAL(10,2)", nullable: false),
                    id_taille = table.Column<int>(type: "int", nullable: true),
                    couleur = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    photo = table.Column<string>(type: "LONGTEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    id_categorie = table.Column<int>(type: "int", nullable: false),
                    id_societe = table.Column<int>(type: "int", nullable: false),
                    actif = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Articles", x => x.id_article);
                    table.ForeignKey(
                        name: "FK_Articles_Categories_id_categorie",
                        column: x => x.id_categorie,
                        principalTable: "Categories",
                        principalColumn: "id_categorie",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Articles_Tailles_id_taille",
                        column: x => x.id_taille,
                        principalTable: "Tailles",
                        principalColumn: "id_taille",
                        onDelete: ReferentialAction.SetNull);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    id_client = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    nom_complet = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    telephone = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    adresse = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    id_societe = table.Column<int>(type: "int", nullable: false),
                    actif = table.Column<bool>(type: "tinyint(1)", nullable: false, defaultValue: true),
                    date_creation = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.id_client);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    id_reservation = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_client = table.Column<int>(type: "int", nullable: false),
                    date_reservation = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    date_debut = table.Column<DateTime>(type: "date", nullable: false),
                    date_fin = table.Column<DateTime>(type: "date", nullable: false),
                    montant_total = table.Column<decimal>(type: "DECIMAL(10,2)", nullable: false),
                    statut_reservation = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    id_paiement = table.Column<int>(type: "int", nullable: true),
                    remise_appliquee = table.Column<decimal>(type: "DECIMAL(10,2)", nullable: false, defaultValue: 0.00m),
                    id_societe = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.id_reservation);
                    table.ForeignKey(
                        name: "FK_Reservations_Clients_id_client",
                        column: x => x.id_client,
                        principalTable: "Clients",
                        principalColumn: "id_client",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Paiements",
                columns: table => new
                {
                    id_paiement = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    id_reservation = table.Column<int>(type: "int", nullable: false),
                    montant = table.Column<decimal>(type: "DECIMAL(10,2)", nullable: false),
                    date_paiement = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    methode_paiement = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    reference = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    id_societe = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Paiements", x => x.id_paiement);
                    table.ForeignKey(
                        name: "FK_Paiements_Reservations_id_reservation",
                        column: x => x.id_reservation,
                        principalTable: "Reservations",
                        principalColumn: "id_reservation",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_id_paiement",
                table: "Reservations",
                column: "id_paiement");

            migrationBuilder.AddForeignKey(
                name: "FK_Reservations_Paiements_id_paiement",
                table: "Reservations",
                column: "id_paiement",
                principalTable: "Paiements",
                principalColumn: "id_paiement",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.CreateIndex(
                name: "IX_Articles_id_categorie",
                table: "Articles",
                column: "id_categorie");

            migrationBuilder.CreateIndex(
                name: "IX_Articles_id_taille",
                table: "Articles",
                column: "id_taille");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_id_client",
                table: "Reservations",
                column: "id_client");

            migrationBuilder.CreateIndex(
                name: "IX_Paiements_id_reservation",
                table: "Paiements",
                column: "id_reservation");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Paiements");

            migrationBuilder.DropTable(
                name: "Articles");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "Clients");
        }
    }
}
