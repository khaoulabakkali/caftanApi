using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using mkBoutiqueCaftan.Data;
using mkBoutiqueCaftan.Models;

namespace mkBoutiqueCaftan.Services;

public interface IConfigurationService
{
    Task<IEnumerable<ConfigurationDto>> GetAllConfigurationsAsync();
    Task<ConfigurationDto?> GetConfigurationByIdAsync(int id);
    Task<ConfigurationDto?> GetConfigurationByCleAsync(string cle);
    Task<ConfigurationDto> CreateConfigurationAsync(CreateConfigurationRequest request);
    Task<ConfigurationDto?> UpdateConfigurationAsync(int id, UpdateConfigurationRequest request);
    Task<bool> DeleteConfigurationAsync(int id);
    Task<bool> ValidateJsonAsync(string json);
}

public class ConfigurationService : IConfigurationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ConfigurationService> _logger;
    private readonly IUserContextService _userContextService;

    public ConfigurationService(ApplicationDbContext context, ILogger<ConfigurationService> logger, IUserContextService userContextService)
    {
        _context = context;
        _logger = logger;
        _userContextService = userContextService;
    }

    public async Task<IEnumerable<ConfigurationDto>> GetAllConfigurationsAsync()
    {
        var idSociete = _userContextService.GetIdSociete();
        if (!idSociete.HasValue)
        {
            _logger.LogWarning("Tentative de récupération des configurations sans IdSociete dans le token");
            return new List<ConfigurationDto>();
        }

        var configurations = await _context.Configurations
            .Where(c => c.IdSociete == idSociete.Value)
            .OrderBy(c => c.Cle)
            .ToListAsync();
        
        return configurations.Select(MapToDto);
    }

    public async Task<ConfigurationDto?> GetConfigurationByIdAsync(int id)
    {
        var idSociete = _userContextService.GetIdSociete();
        if (!idSociete.HasValue)
        {
            _logger.LogWarning("Tentative de récupération d'une configuration sans IdSociete dans le token");
            return null;
        }

        var configuration = await _context.Configurations
            .FirstOrDefaultAsync(c => c.IdConfiguration == id && c.IdSociete == idSociete.Value);
        
        return configuration == null ? null : MapToDto(configuration);
    }

    public async Task<ConfigurationDto?> GetConfigurationByCleAsync(string cle)
    {
        var idSociete = _userContextService.GetIdSociete();
        if (!idSociete.HasValue)
        {
            _logger.LogWarning("Tentative de récupération d'une configuration sans IdSociete dans le token");
            return null;
        }

        var configuration = await _context.Configurations
            .FirstOrDefaultAsync(c => c.Cle == cle && c.IdSociete == idSociete.Value);
        
        return configuration == null ? null : MapToDto(configuration);
    }

    public async Task<ConfigurationDto> CreateConfigurationAsync(CreateConfigurationRequest request)
    {
        var idSociete = _userContextService.GetIdSociete();
        if (!idSociete.HasValue)
        {
            _logger.LogWarning("Tentative de création de configuration sans IdSociete dans le token");
            throw new UnauthorizedAccessException("IdSociete manquant dans le token");
        }

        // Valider le JSON
        if (!await ValidateJsonAsync(request.Data))
        {
            throw new InvalidOperationException("Le format JSON de la propriété Data est invalide");
        }

        // Vérifier si une configuration avec cette clé existe déjà pour cette société
        var existingConfig = await _context.Configurations
            .FirstOrDefaultAsync(c => c.Cle == request.Cle && c.IdSociete == idSociete.Value);
        
        if (existingConfig != null)
        {
            throw new InvalidOperationException($"Une configuration avec la clé '{request.Cle}' existe déjà pour cette société.");
        }

        var configuration = new Configuration
        {
            Cle = request.Cle,
            Data = request.Data,
            IdSociete = idSociete.Value,
            DateCreation = DateTime.Now
        };

        _context.Configurations.Add(configuration);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Configuration créée: {Cle} (ID: {IdConfiguration})", configuration.Cle, configuration.IdConfiguration);

        return MapToDto(configuration);
    }

    public async Task<ConfigurationDto?> UpdateConfigurationAsync(int id, UpdateConfigurationRequest request)
    {
        var idSociete = _userContextService.GetIdSociete();
        if (!idSociete.HasValue)
        {
            _logger.LogWarning("Tentative de mise à jour de configuration sans IdSociete dans le token");
            return null;
        }

        var configuration = await _context.Configurations
            .FirstOrDefaultAsync(c => c.IdConfiguration == id && c.IdSociete == idSociete.Value);
        
        if (configuration == null)
        {
            return null;
        }

        // Si la clé est modifiée, vérifier qu'elle n'existe pas déjà
        if (!string.IsNullOrWhiteSpace(request.Cle) && request.Cle != configuration.Cle)
        {
            var existingConfig = await _context.Configurations
                .FirstOrDefaultAsync(c => c.Cle == request.Cle && c.IdSociete == idSociete.Value && c.IdConfiguration != id);
            
            if (existingConfig != null)
            {
                throw new InvalidOperationException($"Une configuration avec la clé '{request.Cle}' existe déjà pour cette société.");
            }
        }

        // Valider le JSON si fourni
        if (!string.IsNullOrWhiteSpace(request.Data) && !await ValidateJsonAsync(request.Data))
        {
            throw new InvalidOperationException("Le format JSON de la propriété Data est invalide");
        }

        // Mettre à jour les propriétés
        if (!string.IsNullOrWhiteSpace(request.Cle))
            configuration.Cle = request.Cle;

        if (!string.IsNullOrWhiteSpace(request.Data))
            configuration.Data = request.Data;

        configuration.DateModification = DateTime.Now;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Configuration mise à jour: {Cle} (ID: {IdConfiguration})", configuration.Cle, configuration.IdConfiguration);

        return MapToDto(configuration);
    }

    public async Task<bool> DeleteConfigurationAsync(int id)
    {
        var idSociete = _userContextService.GetIdSociete();
        if (!idSociete.HasValue)
        {
            _logger.LogWarning("Tentative de suppression de configuration sans IdSociete dans le token");
            return false;
        }

        var configuration = await _context.Configurations
            .FirstOrDefaultAsync(c => c.IdConfiguration == id && c.IdSociete == idSociete.Value);
        if (configuration == null)
        {
            return false;
        }

        _context.Configurations.Remove(configuration);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Configuration supprimée: {Cle} (ID: {IdConfiguration})", configuration.Cle, configuration.IdConfiguration);
        return true;
    }

    public async Task<bool> ValidateJsonAsync(string json)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return false;
            }

            // Essayer de parser le JSON
            JsonDocument.Parse(json);
            return await Task.FromResult(true);
        }
        catch (JsonException)
        {
            return await Task.FromResult(false);
        }
    }

    private static ConfigurationDto MapToDto(Configuration configuration)
    {
        return new ConfigurationDto
        {
            IdConfiguration = configuration.IdConfiguration,
            IdSociete = configuration.IdSociete,
            Cle = configuration.Cle,
            Data = configuration.Data,
            DateCreation = configuration.DateCreation,
            DateModification = configuration.DateModification
        };
    }
}
