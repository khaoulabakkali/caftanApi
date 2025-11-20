using Microsoft.EntityFrameworkCore;
using mkBoutiqueCaftan.Data;
using mkBoutiqueCaftan.Models;

namespace mkBoutiqueCaftan.Services;

public interface IArticleService
{
    Task<IEnumerable<ArticleDto>> GetAllArticlesAsync(int? idSociete = null, bool includeInactive = false);
    Task<ArticleDto?> GetArticleByIdAsync(int id);
    Task<ArticleDto> CreateArticleAsync(CreateArticleRequest request);
    Task<ArticleDto?> UpdateArticleAsync(int id, UpdateArticleRequest request);
    Task<bool> DeleteArticleAsync(int id);
    Task<bool> ToggleArticleStatusAsync(int id);
}

public class ArticleService : IArticleService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ArticleService> _logger;
    private readonly IUserContextService _userContextService;

    public ArticleService(ApplicationDbContext context, ILogger<ArticleService> logger, IUserContextService userContextService)
    {
        _context = context;
        _logger = logger;
        _userContextService = userContextService;
    }

    public async Task<IEnumerable<ArticleDto>> GetAllArticlesAsync(int? idSociete = null, bool includeInactive = false)
    {
        var idSocieteFromToken = _userContextService.GetIdSociete();
        if (!idSocieteFromToken.HasValue)
        {
            _logger.LogWarning("Tentative de récupération des articles sans IdSociete dans le token");
            return new List<ArticleDto>();
        }

        var query = _context.Articles
            .Include(a => a.Taille)
            .Include(a => a.Categorie)
            .Where(a => a.IdSociete == idSocieteFromToken.Value);

        if (!includeInactive)
        {
            query = query.Where(a => a.Actif);
        }

        var articles = await query
            .OrderBy(a => a.NomArticle)
            .ToListAsync();
        
        return articles.Select(MapToDto);
    }

    public async Task<ArticleDto?> GetArticleByIdAsync(int id)
    {
        var idSociete = _userContextService.GetIdSociete();
        if (!idSociete.HasValue)
        {
            _logger.LogWarning("Tentative de récupération d'un article sans IdSociete dans le token");
            return null;
        }

        var article = await _context.Articles
            .Include(a => a.Taille)
            .Include(a => a.Categorie)
            .FirstOrDefaultAsync(a => a.IdArticle == id && a.IdSociete == idSociete.Value);
        
        return article == null ? null : MapToDto(article);
    }

    public async Task<ArticleDto> CreateArticleAsync(CreateArticleRequest request)
    {
        var idSociete = _userContextService.GetIdSociete();
        if (!idSociete.HasValue)
        {
            _logger.LogWarning("Tentative de création d'article sans IdSociete dans le token");
            throw new UnauthorizedAccessException("IdSociete manquant dans le token");
        }

        // Vérifier si la catégorie existe pour cette société
        var categorie = await _context.Categories
            .FirstOrDefaultAsync(c => c.IdCategorie == request.IdCategorie && c.IdSociete == idSociete.Value);
        
        if (categorie == null)
        {
            throw new InvalidOperationException($"La catégorie avec l'ID {request.IdCategorie} n'existe pas pour cette société.");
        }

        // Vérifier si la taille existe (si fournie) pour cette société
        if (request.IdTaille.HasValue)
        {
            var taille = await _context.Tailles
                .FirstOrDefaultAsync(t => t.IdTaille == request.IdTaille.Value && t.IdSociete == idSociete.Value);
            
            if (taille == null)
            {
                throw new InvalidOperationException($"La taille avec l'ID {request.IdTaille} n'existe pas pour cette société.");
            }
        }

        var article = new Article
        {
            NomArticle = request.NomArticle,
            Description = request.Description,
            PrixLocationBase = request.PrixLocationBase,
            PrixAvanceBase = request.PrixAvanceBase,
            IdTaille = request.IdTaille,
            Couleur = request.Couleur,
            Photo = request.Photo,
            IdCategorie = request.IdCategorie,
            IdSociete = idSociete.Value,
            Actif = request.Actif
        };

        _context.Articles.Add(article);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Article créé: {NomArticle} (ID: {IdArticle})", article.NomArticle, article.IdArticle);

        // Recharger avec les relations
        return await GetArticleByIdAsync(article.IdArticle) ?? MapToDto(article);
    }

    public async Task<ArticleDto?> UpdateArticleAsync(int id, UpdateArticleRequest request)
    {
        var idSociete = _userContextService.GetIdSociete();
        if (!idSociete.HasValue)
        {
            _logger.LogWarning("Tentative de mise à jour d'article sans IdSociete dans le token");
            return null;
        }

        var article = await _context.Articles
            .Include(a => a.Taille)
            .Include(a => a.Categorie)
            .FirstOrDefaultAsync(a => a.IdArticle == id && a.IdSociete == idSociete.Value);
        
        if (article == null)
        {
            return null;
        }

        // Vérifier si la catégorie existe (si fournie) pour cette société
        if (request.IdCategorie.HasValue)
        {
            var categorie = await _context.Categories
                .FirstOrDefaultAsync(c => c.IdCategorie == request.IdCategorie.Value && c.IdSociete == idSociete.Value);
            
            if (categorie == null)
            {
                throw new InvalidOperationException($"La catégorie avec l'ID {request.IdCategorie} n'existe pas pour cette société.");
            }
        }

        // Vérifier si la taille existe (si fournie) pour cette société
        if (request.IdTaille.HasValue)
        {
            var taille = await _context.Tailles
                .FirstOrDefaultAsync(t => t.IdTaille == request.IdTaille.Value && t.IdSociete == idSociete.Value);
            
            if (taille == null)
            {
                throw new InvalidOperationException($"La taille avec l'ID {request.IdTaille} n'existe pas pour cette société.");
            }
        }

        // Mettre à jour les propriétés
        if (!string.IsNullOrWhiteSpace(request.NomArticle))
            article.NomArticle = request.NomArticle;

        if (!string.IsNullOrWhiteSpace(request.Description))
            article.Description = request.Description;

        if (request.PrixLocationBase.HasValue)
            article.PrixLocationBase = request.PrixLocationBase.Value;

        if (request.PrixAvanceBase.HasValue)
            article.PrixAvanceBase = request.PrixAvanceBase.Value;

        if (request.IdTaille.HasValue)
            article.IdTaille = request.IdTaille.Value;
        else if (request.IdTaille == null && request.IdTaille != article.IdTaille)
            article.IdTaille = null;

        if (request.Couleur != null)
            article.Couleur = request.Couleur;

        if (request.Photo != null)
            article.Photo = request.Photo;

        if (request.IdCategorie.HasValue)
            article.IdCategorie = request.IdCategorie.Value;

        if (request.Actif.HasValue)
            article.Actif = request.Actif.Value;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Article mis à jour: {NomArticle} (ID: {IdArticle})", article.NomArticle, article.IdArticle);

        // Recharger avec les relations
        return await GetArticleByIdAsync(article.IdArticle) ?? MapToDto(article);
    }

    public async Task<bool> DeleteArticleAsync(int id)
    {
        var idSociete = _userContextService.GetIdSociete();
        if (!idSociete.HasValue)
        {
            _logger.LogWarning("Tentative de suppression d'article sans IdSociete dans le token");
            return false;
        }

        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.IdArticle == id && a.IdSociete == idSociete.Value);
        if (article == null)
        {
            return false;
        }

        _context.Articles.Remove(article);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Article supprimé: {NomArticle} (ID: {IdArticle})", article.NomArticle, article.IdArticle);
        return true;
    }

    public async Task<bool> ToggleArticleStatusAsync(int id)
    {
        var idSociete = _userContextService.GetIdSociete();
        if (!idSociete.HasValue)
        {
            _logger.LogWarning("Tentative de changement de statut d'article sans IdSociete dans le token");
            return false;
        }

        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.IdArticle == id && a.IdSociete == idSociete.Value);
        if (article == null)
        {
            return false;
        }

        article.Actif = !article.Actif;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Statut de l'article {IdArticle} changé à: {Actif}", id, article.Actif);
        return true;
    }

    private static ArticleDto MapToDto(Article article)
    {
        return new ArticleDto
        {
            IdArticle = article.IdArticle,
            NomArticle = article.NomArticle,
            Description = article.Description,
            PrixLocationBase = article.PrixLocationBase,
            PrixAvanceBase = article.PrixAvanceBase,
            IdTaille = article.IdTaille,
            Couleur = article.Couleur,
            Photo = article.Photo,
            IdCategorie = article.IdCategorie,
            IdSociete = article.IdSociete,
            Actif = article.Actif,
            Taille = article.Taille != null ? new TailleDto
            {
                IdTaille = article.Taille.IdTaille,
                Taille = article.Taille.Libelle
            } : null,
            Categorie = article.Categorie != null ? new CategorieDto
            {
                IdCategorie = article.Categorie.IdCategorie,
                NomCategorie = article.Categorie.NomCategorie,
                Description = article.Categorie.Description,
                OrdreAffichage = article.Categorie.OrdreAffichage
            } : null
        };
    }
}

