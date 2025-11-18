using Microsoft.EntityFrameworkCore;
using mkBoutiqueCaftan.Data;
using mkBoutiqueCaftan.Models;

namespace mkBoutiqueCaftan.Services;

public interface IRoleService
{
    Task InitializeDefaultRolesAsync();
    Task<IEnumerable<RoleDto>> GetAllRolesAsync(bool includeInactive = false);
    Task<RoleDto?> GetRoleByIdAsync(int id);
    Task<RoleDto> CreateRoleAsync(CreateRoleRequest request);
    Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleRequest request);
    Task<bool> DeleteRoleAsync(int id);
    Task<bool> ToggleRoleStatusAsync(int id);
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

    public async Task<IEnumerable<RoleDto>> GetAllRolesAsync(bool includeInactive = false)
    {
        var query = _context.Roles.AsQueryable();
        
        if (!includeInactive)
        {
            query = query.Where(r => r.Actif);
        }

        var roles = await query.ToListAsync();
        return roles.Select(MapToDto);
    }

    public async Task<RoleDto?> GetRoleByIdAsync(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        return role == null ? null : MapToDto(role);
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
    {
        // Vérifier si un rôle avec le même nom existe déjà
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.NomRole.ToLower() == request.NomRole.ToLower());
        
        if (existingRole != null)
        {
            throw new InvalidOperationException($"Un rôle avec le nom '{request.NomRole}' existe déjà.");
        }

        var role = new Role
        {
            NomRole = request.NomRole,
            Description = request.Description,
            Actif = request.Actif
        };

        _context.Roles.Add(role);
        await _context.SaveChangesAsync();

        return MapToDto(role);
    }

    public async Task<RoleDto?> UpdateRoleAsync(int id, UpdateRoleRequest request)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return null;
        }

        // Vérifier si un autre rôle avec le même nom existe déjà
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.NomRole.ToLower() == request.NomRole.ToLower() && r.IdRole != id);
        
        if (existingRole != null)
        {
            throw new InvalidOperationException($"Un rôle avec le nom '{request.NomRole}' existe déjà.");
        }

        role.NomRole = request.NomRole;
        role.Description = request.Description;
        
        if (request.Actif.HasValue)
        {
            role.Actif = request.Actif.Value;
        }

        await _context.SaveChangesAsync();

        return MapToDto(role);
    }

    public async Task<bool> DeleteRoleAsync(int id)
    {
        var role = await _context.Roles
            .Include(r => r.Users)
            .FirstOrDefaultAsync(r => r.IdRole == id);
        
        if (role == null)
        {
            return false;
        }

        // Vérifier si le rôle est utilisé par des utilisateurs
        if (role.Users.Any())
        {
            throw new InvalidOperationException($"Le rôle '{role.NomRole}' ne peut pas être supprimé car il est utilisé par {role.Users.Count} utilisateur(s).");
        }

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ToggleRoleStatusAsync(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
        {
            return false;
        }

        role.Actif = !role.Actif;
        await _context.SaveChangesAsync();

        return true;
    }

    private static RoleDto MapToDto(Role role)
    {
        return new RoleDto
        {
            IdRole = role.IdRole,
            NomRole = role.NomRole,
            Description = role.Description,
            Actif = role.Actif
        };
    }
}

