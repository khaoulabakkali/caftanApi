using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mkBoutiqueCaftan.Models;
using mkBoutiqueCaftan.Services;

namespace mkBoutiqueCaftan.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class CategorieController : ControllerBase
{
    private readonly ICategorieService _categorieService;
    private readonly ILogger<CategorieController> _logger;

    public CategorieController(ICategorieService categorieService, ILogger<CategorieController> logger)
    {
        _categorieService = categorieService;
        _logger = logger;
    }

    /// <summary>
    /// Récupère toutes les catégories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<CategorieDto>>> GetAllCategories()
    {
        try
        {
            var categories = await _categorieService.GetAllCategoriesAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des catégories");
            return StatusCode(500, new { message = "Erreur lors de la récupération des catégories" });
        }
    }

    /// <summary>
    /// Récupère une catégorie par son ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<CategorieDto>> GetCategorieById(int id)
    {
        try
        {
            var categorie = await _categorieService.GetCategorieByIdAsync(id);
            if (categorie == null)
            {
                return NotFound(new { message = $"Catégorie avec l'ID {id} introuvable" });
            }
            return Ok(categorie);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la catégorie {CategorieId}", id);
            return StatusCode(500, new { message = "Erreur lors de la récupération de la catégorie" });
        }
    }

    /// <summary>
    /// Crée une nouvelle catégorie
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<CategorieDto>> CreateCategorie([FromBody] CreateCategorieRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.NomCategorie))
        {
            return BadRequest(new { message = "Le nom de la catégorie est requis" });
        }

        if (request.NomCategorie.Length > 50)
        {
            return BadRequest(new { message = "Le nom de la catégorie ne peut pas dépasser 50 caractères" });
        }

        try
        {
            var categorie = await _categorieService.CreateCategorieAsync(request);
            return CreatedAtAction(nameof(GetCategorieById), new { id = categorie.IdCategorie }, categorie);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de la catégorie");
            return StatusCode(500, new { message = "Erreur lors de la création de la catégorie" });
        }
    }

    /// <summary>
    /// Met à jour une catégorie existante
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<CategorieDto>> UpdateCategorie(int id, [FromBody] UpdateCategorieRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.NomCategorie))
        {
            return BadRequest(new { message = "Le nom de la catégorie est requis" });
        }

        if (request.NomCategorie.Length > 50)
        {
            return BadRequest(new { message = "Le nom de la catégorie ne peut pas dépasser 50 caractères" });
        }

        try
        {
            var categorie = await _categorieService.UpdateCategorieAsync(id, request);
            if (categorie == null)
            {
                return NotFound(new { message = $"Catégorie avec l'ID {id} introuvable" });
            }
            return Ok(categorie);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour de la catégorie {CategorieId}", id);
            return StatusCode(500, new { message = "Erreur lors de la mise à jour de la catégorie" });
        }
    }

    /// <summary>
    /// Supprime une catégorie
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteCategorie(int id)
    {
        try
        {
            var deleted = await _categorieService.DeleteCategorieAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Catégorie avec l'ID {id} introuvable" });
            }
            return Ok(new { message = "Catégorie supprimée avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de la catégorie {CategorieId}", id);
            return StatusCode(500, new { message = "Erreur lors de la suppression de la catégorie" });
        }
    }
}

