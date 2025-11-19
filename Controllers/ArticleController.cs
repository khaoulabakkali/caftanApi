using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mkBoutiqueCaftan.Models;
using mkBoutiqueCaftan.Services;

namespace mkBoutiqueCaftan.Controllers;

[ApiController]
[Route("api/articles")]
[Authorize]
public class ArticleController : ControllerBase
{
    private readonly IArticleService _articleService;
    private readonly ILogger<ArticleController> _logger;

    public ArticleController(IArticleService articleService, ILogger<ArticleController> logger)
    {
        _articleService = articleService;
        _logger = logger;
    }

    /// <summary>
    /// Récupère tous les articles
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ArticleDto>>> GetAllArticles(
        [FromQuery] int? idSociete = null,
        [FromQuery] bool includeInactive = false)
    {
        try
        {
            var articles = await _articleService.GetAllArticlesAsync(idSociete, includeInactive);
            return Ok(articles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des articles");
            return StatusCode(500, new { message = "Erreur lors de la récupération des articles" });
        }
    }

    /// <summary>
    /// Récupère un article par son ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ArticleDto>> GetArticleById(int id)
    {
        try
        {
            var article = await _articleService.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound(new { message = $"Article avec l'ID {id} introuvable" });
            }
            return Ok(article);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de l'article {ArticleId}", id);
            return StatusCode(500, new { message = "Erreur lors de la récupération de l'article" });
        }
    }

    /// <summary>
    /// Crée un nouvel article
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ArticleDto>> CreateArticle([FromBody] CreateArticleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.NomArticle))
        {
            return BadRequest(new { message = "Le nom de l'article est requis" });
        }

        if (string.IsNullOrWhiteSpace(request.Description))
        {
            return BadRequest(new { message = "La description est requise" });
        }

        if (request.NomArticle.Length > 150)
        {
            return BadRequest(new { message = "Le nom de l'article ne peut pas dépasser 150 caractères" });
        }

        if (request.PrixLocationBase < 0 || request.PrixAvanceBase < 0)
        {
            return BadRequest(new { message = "Les prix ne peuvent pas être négatifs" });
        }

        try
        {
            var article = await _articleService.CreateArticleAsync(request);
            return CreatedAtAction(nameof(GetArticleById), new { id = article.IdArticle }, article);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de l'article");
            return StatusCode(500, new { message = "Erreur lors de la création de l'article" });
        }
    }

    /// <summary>
    /// Met à jour un article existant
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ArticleDto>> UpdateArticle(int id, [FromBody] UpdateArticleRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.NomArticle != null && request.NomArticle.Length > 150)
        {
            return BadRequest(new { message = "Le nom de l'article ne peut pas dépasser 150 caractères" });
        }

        if (request.PrixLocationBase.HasValue && request.PrixLocationBase.Value < 0)
        {
            return BadRequest(new { message = "Le prix de location ne peut pas être négatif" });
        }

        if (request.PrixAvanceBase.HasValue && request.PrixAvanceBase.Value < 0)
        {
            return BadRequest(new { message = "Le prix d'avance ne peut pas être négatif" });
        }

        try
        {
            var article = await _articleService.UpdateArticleAsync(id, request);
            if (article == null)
            {
                return NotFound(new { message = $"Article avec l'ID {id} introuvable" });
            }
            return Ok(article);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour de l'article {ArticleId}", id);
            return StatusCode(500, new { message = "Erreur lors de la mise à jour de l'article" });
        }
    }

    /// <summary>
    /// Supprime un article
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteArticle(int id)
    {
        try
        {
            var deleted = await _articleService.DeleteArticleAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Article avec l'ID {id} introuvable" });
            }
            return Ok(new { message = "Article supprimé avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de l'article {ArticleId}", id);
            return StatusCode(500, new { message = "Erreur lors de la suppression de l'article" });
        }
    }

    /// <summary>
    /// Active ou désactive un article
    /// </summary>
    [HttpPatch("{id}/actif")]
    public async Task<ActionResult<ArticleDto>> ToggleArticleStatus(int id)
    {
        try
        {
            var toggled = await _articleService.ToggleArticleStatusAsync(id);
            if (!toggled)
            {
                return NotFound(new { message = $"Article avec l'ID {id} introuvable" });
            }
            
            var article = await _articleService.GetArticleByIdAsync(id);
            return Ok(article);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du changement de statut de l'article {ArticleId}", id);
            return StatusCode(500, new { message = "Erreur lors du changement de statut" });
        }
    }
}

