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
    Task<IEnumerable<UserDto>> GetUtilisateursByRoleAsync(int roleId);
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

            // Récupérer ou créer une société par défaut
            var societe = await _context.Societes.FirstOrDefaultAsync();
            if (societe == null)
            {
                // Créer une société par défaut
                societe = new Societe
                {
                    NomSociete = "Société Par Défaut",
                    Description = "Société créée par défaut lors de l'initialisation",
                    Actif = true,
                    DateCreation = DateTime.Now
                };
                _context.Societes.Add(societe);
                await _context.SaveChangesAsync();
            }

            var defaultRoles = new List<Role>
            {
                new Role
                {
                    IdSociete = societe.IdSociete,
                    NomRole = "ADMIN",
                    Description = "Administrateur avec tous les droits",
                    Actif = true
                },
                new Role
                {
                    IdSociete = societe.IdSociete,
                    NomRole = "MANAGER",
                    Description = "Gestionnaire avec droits de gestion",
                    Actif = true
                },
                new Role
                {
                    IdSociete = societe.IdSociete,
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
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.IdRole == id);
        return role == null ? null : MapToDto(role);
    }

    public async Task<RoleDto> CreateRoleAsync(CreateRoleRequest request)
    {
        // Vérifier si un rôle avec le même nom existe déjà dans la même société
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.NomRole.ToLower() == request.NomRole.ToLower() && r.IdSociete == request.IdSociete);
        
        if (existingRole != null)
        {
            throw new InvalidOperationException($"Un rôle avec le nom '{request.NomRole}' existe déjà dans cette société.");
        }

        var role = new Role
        {
            IdSociete = request.IdSociete,
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
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.IdRole == id);
        if (role == null)
        {
            return null;
        }

        // Vérifier si un autre rôle avec le même nom existe déjà dans la même société
        var existingRole = await _context.Roles
            .FirstOrDefaultAsync(r => r.NomRole.ToLower() == request.NomRole.ToLower() && r.IdRole != id && r.IdSociete == (request.IdSociete ?? role.IdSociete));
        
        if (existingRole != null)
        {
            throw new InvalidOperationException($"Un rôle avec le nom '{request.NomRole}' existe déjà dans cette société.");
        }

        if (request.IdSociete.HasValue)
        {
            role.IdSociete = request.IdSociete.Value;
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
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.IdRole == id);
        if (role == null)
        {
            return false;
        }

        role.Actif = !role.Actif;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<IEnumerable<UserDto>> GetUtilisateursByRoleAsync(int roleId)
    {
        // Vérifier que le rôle existe
        var role = await _context.Roles.FindAsync(roleId);
        if (role == null)
        {
            throw new InvalidOperationException($"Rôle avec l'ID {roleId} introuvable.");
        }

        var users = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.IdRole == roleId)
            .ToListAsync();

        return users.Select(MapUserToDto);
    }

    private static RoleDto MapToDto(Role role)
    {
        return new RoleDto
        {
            IdRole = role.IdRole,
            IdSociete = role.IdSociete,
            NomRole = role.NomRole,
            Description = role.Description,
            Actif = role.Actif
        };
    }

    private static UserDto MapUserToDto(User user)
    {
        return new UserDto
        {
            IdUtilisateur = user.IdUtilisateur,
            NomComplet = user.NomComplet,
            Login = user.Login,
            Email = user.Email,
            IdRole = user.IdRole,
            Telephone = user.Telephone,
            Actif = user.Actif,
            DateCreationCompte = user.DateCreationCompte,
            Role = user.Role != null ? new RoleDto
            {
                IdRole = user.Role.IdRole,
                IdSociete = user.Role.IdSociete,
                NomRole = user.Role.NomRole,
                Description = user.Role.Description,
                Actif = user.Role.Actif
            } : null
        };
    }
}

