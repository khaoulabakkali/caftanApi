using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mkBoutiqueCaftan.Models;
using mkBoutiqueCaftan.Services;

namespace mkBoutiqueCaftan.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Récupère tous les utilisateurs actifs
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<UserDto>>> GetAllUsers()
    {
        try
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des utilisateurs");
            return StatusCode(500, new { message = "Erreur lors de la récupération des utilisateurs" });
        }
    }

    /// <summary>
    /// Récupère un utilisateur par son ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDto>> GetUserById(int id)
    {
        try
        {
            var user = await _userService.GetUserByIdAsync(id);
            if (user == null)
            {
                return NotFound(new { message = $"Utilisateur avec l'ID {id} introuvable" });
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'utilisateur {UserId}", id);
            return StatusCode(500, new { message = "Erreur lors de la récupération de l'utilisateur" });
        }
    }

    /// <summary>
    /// Crée un nouvel utilisateur
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.NomComplet) ||
                string.IsNullOrWhiteSpace(request.Login) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Le nom complet, le login et le mot de passe sont requis" });
            }

            var user = await _userService.CreateUserAsync(request);
            if (user == null)
            {
                return BadRequest(new { message = "Impossible de créer l'utilisateur. Le login existe peut-être déjà ou le rôle est invalide." });
            }

            return CreatedAtAction(nameof(GetUserById), new { id = user.IdUtilisateur }, user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de l'utilisateur");
            return StatusCode(500, new { message = "Erreur lors de la création de l'utilisateur" });
        }
    }

    /// <summary>
    /// Met à jour un utilisateur
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userService.UpdateUserAsync(id, request);
            if (user == null)
            {
                return NotFound(new { message = $"Utilisateur avec l'ID {id} introuvable ou données invalides" });
            }

            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour de l'utilisateur {UserId}", id);
            return StatusCode(500, new { message = "Erreur lors de la mise à jour de l'utilisateur" });
        }
    }

    /// <summary>
    /// Supprime (désactive) un utilisateur
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteUser(int id)
    {
        try
        {
            var success = await _userService.DeleteUserAsync(id);
            if (!success)
            {
                return NotFound(new { message = $"Utilisateur avec l'ID {id} introuvable" });
            }

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'utilisateur {UserId}", id);
            return StatusCode(500, new { message = "Erreur lors de la suppression de l'utilisateur" });
        }
    }

    /// <summary>
    /// Change le mot de passe d'un utilisateur
    /// </summary>
    [HttpPost("{id}/change-password")]
    public async Task<ActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "Le nouveau mot de passe est requis" });
            }

            var success = await _userService.ChangePasswordAsync(id, request.NewPassword);
            if (!success)
            {
                return NotFound(new { message = $"Utilisateur avec l'ID {id} introuvable" });
            }

            return Ok(new { message = "Mot de passe changé avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du changement de mot de passe pour l'utilisateur {UserId}", id);
            return StatusCode(500, new { message = "Erreur lors du changement de mot de passe" });
        }
    }

    /// <summary>
    /// Active ou désactive un utilisateur
    /// </summary>
    [HttpPatch("{id}/actif")]
    public async Task<ActionResult> ToggleUserStatus(int id)
    {
        try
        {
            var success = await _userService.ToggleUserStatusAsync(id);
            if (!success)
            {
                return NotFound(new { message = $"Utilisateur avec l'ID {id} introuvable" });
            }

            return Ok(new { message = "Statut de l'utilisateur modifié avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du changement de statut pour l'utilisateur {UserId}", id);
            return StatusCode(500, new { message = "Erreur lors du changement de statut" });
        }
    }
}

