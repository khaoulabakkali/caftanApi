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

    public ArticleService(ApplicationDbContext context, ILogger<ArticleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ArticleDto>> GetAllArticlesAsync(int? idSociete = null, bool includeInactive = false)
    {
        var query = _context.Articles
            .Include(a => a.Taille)
            .Include(a => a.Categorie)
            .AsQueryable();

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
        var article = await _context.Articles
            .Include(a => a.Taille)
            .Include(a => a.Categorie)
            .FirstOrDefaultAsync(a => a.IdArticle == id);
        
        return article == null ? null : MapToDto(article);
    }

    public async Task<ArticleDto> CreateArticleAsync(CreateArticleRequest request)
    {
        // Vérifier si la catégorie existe
        var categorie = await _context.Categories
            .FirstOrDefaultAsync(c => c.IdCategorie == request.IdCategorie);
        
        if (categorie == null)
        {
            throw new InvalidOperationException($"La catégorie avec l'ID {request.IdCategorie} n'existe pas.");
        }

        // Vérifier si la taille existe (si fournie)
        if (request.IdTaille.HasValue)
        {
            var taille = await _context.Tailles
                .FirstOrDefaultAsync(t => t.IdTaille == request.IdTaille.Value);
            
            if (taille == null)
            {
                throw new InvalidOperationException($"La taille avec l'ID {request.IdTaille} n'existe pas.");
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
        var article = await _context.Articles
            .Include(a => a.Taille)
            .Include(a => a.Categorie)
            .FirstOrDefaultAsync(a => a.IdArticle == id);
        
        if (article == null)
        {
            return null;
        }

        // Vérifier si la catégorie existe (si fournie)
        if (request.IdCategorie.HasValue)
        {
            var categorie = await _context.Categories
                .FirstOrDefaultAsync(c => c.IdCategorie == request.IdCategorie.Value);
            
            if (categorie == null)
            {
                throw new InvalidOperationException($"La catégorie avec l'ID {request.IdCategorie} n'existe pas.");
            }
        }

        // Vérifier si la taille existe (si fournie)
        if (request.IdTaille.HasValue)
        {
            var taille = await _context.Tailles
                .FirstOrDefaultAsync(t => t.IdTaille == request.IdTaille.Value);
            
            if (taille == null)
            {
                throw new InvalidOperationException($"La taille avec l'ID {request.IdTaille} n'existe pas.");
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
        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.IdArticle == id);
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
        var article = await _context.Articles
            .FirstOrDefaultAsync(a => a.IdArticle == id);
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

