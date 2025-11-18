using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mkBoutiqueCaftan.Models;
using mkBoutiqueCaftan.Services;

namespace mkBoutiqueCaftan.Controllers;

[ApiController]
[Route("api/roles")]
[Authorize]
public class RoleController : ControllerBase
{
    private readonly IRoleService _roleService;
    private readonly ILogger<RoleController> _logger;

    public RoleController(IRoleService roleService, ILogger<RoleController> logger)
    {
        _roleService = roleService;
        _logger = logger;
    }

    /// <summary>
    /// Récupère tous les rôles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetAllRoles([FromQuery] bool includeInactive = false)
    {
        try
        {
            var roles = await _roleService.GetAllRolesAsync(includeInactive);
            return Ok(roles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des rôles");
            return StatusCode(500, new { message = "Erreur lors de la récupération des rôles" });
        }
    }

    /// <summary>
    /// Récupère un rôle par son ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<RoleDto>> GetRoleById(int id)
    {
        try
        {
            var role = await _roleService.GetRoleByIdAsync(id);
            if (role == null)
            {
                return NotFound(new { message = $"Rôle avec l'ID {id} introuvable" });
            }
            return Ok(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du rôle {RoleId}", id);
            return StatusCode(500, new { message = "Erreur lors de la récupération du rôle" });
        }
    }

    /// <summary>
    /// Crée un nouveau rôle
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<RoleDto>> CreateRole([FromBody] CreateRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.NomRole))
        {
            return BadRequest(new { message = "Le nom du rôle est requis" });
        }

        try
        {
            var role = await _roleService.CreateRoleAsync(request);
            return CreatedAtAction(nameof(GetRoleById), new { id = role.IdRole }, role);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du rôle");
            return StatusCode(500, new { message = "Erreur lors de la création du rôle" });
        }
    }

    /// <summary>
    /// Met à jour un rôle existant
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<RoleDto>> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.NomRole))
        {
            return BadRequest(new { message = "Le nom du rôle est requis" });
        }

        try
        {
            var role = await _roleService.UpdateRoleAsync(id, request);
            if (role == null)
            {
                return NotFound(new { message = $"Rôle avec l'ID {id} introuvable" });
            }
            return Ok(role);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du rôle {RoleId}", id);
            return StatusCode(500, new { message = "Erreur lors de la mise à jour du rôle" });
        }
    }

    /// <summary>
    /// Supprime un rôle
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteRole(int id)
    {
        try
        {
            var deleted = await _roleService.DeleteRoleAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Rôle avec l'ID {id} introuvable" });
            }
            return Ok(new { message = "Rôle supprimé avec succès" });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du rôle {RoleId}", id);
            return StatusCode(500, new { message = "Erreur lors de la suppression du rôle" });
        }
    }

    /// <summary>
    /// Active ou désactive un rôle
    /// </summary>
    [HttpPatch("{id}/actif")]
    public async Task<ActionResult<RoleDto>> ToggleRoleStatus(int id)
    {
        try
        {
            var toggled = await _roleService.ToggleRoleStatusAsync(id);
            if (!toggled)
            {
                return NotFound(new { message = $"Rôle avec l'ID {id} introuvable" });
            }
            
            var role = await _roleService.GetRoleByIdAsync(id);
            return Ok(role);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du changement de statut du rôle {RoleId}", id);
            return StatusCode(500, new { message = "Erreur lors du changement de statut" });
        }
    }
}

