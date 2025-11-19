using Microsoft.EntityFrameworkCore;
using mkBoutiqueCaftan.Data;
using mkBoutiqueCaftan.Models;

namespace mkBoutiqueCaftan.Services;

public interface ICategorieService
{
    Task<IEnumerable<CategorieDto>> GetAllCategoriesAsync();
    Task<CategorieDto?> GetCategorieByIdAsync(int id);
    Task<CategorieDto> CreateCategorieAsync(CreateCategorieRequest request);
    Task<CategorieDto?> UpdateCategorieAsync(int id, UpdateCategorieRequest request);
    Task<bool> DeleteCategorieAsync(int id);
}

public class CategorieService : ICategorieService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CategorieService> _logger;
    private readonly IUserContextService _userContextService;

    public CategorieService(ApplicationDbContext context, ILogger<CategorieService> logger, IUserContextService userContextService)
    {
        _context = context;
        _logger = logger;
        _userContextService = userContextService;
    }

    public async Task<IEnumerable<CategorieDto>> GetAllCategoriesAsync()
    {
        var currentIdSociete = _userContextService.GetIdSociete();
        if (!currentIdSociete.HasValue)
        {
            throw new UnauthorizedAccessException("IdSociete non trouvé dans le token. Veuillez vous reconnecter.");
        }

        var categories = await _context.Categories
            .Where(c => c.IdSociete == currentIdSociete.Value)
            .OrderBy(c => c.OrdreAffichage ?? int.MaxValue)
            .ThenBy(c => c.NomCategorie)
            .ToListAsync();
        
        return categories.Select(MapToDto);
    }

    public async Task<CategorieDto?> GetCategorieByIdAsync(int id)
    {
        var currentIdSociete = _userContextService.GetIdSociete();
        if (!currentIdSociete.HasValue)
        {
            throw new UnauthorizedAccessException("IdSociete non trouvé dans le token. Veuillez vous reconnecter.");
        }

        var categorie = await _context.Categories
            .FirstOrDefaultAsync(c => c.IdCategorie == id && c.IdSociete == currentIdSociete.Value);
        return categorie == null ? null : MapToDto(categorie);
    }

    public async Task<CategorieDto> CreateCategorieAsync(CreateCategorieRequest request)
    {
        var currentIdSociete = _userContextService.GetIdSociete();
        if (!currentIdSociete.HasValue)
        {
            throw new UnauthorizedAccessException("IdSociete non trouvé dans le token. Veuillez vous reconnecter.");
        }

        // Vérifier si une catégorie avec le même nom existe déjà pour cette société
        var existingCategorie = await _context.Categories
            .FirstOrDefaultAsync(c => c.NomCategorie.ToLower() == request.NomCategorie.ToLower() && c.IdSociete == currentIdSociete.Value);
        
        if (existingCategorie != null)
        {
            throw new InvalidOperationException($"Une catégorie avec le nom '{request.NomCategorie}' existe déjà pour cette société.");
        }

        var categorie = new Categorie
        {
            NomCategorie = request.NomCategorie,
            Description = request.Description,
            OrdreAffichage = request.OrdreAffichage,
            IdSociete = currentIdSociete.Value
        };

        _context.Categories.Add(categorie);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Catégorie créée: {NomCategorie} (ID: {IdCategorie})", categorie.NomCategorie, categorie.IdCategorie);
        return MapToDto(categorie);
    }

    public async Task<CategorieDto?> UpdateCategorieAsync(int id, UpdateCategorieRequest request)
    {
        var currentIdSociete = _userContextService.GetIdSociete();
        if (!currentIdSociete.HasValue)
        {
            throw new UnauthorizedAccessException("IdSociete non trouvé dans le token. Veuillez vous reconnecter.");
        }

        var categorie = await _context.Categories
            .FirstOrDefaultAsync(c => c.IdCategorie == id && c.IdSociete == currentIdSociete.Value);
        if (categorie == null)
        {
            return null;
        }

        // Vérifier si une autre catégorie avec le même nom existe déjà pour cette société
        var existingCategorie = await _context.Categories
            .FirstOrDefaultAsync(c => c.NomCategorie.ToLower() == request.NomCategorie.ToLower() && c.IdCategorie != id && c.IdSociete == currentIdSociete.Value);
        
        if (existingCategorie != null)
        {
            throw new InvalidOperationException($"Une catégorie avec le nom '{request.NomCategorie}' existe déjà pour cette société.");
        }

        categorie.NomCategorie = request.NomCategorie;
        categorie.Description = request.Description;
        categorie.OrdreAffichage = request.OrdreAffichage;
        
        await _context.SaveChangesAsync();

        _logger.LogInformation("Catégorie mise à jour: {NomCategorie} (ID: {IdCategorie})", categorie.NomCategorie, categorie.IdCategorie);
        return MapToDto(categorie);
    }

    public async Task<bool> DeleteCategorieAsync(int id)
    {
        var currentIdSociete = _userContextService.GetIdSociete();
        if (!currentIdSociete.HasValue)
        {
            throw new UnauthorizedAccessException("IdSociete non trouvé dans le token. Veuillez vous reconnecter.");
        }

        var categorie = await _context.Categories
            .FirstOrDefaultAsync(c => c.IdCategorie == id && c.IdSociete == currentIdSociete.Value);
        if (categorie == null)
        {
            return false;
        }

        _context.Categories.Remove(categorie);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Catégorie supprimée: {NomCategorie} (ID: {IdCategorie})", categorie.NomCategorie, categorie.IdCategorie);
        return true;
    }

    private static CategorieDto MapToDto(Categorie categorie)
    {
        return new CategorieDto
        {
            IdCategorie = categorie.IdCategorie,
            NomCategorie = categorie.NomCategorie,
            Description = categorie.Description,
            OrdreAffichage = categorie.OrdreAffichage,
            IdSociete = categorie.IdSociete
        };
    }
}

