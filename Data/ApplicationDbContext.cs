using Microsoft.EntityFrameworkCore;
using mkBoutiqueCaftan.Models;

namespace mkBoutiqueCaftan.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuration de l'entité Role
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.HasKey(e => e.IdRole);
            
            entity.Property(e => e.IdRole)
                .HasColumnName("id_role")
                .ValueGeneratedOnAdd();
            
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

            // Index unique sur NomRole
            entity.HasIndex(e => e.NomRole)
                .IsUnique()
                .HasDatabaseName("IX_Roles_NomRole");
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
            
            entity.Property(e => e.MotDePasseHash)
                .HasColumnName("mot_de_passe_hash")
                .IsRequired()
                .HasMaxLength(255);
            
            entity.Property(e => e.IdRole)
                .HasColumnName("id_role")
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

            // Index unique sur Login
            entity.HasIndex(e => e.Login)
                .IsUnique()
                .HasDatabaseName("IX_Users_Login");

            // Relation avec Role
            entity.HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.IdRole)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

