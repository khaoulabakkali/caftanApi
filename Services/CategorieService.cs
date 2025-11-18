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

    public CategorieService(ApplicationDbContext context, ILogger<CategorieService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<CategorieDto>> GetAllCategoriesAsync()
    {
        var categories = await _context.Categories
            .OrderBy(c => c.OrdreAffichage ?? int.MaxValue)
            .ThenBy(c => c.NomCategorie)
            .ToListAsync();
        
        return categories.Select(MapToDto);
    }

    public async Task<CategorieDto?> GetCategorieByIdAsync(int id)
    {
        var categorie = await _context.Categories.FindAsync(id);
        return categorie == null ? null : MapToDto(categorie);
    }

    public async Task<CategorieDto> CreateCategorieAsync(CreateCategorieRequest request)
    {
        // Vérifier si une catégorie avec le même nom existe déjà
        var existingCategorie = await _context.Categories
            .FirstOrDefaultAsync(c => c.NomCategorie.ToLower() == request.NomCategorie.ToLower());
        
        if (existingCategorie != null)
        {
            throw new InvalidOperationException($"Une catégorie avec le nom '{request.NomCategorie}' existe déjà.");
        }

        var categorie = new Categorie
        {
            NomCategorie = request.NomCategorie,
            Description = request.Description,
            OrdreAffichage = request.OrdreAffichage
        };

        _context.Categories.Add(categorie);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Catégorie créée: {NomCategorie} (ID: {IdCategorie})", categorie.NomCategorie, categorie.IdCategorie);
        return MapToDto(categorie);
    }

    public async Task<CategorieDto?> UpdateCategorieAsync(int id, UpdateCategorieRequest request)
    {
        var categorie = await _context.Categories.FindAsync(id);
        if (categorie == null)
        {
            return null;
        }

        // Vérifier si une autre catégorie avec le même nom existe déjà
        var existingCategorie = await _context.Categories
            .FirstOrDefaultAsync(c => c.NomCategorie.ToLower() == request.NomCategorie.ToLower() && c.IdCategorie != id);
        
        if (existingCategorie != null)
        {
            throw new InvalidOperationException($"Une catégorie avec le nom '{request.NomCategorie}' existe déjà.");
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
        var categorie = await _context.Categories.FindAsync(id);
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
            OrdreAffichage = categorie.OrdreAffichage
        };
    }
}

