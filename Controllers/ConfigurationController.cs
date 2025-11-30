using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mkBoutiqueCaftan.Models;
using mkBoutiqueCaftan.Services;

namespace mkBoutiqueCaftan.Controllers;

[ApiController]
[Route("api/configurations")]
[Authorize]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationService _configurationService;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(IConfigurationService configurationService, ILogger<ConfigurationController> logger)
    {
        _configurationService = configurationService;
        _logger = logger;
    }

    /// <summary>
    /// Récupère toutes les configurations
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ConfigurationDto>>> GetAllConfigurations()
    {
        try
        {
            var configurations = await _configurationService.GetAllConfigurationsAsync();
            return Ok(configurations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des configurations");
            return StatusCode(500, new { message = "Erreur lors de la récupération des configurations" });
        }
    }

    /// <summary>
    /// Récupère une configuration par son ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ConfigurationDto>> GetConfigurationById(int id)
    {
        try
        {
            var configuration = await _configurationService.GetConfigurationByIdAsync(id);
            if (configuration == null)
            {
                return NotFound(new { message = $"Configuration avec l'ID {id} introuvable" });
            }
            return Ok(configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la configuration {ConfigurationId}", id);
            return StatusCode(500, new { message = "Erreur lors de la récupération de la configuration" });
        }
    }

    /// <summary>
    /// Récupère une configuration par sa clé
    /// </summary>
    [HttpGet("cle/{cle}")]
    public async Task<ActionResult<ConfigurationDto>> GetConfigurationByCle(string cle)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(cle))
            {
                return BadRequest(new { message = "La clé est requise" });
            }

            var configuration = await _configurationService.GetConfigurationByCleAsync(cle);
            if (configuration == null)
            {
                return NotFound(new { message = $"Configuration avec la clé '{cle}' introuvable" });
            }
            return Ok(configuration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la configuration avec la clé {Cle}", cle);
            return StatusCode(500, new { message = "Erreur lors de la récupération de la configuration" });
        }
    }

    /// <summary>
    /// Crée une nouvelle configuration
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ConfigurationDto>> CreateConfiguration([FromBody] CreateConfigurationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.Cle))
        {
            return BadRequest(new { message = "La clé est requise" });
        }

        if (string.IsNullOrWhiteSpace(request.Data))
        {
            return BadRequest(new { message = "Les données JSON sont requises" });
        }

        if (request.Cle.Length > 100)
        {
            return BadRequest(new { message = "La clé ne peut pas dépasser 100 caractères" });
        }

        // Valider le format JSON
        var isValidJson = await _configurationService.ValidateJsonAsync(request.Data);
        if (!isValidJson)
        {
            return BadRequest(new { message = "Le format JSON fourni est invalide" });
        }

        try
        {
            var configuration = await _configurationService.CreateConfigurationAsync(request);
            return CreatedAtAction(nameof(GetConfigurationById), new { id = configuration.IdConfiguration }, configuration);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de la configuration");
            return StatusCode(500, new { message = "Erreur lors de la création de la configuration" });
        }
    }

    /// <summary>
    /// Met à jour une configuration existante
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ConfigurationDto>> UpdateConfiguration(int id, [FromBody] UpdateConfigurationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.Cle != null && request.Cle.Length > 100)
        {
            return BadRequest(new { message = "La clé ne peut pas dépasser 100 caractères" });
        }

        // Valider le format JSON si fourni
        if (!string.IsNullOrWhiteSpace(request.Data))
        {
            var isValidJson = await _configurationService.ValidateJsonAsync(request.Data);
            if (!isValidJson)
            {
                return BadRequest(new { message = "Le format JSON fourni est invalide" });
            }
        }

        try
        {
            var configuration = await _configurationService.UpdateConfigurationAsync(id, request);
            if (configuration == null)
            {
                return NotFound(new { message = $"Configuration avec l'ID {id} introuvable" });
            }
            return Ok(configuration);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour de la configuration {ConfigurationId}", id);
            return StatusCode(500, new { message = "Erreur lors de la mise à jour de la configuration" });
        }
    }

    /// <summary>
    /// Supprime une configuration
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteConfiguration(int id)
    {
        try
        {
            var deleted = await _configurationService.DeleteConfigurationAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Configuration avec l'ID {id} introuvable" });
            }
            return Ok(new { message = "Configuration supprimée avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de la configuration {ConfigurationId}", id);
            return StatusCode(500, new { message = "Erreur lors de la suppression de la configuration" });
        }
    }

    /// <summary>
    /// Valide un JSON
    /// </summary>
    [HttpPost("validate-json")]
    public async Task<ActionResult> ValidateJson([FromBody] ValidateJsonRequest request)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Json))
        {
            return BadRequest(new { message = "Le JSON à valider est requis" });
        }

        try
        {
            var isValid = await _configurationService.ValidateJsonAsync(request.Json);
            return Ok(new { isValid, message = isValid ? "Le JSON est valide" : "Le JSON est invalide" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la validation du JSON");
            return StatusCode(500, new { message = "Erreur lors de la validation du JSON" });
        }
    }
}

public class ValidateJsonRequest
{
    public string Json { get; set; } = string.Empty;
}
