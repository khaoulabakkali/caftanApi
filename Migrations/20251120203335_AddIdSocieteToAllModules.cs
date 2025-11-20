using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mkBoutiqueCaftan.Migrations
{
    /// <inheritdoc />
    public partial class AddIdSocieteToAllModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fonction helper pour ajouter une colonne id_societe si elle n'existe pas
            var addIdSocieteColumn = @"
                SET @col_exists = (
                    SELECT COUNT(*) 
                    FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_SCHEMA = DATABASE() 
                    AND TABLE_NAME = '{0}' 
                    AND COLUMN_NAME = 'id_societe'
                );
                
                SET @sql = IF(@col_exists = 0,
                    'ALTER TABLE {0} ADD COLUMN id_societe INT NOT NULL DEFAULT 0',
                    'SELECT ''Column already exists'' AS message'
                );
                
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ";

            // Ajouter id_societe à toutes les tables
            migrationBuilder.Sql(string.Format(addIdSocieteColumn, "Articles"));
            migrationBuilder.Sql(string.Format(addIdSocieteColumn, "Categories"));
            migrationBuilder.Sql(string.Format(addIdSocieteColumn, "Tailles"));
            migrationBuilder.Sql(string.Format(addIdSocieteColumn, "Reservations"));
            migrationBuilder.Sql(string.Format(addIdSocieteColumn, "Paiements"));

            // Mettre à jour les données existantes avec un id_societe valide
            migrationBuilder.Sql(@"
                SET @first_societe_id = (SELECT id_societe FROM Societes LIMIT 1);
                IF @first_societe_id IS NULL THEN
                    INSERT INTO Societes (nom_societe, description, adresse, telephone, email, site_web, logo, actif, date_creation)
                    VALUES ('Default Societe', 'Description par défaut', NULL, NULL, NULL, NULL, NULL, TRUE, NOW());
                    SET @first_societe_id = LAST_INSERT_ID();
                END IF;
                UPDATE Articles SET id_societe = @first_societe_id WHERE id_societe = 0;
                UPDATE Categories SET id_societe = @first_societe_id WHERE id_societe = 0;
                UPDATE Tailles SET id_societe = @first_societe_id WHERE id_societe = 0;
                UPDATE Reservations SET id_societe = @first_societe_id WHERE id_societe = 0;
                UPDATE Paiements SET id_societe = @first_societe_id WHERE id_societe = 0;
            ");

            // Supprimer les anciens index uniques s'ils existent
            migrationBuilder.Sql(@"
                SET @index_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Tailles' AND INDEX_NAME = 'IX_Tailles_Taille');
                SET @sql = IF(@index_exists > 0, 'ALTER TABLE Tailles DROP INDEX IX_Tailles_Taille', 'SELECT ''Index does not exist'' AS message');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            migrationBuilder.Sql(@"
                SET @index_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Categories' AND INDEX_NAME = 'IX_Categories_NomCategorie');
                SET @sql = IF(@index_exists > 0, 'ALTER TABLE Categories DROP INDEX IX_Categories_NomCategorie', 'SELECT ''Index does not exist'' AS message');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");

            // Créer les nouveaux index composites et simples
            migrationBuilder.Sql(@"
                SET @index_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Tailles' AND INDEX_NAME = 'IX_Tailles_Libelle_Societe');
                IF @index_exists = 0 THEN
                    CREATE UNIQUE INDEX `IX_Tailles_Libelle_Societe` ON `Tailles` (`taille`, `id_societe`);
                END IF;
            ");

            migrationBuilder.Sql(@"
                SET @index_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Categories' AND INDEX_NAME = 'IX_Categories_NomCategorie_Societe');
                IF @index_exists = 0 THEN
                    CREATE UNIQUE INDEX `IX_Categories_NomCategorie_Societe` ON `Categories` (`nom_categorie`, `id_societe`);
                END IF;
            ");

            // Créer les index simples sur id_societe
            var createIndex = @"
                SET @index_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{0}' AND INDEX_NAME = '{1}');
                IF @index_exists = 0 THEN
                    CREATE INDEX `{1}` ON `{0}` (`id_societe`);
                END IF;
            ";

            migrationBuilder.Sql(string.Format(createIndex, "Articles", "IX_Articles_id_societe"));
            migrationBuilder.Sql(string.Format(createIndex, "Categories", "IX_Categories_id_societe"));
            migrationBuilder.Sql(string.Format(createIndex, "Tailles", "IX_Tailles_id_societe"));
            migrationBuilder.Sql(string.Format(createIndex, "Reservations", "IX_Reservations_id_societe"));
            migrationBuilder.Sql(string.Format(createIndex, "Paiements", "IX_Paiements_id_societe"));

            // Ajouter les clés étrangères
            var addForeignKey = @"
                SET @fk_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{0}' AND CONSTRAINT_NAME = '{1}');
                SET @sql = IF(@fk_exists = 0,
                    'ALTER TABLE {0} ADD CONSTRAINT {1} FOREIGN KEY (id_societe) REFERENCES Societes(id_societe) ON DELETE RESTRICT',
                    'SELECT ''Foreign key already exists'' AS message'
                );
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ";

            migrationBuilder.Sql(string.Format(addForeignKey, "Articles", "FK_Articles_Societes_id_societe"));
            migrationBuilder.Sql(string.Format(addForeignKey, "Categories", "FK_Categories_Societes_id_societe"));
            migrationBuilder.Sql(string.Format(addForeignKey, "Tailles", "FK_Tailles_Societes_id_societe"));
            migrationBuilder.Sql(string.Format(addForeignKey, "Reservations", "FK_Reservations_Societes_id_societe"));
            migrationBuilder.Sql(string.Format(addForeignKey, "Paiements", "FK_Paiements_Societes_id_societe"));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Supprimer les clés étrangères
            var dropForeignKey = @"
                SET @fk_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{0}' AND CONSTRAINT_NAME = '{1}');
                SET @sql = IF(@fk_exists > 0, 'ALTER TABLE {0} DROP FOREIGN KEY {1}', 'SELECT ''Foreign key does not exist'' AS message');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ";

            migrationBuilder.Sql(string.Format(dropForeignKey, "Articles", "FK_Articles_Societes_id_societe"));
            migrationBuilder.Sql(string.Format(dropForeignKey, "Categories", "FK_Categories_Societes_id_societe"));
            migrationBuilder.Sql(string.Format(dropForeignKey, "Tailles", "FK_Tailles_Societes_id_societe"));
            migrationBuilder.Sql(string.Format(dropForeignKey, "Reservations", "FK_Reservations_Societes_id_societe"));
            migrationBuilder.Sql(string.Format(dropForeignKey, "Paiements", "FK_Paiements_Societes_id_societe"));

            // Supprimer les index
            var dropIndex = @"
                SET @index_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{0}' AND INDEX_NAME = '{1}');
                SET @sql = IF(@index_exists > 0, 'ALTER TABLE {0} DROP INDEX {1}', 'SELECT ''Index does not exist'' AS message');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ";

            migrationBuilder.Sql(string.Format(dropIndex, "Articles", "IX_Articles_id_societe"));
            migrationBuilder.Sql(string.Format(dropIndex, "Categories", "IX_Categories_id_societe"));
            migrationBuilder.Sql(string.Format(dropIndex, "Categories", "IX_Categories_NomCategorie_Societe"));
            migrationBuilder.Sql(string.Format(dropIndex, "Tailles", "IX_Tailles_id_societe"));
            migrationBuilder.Sql(string.Format(dropIndex, "Tailles", "IX_Tailles_Libelle_Societe"));
            migrationBuilder.Sql(string.Format(dropIndex, "Reservations", "IX_Reservations_id_societe"));
            migrationBuilder.Sql(string.Format(dropIndex, "Paiements", "IX_Paiements_id_societe"));

            // Supprimer les colonnes
            var dropColumn = @"
                SET @col_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = '{0}' AND COLUMN_NAME = 'id_societe');
                SET @sql = IF(@col_exists > 0, 'ALTER TABLE {0} DROP COLUMN id_societe', 'SELECT ''Column does not exist'' AS message');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ";

            migrationBuilder.Sql(string.Format(dropColumn, "Articles"));
            migrationBuilder.Sql(string.Format(dropColumn, "Categories"));
            migrationBuilder.Sql(string.Format(dropColumn, "Tailles"));
            migrationBuilder.Sql(string.Format(dropColumn, "Reservations"));
            migrationBuilder.Sql(string.Format(dropColumn, "Paiements"));

            // Recréer les anciens index uniques
            migrationBuilder.Sql(@"
                SET @index_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Tailles' AND INDEX_NAME = 'IX_Tailles_Taille');
                IF @index_exists = 0 THEN
                    CREATE UNIQUE INDEX `IX_Tailles_Taille` ON `Tailles` (`taille`);
                END IF;
            ");

            migrationBuilder.Sql(@"
                SET @index_exists = (SELECT COUNT(*) FROM INFORMATION_SCHEMA.STATISTICS WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Categories' AND INDEX_NAME = 'IX_Categories_NomCategorie');
                IF @index_exists = 0 THEN
                    CREATE UNIQUE INDEX `IX_Categories_NomCategorie` ON `Categories` (`nom_categorie`);
                END IF;
            ");
        }
    }
}
