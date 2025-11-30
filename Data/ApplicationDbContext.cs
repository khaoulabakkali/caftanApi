using Microsoft.EntityFrameworkCore;
using mkBoutiqueCaftan.Models;

namespace mkBoutiqueCaftan.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Societe> Societes { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Taille> Tailles { get; set; }
    public DbSet<Categorie> Categories { get; set; }
    public DbSet<Article> Articles { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Paiement> Paiements { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<ReservationArticle> ReservationArticles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration de l'entité Societe
        modelBuilder.Entity<Societe>(entity =>
        {
            entity.ToTable("Societes");
            entity.HasKey(e => e.IdSociete);
            
            entity.Property(e => e.IdSociete)
                .HasColumnName("id_societe")
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.NomSociete)
                .HasColumnName("nom_societe")
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasColumnType("TEXT");
            
            entity.Property(e => e.Adresse)
                .HasColumnName("adresse")
                .HasColumnType("TEXT");
            
            entity.Property(e => e.Telephone)
                .HasColumnName("telephone")
                .HasMaxLength(20);
            
            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(100);
            
            entity.Property(e => e.SiteWeb)
                .HasColumnName("site_web")
                .HasMaxLength(255);
            
            entity.Property(e => e.Logo)
                .HasColumnName("logo")
                .HasMaxLength(255);
            
            entity.Property(e => e.Actif)
                .HasColumnName("actif")
                .IsRequired()
                .HasDefaultValue(true);
            
            entity.Property(e => e.DateCreation)
                .HasColumnName("date_creation")
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Index unique sur NomSociete
            entity.HasIndex(e => e.NomSociete)
                .IsUnique()
                .HasDatabaseName("IX_Societes_NomSociete");

            // Index unique sur Email (si fourni)
            entity.HasIndex(e => e.Email)
                .IsUnique()
                .HasDatabaseName("IX_Societes_Email")
                .HasFilter("[email] IS NOT NULL");
        });

        // Configuration de l'entité Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.IdRole);
            
            entity.Property(e => e.IdRole)
                .HasColumnName("id_role")
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.IdSociete)
                .HasColumnName("id_societe")
                .IsRequired();
            
            entity.Property(e => e.NomRole)
                .HasColumnName("nom_role")
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasMaxLength(255);
            
            entity.Property(e => e.Actif)
                .HasColumnName("actif")
                .IsRequired()
                .HasDefaultValue(true);

