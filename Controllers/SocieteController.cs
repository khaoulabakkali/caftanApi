using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mkBoutiqueCaftan.Models;
using mkBoutiqueCaftan.Services;

namespace mkBoutiqueCaftan.Controllers;

[ApiController]
[Route("api/societes")]
[Authorize]
public class SocieteController : ControllerBase
{
    private readonly ISocieteService _societeService;
    private readonly ILogger<SocieteController> _logger;

    public SocieteController(ISocieteService societeService, ILogger<SocieteController> logger)
    {
        _societeService = societeService;
        _logger = logger;
    }

    /// <summary>
    /// Récupère toutes les sociétés
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<SocieteDto>>> GetAllSocietes(
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            var societes = await _societeService.GetAllSocietesAsync(includeInactive);
            return Ok(societes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des sociétés");
            return StatusCode(500, new { message = "Erreur lors de la récupération des sociétés" });
        }
    }

    /// <summary>
    /// Récupère une société par son ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<SocieteDto>> GetSocieteById(int id)
    {
        try
        {
            var societe = await _societeService.GetSocieteByIdAsync(id);
            if (societe == null)
            {
                return NotFound(new { message = $"Société avec l'ID {id} introuvable" });
            }
            return Ok(societe);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la société {SocieteId}", id);
            return StatusCode(500, new { message = "Erreur lors de la récupération de la société" });
        }
    }

    /// <summary>
    /// Crée une nouvelle société
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<SocieteDto>> CreateSociete([FromBody] CreateSocieteRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.NomSociete))
        {
            return BadRequest(new { message = "Le nom de la société est requis" });
        }

        if (request.NomSociete.Length > 100)
        {
            return BadRequest(new { message = "Le nom de la société ne peut pas dépasser 100 caractères" });
        }

        if (request.Email != null && request.Email.Length > 100)
        {
            return BadRequest(new { message = "L'email ne peut pas dépasser 100 caractères" });
        }

        if (request.Telephone != null && request.Telephone.Length > 20)
        {
            return BadRequest(new { message = "Le téléphone ne peut pas dépasser 20 caractères" });
        }

        try
        {
            var societe = await _societeService.CreateSocieteAsync(request);
            return CreatedAtAction(nameof(GetSocieteById), new { id = societe.IdSociete }, societe);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de la société");
            return StatusCode(500, new { message = "Erreur lors de la création de la société" });
        }
    }

    /// <summary>
    /// Met à jour une société existante
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<SocieteDto>> UpdateSociete(int id, [FromBody] UpdateSocieteRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.NomSociete != null && request.NomSociete.Length > 100)
        {
            return BadRequest(new { message = "Le nom de la société ne peut pas dépasser 100 caractères" });
        }

        if (request.Email != null && request.Email.Length > 100)
        {
            return BadRequest(new { message = "L'email ne peut pas dépasser 100 caractères" });
        }

        if (request.Telephone != null && request.Telephone.Length > 20)
        {
            return BadRequest(new { message = "Le téléphone ne peut pas dépasser 20 caractères" });
        }

        try
        {
            var societe = await _societeService.UpdateSocieteAsync(id, request);
            if (societe == null)
            {
                return NotFound(new { message = $"Société avec l'ID {id} introuvable" });
            }
            return Ok(societe);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour de la société {SocieteId}", id);
            return StatusCode(500, new { message = "Erreur lors de la mise à jour de la société" });
        }
    }

    /// <summary>
    /// Supprime une société
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteSociete(int id)
    {
        try
        {
            var deleted = await _societeService.DeleteSocieteAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Société avec l'ID {id} introuvable" });
            }
            return Ok(new { message = "Société supprimée avec succès" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de la société {SocieteId}", id);
            return StatusCode(500, new { message = "Erreur lors de la suppression de la société" });
        }
    }

    /// <summary>
    /// Active ou désactive une société
    /// </summary>
    [HttpPatch("{id}/actif")]
    public async Task<ActionResult<SocieteDto>> ToggleSocieteStatus(int id)
    {
        try
        {
            var toggled = await _societeService.ToggleSocieteStatusAsync(id);
            if (!toggled)
            {
                return NotFound(new { message = $"Société avec l'ID {id} introuvable" });
            }
            
            var societe = await _societeService.GetSocieteByIdAsync(id);
            return Ok(societe);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du changement de statut de la société {SocieteId}", id);
            return StatusCode(500, new { message = "Erreur lors du changement de statut" });
        }
    }
}

