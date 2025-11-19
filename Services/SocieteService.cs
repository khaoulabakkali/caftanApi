using Microsoft.EntityFrameworkCore;
using mkBoutiqueCaftan.Data;
using mkBoutiqueCaftan.Models;

namespace mkBoutiqueCaftan.Services;

public interface ISocieteService
{
    Task<IEnumerable<SocieteDto>> GetAllSocietesAsync(bool includeInactive = false);
    Task<SocieteDto?> GetSocieteByIdAsync(int id);
    Task<SocieteDto> CreateSocieteAsync(CreateSocieteRequest request);
    Task<SocieteDto?> UpdateSocieteAsync(int id, UpdateSocieteRequest request);
    Task<bool> DeleteSocieteAsync(int id);
    Task<bool> ToggleSocieteStatusAsync(int id);
}

public class SocieteService : ISocieteService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<SocieteService> _logger;

    public SocieteService(ApplicationDbContext context, ILogger<SocieteService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<SocieteDto>> GetAllSocietesAsync(bool includeInactive = false)
    {
        var query = _context.Societes.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(s => s.Actif);
        }

        var societes = await query
            .OrderBy(s => s.NomSociete)
            .ToListAsync();
        
        return societes.Select(MapToDto);
    }

    public async Task<SocieteDto?> GetSocieteByIdAsync(int id)
    {
        var societe = await _context.Societes.FindAsync(id);
        return societe == null ? null : MapToDto(societe);
    }

    public async Task<SocieteDto> CreateSocieteAsync(CreateSocieteRequest request)
    {
        // Vérifier si une société avec le même nom existe déjà
        var existingSociete = await _context.Societes
            .FirstOrDefaultAsync(s => s.NomSociete.ToLower() == request.NomSociete.ToLower());
        
        if (existingSociete != null)
        {
            throw new InvalidOperationException($"Une société avec le nom '{request.NomSociete}' existe déjà.");
        }

        // Vérifier si l'email existe déjà (si fourni)
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailExists = await _context.Societes
                .AnyAsync(s => s.Email != null && s.Email.ToLower() == request.Email.ToLower());
            
            if (emailExists)
            {
                throw new InvalidOperationException($"Une société avec l'email '{request.Email}' existe déjà.");
            }
        }

        var societe = new Societe
        {
            NomSociete = request.NomSociete,
            Description = request.Description,
            Adresse = request.Adresse,
            Telephone = request.Telephone,
            Email = request.Email,
            SiteWeb = request.SiteWeb,
            Logo = request.Logo,
            Actif = request.Actif,
            DateCreation = DateTime.Now
        };

        _context.Societes.Add(societe);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Société créée: {NomSociete} (ID: {IdSociete})", societe.NomSociete, societe.IdSociete);
        return MapToDto(societe);
    }

    public async Task<SocieteDto?> UpdateSocieteAsync(int id, UpdateSocieteRequest request)
    {
        var societe = await _context.Societes.FindAsync(id);
        if (societe == null)
        {
            return null;
        }

        // Vérifier si une autre société avec le même nom existe déjà (si fourni)
        if (!string.IsNullOrWhiteSpace(request.NomSociete) && request.NomSociete.ToLower() != societe.NomSociete.ToLower())
        {
            var existingSociete = await _context.Societes
                .FirstOrDefaultAsync(s => s.NomSociete.ToLower() == request.NomSociete.ToLower() && s.IdSociete != id);
            
            if (existingSociete != null)
            {
                throw new InvalidOperationException($"Une société avec le nom '{request.NomSociete}' existe déjà.");
            }
        }

        // Vérifier si l'email existe déjà pour une autre société (si fourni)
        if (!string.IsNullOrWhiteSpace(request.Email) && request.Email.ToLower() != societe.Email?.ToLower())
        {
            var emailExists = await _context.Societes
                .AnyAsync(s => s.Email != null && s.Email.ToLower() == request.Email.ToLower() && s.IdSociete != id);
            
            if (emailExists)
            {
                throw new InvalidOperationException($"Une société avec l'email '{request.Email}' existe déjà.");
            }
        }

        // Mettre à jour les propriétés
        if (!string.IsNullOrWhiteSpace(request.NomSociete))
            societe.NomSociete = request.NomSociete;

        if (request.Description != null)
            societe.Description = request.Description;

        if (request.Adresse != null)
            societe.Adresse = request.Adresse;

        if (request.Telephone != null)
            societe.Telephone = request.Telephone;

        if (request.Email != null)
            societe.Email = request.Email;

        if (request.SiteWeb != null)
            societe.SiteWeb = request.SiteWeb;

        if (request.Logo != null)
            societe.Logo = request.Logo;

        if (request.Actif.HasValue)
            societe.Actif = request.Actif.Value;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Société mise à jour: {NomSociete} (ID: {IdSociete})", societe.NomSociete, societe.IdSociete);
        return MapToDto(societe);
    }

    public async Task<bool> DeleteSocieteAsync(int id)
    {
        var societe = await _context.Societes
            .FirstOrDefaultAsync(s => s.IdSociete == id);
        
        if (societe == null)
        {
            return false;
        }

        _context.Societes.Remove(societe);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Société supprimée: {NomSociete} (ID: {IdSociete})", societe.NomSociete, societe.IdSociete);
        return true;
    }

    public async Task<bool> ToggleSocieteStatusAsync(int id)
    {
        var societe = await _context.Societes.FindAsync(id);
        if (societe == null)
        {
            return false;
        }

        societe.Actif = !societe.Actif;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Statut de la société {IdSociete} changé à: {Actif}", id, societe.Actif);
        return true;
    }

    private static SocieteDto MapToDto(Societe societe)
    {
        return new SocieteDto
        {
            IdSociete = societe.IdSociete,
            NomSociete = societe.NomSociete,
            Description = societe.Description,
            Adresse = societe.Adresse,
            Telephone = societe.Telephone,
            Email = societe.Email,
            SiteWeb = societe.SiteWeb,
            Logo = societe.Logo,
            Actif = societe.Actif,
            DateCreation = societe.DateCreation
        };
    }
}

