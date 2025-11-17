using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace mkBoutiqueCaftan.Data;

public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        
        // Chaîne de connexion par défaut pour les migrations
        // Vous pouvez la modifier selon votre configuration
        var connectionString = "Server=localhost;Database=mkBoutiqueCaftan;User=root;Password=root;Port=3306;";
        
        optionsBuilder.UseMySql(connectionString, 
            new MySqlServerVersion(new Version(10, 11, 0))); // Version MariaDB par défaut
        
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}

