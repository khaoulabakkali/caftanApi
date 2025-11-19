using Microsoft.EntityFrameworkCore;
using mkBoutiqueCaftan.Data;
using mkBoutiqueCaftan.Models;

namespace mkBoutiqueCaftan.Services;

public interface ITailleService
{
    Task<IEnumerable<TailleDto>> GetAllTaillesAsync();
    Task<TailleDto?> GetTailleByIdAsync(int id);
    Task<TailleDto> CreateTailleAsync(CreateTailleRequest request);
    Task<TailleDto?> UpdateTailleAsync(int id, UpdateTailleRequest request);
    Task<bool> DeleteTailleAsync(int id);
}

public class TailleService : ITailleService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TailleService> _logger;

    public TailleService(ApplicationDbContext context, ILogger<TailleService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<TailleDto>> GetAllTaillesAsync()
    {
        var tailles = await _context.Tailles
            .OrderBy(t => t.Libelle)
            .ToListAsync();
        
        return tailles.Select(MapToDto);
    }

    public async Task<TailleDto?> GetTailleByIdAsync(int id)
    {
        var taille = await _context.Tailles
            .FirstOrDefaultAsync(t => t.IdTaille == id);
        return taille == null ? null : MapToDto(taille);
    }

    public async Task<TailleDto> CreateTailleAsync(CreateTailleRequest request)
    {
        // Vérifier si une taille avec le même libellé existe déjà
        var existingTaille = await _context.Tailles
            .FirstOrDefaultAsync(t => t.Libelle.ToLower() == request.Taille.ToLower());
        
        if (existingTaille != null)
        {
            throw new InvalidOperationException($"Une taille avec le libellé '{request.Taille}' existe déjà.");
        }

        var taille = new Taille
        {
            Libelle = request.Taille
        };

        _context.Tailles.Add(taille);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Taille créée: {Taille} (ID: {IdTaille})", taille.Libelle, taille.IdTaille);
        return MapToDto(taille);
    }

    public async Task<TailleDto?> UpdateTailleAsync(int id, UpdateTailleRequest request)
    {
        var taille = await _context.Tailles
            .FirstOrDefaultAsync(t => t.IdTaille == id);
        if (taille == null)
        {
            return null;
        }

        // Vérifier si une autre taille avec le même libellé existe déjà
        var existingTaille = await _context.Tailles
            .FirstOrDefaultAsync(t => t.Libelle.ToLower() == request.Taille.ToLower() && t.IdTaille != id);
        
        if (existingTaille != null)
        {
            throw new InvalidOperationException($"Une taille avec le libellé '{request.Taille}' existe déjà.");
        }

        taille.Libelle = request.Taille;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Taille mise à jour: {Taille} (ID: {IdTaille})", taille.Libelle, taille.IdTaille);
        return MapToDto(taille);
    }

    public async Task<bool> DeleteTailleAsync(int id)
    {
        var taille = await _context.Tailles
            .FirstOrDefaultAsync(t => t.IdTaille == id);
        if (taille == null)
        {
            return false;
        }

        _context.Tailles.Remove(taille);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Taille supprimée: {Taille} (ID: {IdTaille})", taille.Libelle, taille.IdTaille);
        return true;
    }

    private static TailleDto MapToDto(Taille taille)
    {
        return new TailleDto
        {
            IdTaille = taille.IdTaille,
            Taille = taille.Libelle
        };
    }
}

