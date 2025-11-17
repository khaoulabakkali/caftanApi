using Microsoft.EntityFrameworkCore;
using mkBoutiqueCaftan.Data;
using mkBoutiqueCaftan.Models;

namespace mkBoutiqueCaftan.Services;

public interface IRoleService
{
    Task InitializeDefaultRolesAsync();
}

public class RoleService : IRoleService
{
    private readonly ApplicationDbContext _context;

    public RoleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task InitializeDefaultRolesAsync()
    {
        try
        {
            // Vérifier si la table existe en essayant de compter les rôles
            var roleCount = await _context.Roles.CountAsync();
            
            // Si des rôles existent déjà, ne rien faire
            if (roleCount > 0)
            {
                return;
            }

            var defaultRoles = new List<Role>
            {
                new Role
                {
                    NomRole = "ADMIN",
                    Description = "Administrateur avec tous les droits",
                    Actif = true
                },
                new Role
                {
                    NomRole = "MANAGER",
                    Description = "Gestionnaire avec droits de gestion",
                    Actif = true
                },
                new Role
                {
                    NomRole = "STAFF",
                    Description = "Employé avec droits de base",
                    Actif = true
                }
            };

            _context.Roles.AddRange(defaultRoles);
            await _context.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Si la table n'existe pas encore, on ignore l'erreur
            // Les migrations doivent être appliquées d'abord
            throw;
        }
    }
}