            // Index unique sur NomRole et IdSociete
            entity.HasIndex(e => new { e.NomRole, e.IdSociete })
                .IsUnique()
                .HasDatabaseName("IX_Roles_NomRole_IdSociete");
        });

        // Configuration de l'entité User
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.IdUtilisateur);
            
            entity.Property(e => e.IdUtilisateur)
                .HasColumnName("id_utilisateur")
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.NomComplet)
                .HasColumnName("nom_complet")
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Login)
                .HasColumnName("login")
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.Email)
                .HasColumnName("email")
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.MotDePasseHash)
                .HasColumnName("mot_de_passe_hash")
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.IdRole)
                .HasColumnName("id_role")
                .IsRequired();
            
            entity.Property(e => e.IdSociete)
                .HasColumnName("id_societe")
                .IsRequired();
            
            entity.Property(e => e.Telephone)
                .HasColumnName("telephone")
                .HasMaxLength(20);
            
            entity.Property(e => e.Actif)
                .HasColumnName("actif")
                .IsRequired()
                .HasDefaultValue(true);
            
            entity.Property(e => e.DateCreationCompte)
                .HasColumnName("date_creation_compte")
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Index unique sur Login et IdSociete (composite)
            entity.HasIndex(e => new { e.Login, e.IdSociete })
                .IsUnique()
                .HasDatabaseName("IX_Users_Login_Societe");
            
            // Index unique sur Email et IdSociete (composite)
            entity.HasIndex(e => new { e.Email, e.IdSociete })
                .IsUnique()
                .HasDatabaseName("IX_Users_Email_Societe");

            // Relation avec Role
            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.IdRole)
                .OnDelete(DeleteBehavior.Restrict);

            // Relation avec Societe
            entity.HasOne(u => u.Societe)
                .WithMany()
                .HasForeignKey(u => u.IdSociete)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuration de l'entité Taille
        modelBuilder.Entity<Taille>(entity =>
        {
            entity.ToTable("Tailles");
            entity.HasKey(e => e.IdTaille);
            
            entity.Property(e => e.IdTaille)
                .HasColumnName("id_taille")
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.Libelle)
                .HasColumnName("taille")
                .IsRequired()
                .HasMaxLength(20);

            entity.Property(e => e.IdSociete)
                .HasColumnName("id_societe")
                .IsRequired();

            // Index unique composite sur Libelle et IdSociete
            entity.HasIndex(e => new { e.Libelle, e.IdSociete })
                .IsUnique()
                .HasDatabaseName("IX_Tailles_Libelle_Societe");
            
            // Index sur IdSociete
            entity.HasIndex(e => e.IdSociete)
                .HasDatabaseName("IX_Tailles_id_societe");

            // Relation avec Societe
            entity.HasOne(t => t.Societe)
                .WithMany()
                .HasForeignKey(t => t.IdSociete)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuration de l'entité Categorie
        modelBuilder.Entity<Categorie>(entity =>
        {
            entity.ToTable("Categories");
            entity.HasKey(e => e.IdCategorie);
            
            entity.Property(e => e.IdCategorie)
                .HasColumnName("id_categorie")
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.NomCategorie)
                .HasColumnName("nom_categorie")
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.Description)
                .HasColumnName("description")
                .HasColumnType("TEXT");
            
            entity.Property(e => e.OrdreAffichage)
                .HasColumnName("ordre_affichage");

            entity.Property(e => e.IdSociete)
                .HasColumnName("id_societe")
                .IsRequired();

            // Index unique composite sur NomCategorie et IdSociete
            entity.HasIndex(e => new { e.NomCategorie, e.IdSociete })
                .IsUnique()
                .HasDatabaseName("IX_Categories_NomCategorie_Societe");
            
            // Index sur IdSociete
            entity.HasIndex(e => e.IdSociete)
                .HasDatabaseName("IX_Categories_id_societe");

            // Relation avec Societe
            entity.HasOne(c => c.Societe)
                .WithMany()
                .HasForeignKey(c => c.IdSociete)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuration de l'entité Article
        modelBuilder.Entity<Article>(entity =>
        {
            entity.ToTable("Articles");
            entity.HasKey(e => e.IdArticle);
            
            entity.Property(e => e.IdArticle)
                .HasColumnName("id_article")
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.NomArticle)
                .HasColumnName("nom_article")
                .IsRequired()
                .HasMaxLength(150);
            
            entity.Property(e => e.Description)
                .HasColumnName("description")
                .IsRequired()
                .HasColumnType("TEXT");
            
            entity.Property(e => e.PrixLocationBase)
                .HasColumnName("prix_location_base")
                .IsRequired()
                .HasColumnType("DECIMAL(10,2)");
            
            entity.Property(e => e.PrixAvanceBase)
                .HasColumnName("prix_avance_base")
                .IsRequired()
                .HasColumnType("DECIMAL(10,2)");
            
            entity.Property(e => e.IdTaille)
                .HasColumnName("id_taille");
            
            entity.Property(e => e.Couleur)
                .HasColumnName("couleur")
                .HasMaxLength(50);
            
            entity.Property(e => e.Photo)
                .HasColumnName("photo")
                .HasColumnType("LONGTEXT");
            
            entity.Property(e => e.IdCategorie)
                .HasColumnName("id_categorie")
                .IsRequired();
            
            entity.Property(e => e.IdSociete)
                .HasColumnName("id_societe")
                .IsRequired();
            
            entity.Property(e => e.Actif)
                .HasColumnName("actif")
                .IsRequired()
                .HasDefaultValue(true);
            
            // Index sur IdSociete
            entity.HasIndex(e => e.IdSociete)
                .HasDatabaseName("IX_Articles_id_societe");

            // Relation avec Societe
            entity.HasOne(a => a.Societe)
                .WithMany()
                .HasForeignKey(a => a.IdSociete)
                .OnDelete(DeleteBehavior.Restrict);

            // Relations
            entity.HasOne(a => a.Taille)
                .WithMany()
                .HasForeignKey(a => a.IdTaille)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(a => a.Categorie)
                .WithMany()
                .HasForeignKey(a => a.IdCategorie)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuration de l'entité Client
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");
            entity.HasKey(e => e.IdClient);
            
            entity.Property(e => e.IdClient)
                .HasColumnName("id_client")
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.NomClient)
                .HasColumnName("nom_client")
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.PrenomClient)
                .HasColumnName("prenom_client")
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(e => e.Telephone)
                .HasColumnName("telephone")
                .IsRequired()
                .HasMaxLength(20);
            
            entity.Property(e => e.Email)
                .HasColumnName("email")
                .HasMaxLength(100);
            
            entity.Property(e => e.AdressePrincipale)
                .HasColumnName("adresse_principale")
                .HasColumnType("TEXT");
            
            entity.Property(e => e.IdSociete)
                .HasColumnName("id_societe")
                .IsRequired();
            
            entity.Property(e => e.TotalCommandes)
                .HasColumnName("total_commandes")
                .IsRequired()
                .HasDefaultValue(0);
            
            entity.Property(e => e.DateCreationFiche)
                .HasColumnName("date_creation_fiche")
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.Actif)
                .HasColumnName("actif")
                .IsRequired()
                .HasDefaultValue(true);

            // Index unique composite sur Telephone et IdSociete
            entity.HasIndex(e => new { e.Telephone, e.IdSociete })
                .IsUnique()
                .HasDatabaseName("IX_Clients_Telephone_Societe");
            
            // Index sur IdSociete
            entity.HasIndex(e => e.IdSociete)
                .HasDatabaseName("IX_Clients_id_societe");

            // Relation avec Societe
            entity.HasOne(c => c.Societe)
                .WithMany()
                .HasForeignKey(c => c.IdSociete)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuration de l'entité Paiement
        modelBuilder.Entity<Paiement>(entity =>
        {
            entity.ToTable("Paiements");
            entity.HasKey(e => e.IdPaiement);
            
            entity.Property(e => e.IdPaiement)
                .HasColumnName("id_paiement")
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.IdReservation)
                .HasColumnName("id_reservation")
                .IsRequired();
            
            entity.Property(e => e.Montant)
                .HasColumnName("montant")
                .IsRequired()
                .HasColumnType("DECIMAL(10,2)");
            
            entity.Property(e => e.DatePaiement)
                .HasColumnName("date_paiement")
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.MethodePaiement)
                .HasColumnName("methode_paiement")
                .HasMaxLength(50);
            
            entity.Property(e => e.Reference)
                .HasColumnName("reference")
                .HasMaxLength(100);

            entity.Property(e => e.IdSociete)
                .HasColumnName("id_societe")
                .IsRequired();

            // Index sur IdSociete
            entity.HasIndex(e => e.IdSociete)
                .HasDatabaseName("IX_Paiements_id_societe");

            // Relation avec Societe
            entity.HasOne(p => p.Societe)
                .WithMany()
                .HasForeignKey(p => p.IdSociete)
                .OnDelete(DeleteBehavior.Restrict);

            // Relation avec Reservation (Many-to-One)
            entity.HasOne(p => p.Reservation)
                .WithOne(r => r.Paiement)
                .HasForeignKey<Paiement>(p => p.IdReservation)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configuration de l'entité Reservation
        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.ToTable("Reservations");
            entity.HasKey(e => e.IdReservation);
            
            entity.Property(e => e.IdReservation)
                .HasColumnName("id_reservation")
                .ValueGeneratedOnAdd();
            
            entity.Property(e => e.IdClient)
                .HasColumnName("id_client")
                .IsRequired();
            
            entity.Property(e => e.DateReservation)
                .HasColumnName("date_reservation")
                .IsRequired()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
            entity.Property(e => e.DateDebut)
                .HasColumnName("date_debut")
                .IsRequired()
                .HasColumnType("DATE");
            
            entity.Property(e => e.DateFin)
                .HasColumnName("date_fin")
                .IsRequired()
                .HasColumnType("DATE");
            
            entity.Property(e => e.MontantTotal)
                .HasColumnName("montant_total")
                .IsRequired()
                .HasColumnType("DECIMAL(10,2)");
            
            entity.Property(e => e.StatutReservation)
                .HasColumnName("statut_reservation")
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(StatutReservation.EnAttente);
            
            entity.Property(e => e.IdPaiement)
                .HasColumnName("id_paiement");
            
            entity.Property(e => e.RemiseAppliquee)
                .HasColumnName("remise_appliquee")
                .IsRequired()
                .HasColumnType("DECIMAL(10,2)")
                .HasDefaultValue(0.00m);

            entity.Property(e => e.IdSociete)
                .HasColumnName("id_societe")
                .IsRequired();

            // Index sur IdSociete
            entity.HasIndex(e => e.IdSociete)
                .HasDatabaseName("IX_Reservations_id_societe");

            entity.Property(e => e.IdArticle)
                .HasColumnName("id_article");

            // Relations
            entity.HasOne(r => r.Client)
                .WithMany(c => c.Reservations)
                .HasForeignKey(r => r.IdClient)
                .OnDelete(DeleteBehavior.Restrict);

            // Relation avec Article
            entity.HasOne(r => r.Article)
                .WithMany()
                .HasForeignKey(r => r.IdArticle)
                .OnDelete(DeleteBehavior.Restrict);

            // Relation avec Societe
            entity.HasOne(r => r.Societe)
                .WithMany()
                .HasForeignKey(r => r.IdSociete)
                .OnDelete(DeleteBehavior.Restrict);

            // Relation avec Paiement (One-to-One optionnel)
            entity.HasOne(r => r.Paiement)
                .WithOne(p => p.Reservation)
                .HasForeignKey<Reservation>(r => r.IdPaiement)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configuration de la table de liaison ReservationArticle (Many-to-Many)
        modelBuilder.Entity<ReservationArticle>(entity =>
        {
            entity.ToTable("ReservationArticles");
            entity.HasKey(ra => new { ra.IdReservation, ra.IdArticle });

            entity.Property(ra => ra.IdReservation)
                .HasColumnName("id_reservation")
                .IsRequired();

            entity.Property(ra => ra.IdArticle)
                .HasColumnName("id_article")
                .IsRequired();

            entity.Property(ra => ra.Quantite)
                .HasColumnName("quantite")
                .IsRequired()
                .HasDefaultValue(1);

            // Relation avec Reservation
            entity.HasOne(ra => ra.Reservation)
                .WithMany(r => r.ReservationArticles)
                .HasForeignKey(ra => ra.IdReservation)
                .OnDelete(DeleteBehavior.Cascade);

            // Relation avec Article
            entity.HasOne(ra => ra.Article)
                .WithMany()
                .HasForeignKey(ra => ra.IdArticle)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

