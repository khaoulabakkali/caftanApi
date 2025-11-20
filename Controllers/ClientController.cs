using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mkBoutiqueCaftan.Models;
using mkBoutiqueCaftan.Services;

namespace mkBoutiqueCaftan.Controllers;

[ApiController]
[Route("api/clients")]
[Authorize]
public class ClientController : ControllerBase
{
    private readonly IClientService _clientService;
    private readonly ILogger<ClientController> _logger;

    public ClientController(IClientService clientService, ILogger<ClientController> logger)
    {
        _clientService = clientService;
        _logger = logger;
    }

    /// <summary>
    /// Récupère tous les clients
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientDto>>> GetAllClients([FromQuery] bool includeInactive = false)
    {
        try
        {
            var clients = await _clientService.GetAllClientsAsync(includeInactive);
            return Ok(clients);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des clients");
            return StatusCode(500, new { message = "Erreur lors de la récupération des clients" });
        }
    }

    /// <summary>
    /// Récupère un client par son ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDto>> GetClientById(int id)
    {
        try
        {
            var client = await _clientService.GetClientByIdAsync(id);
            if (client == null)
            {
                return NotFound(new { message = $"Client avec l'ID {id} introuvable" });
            }
            return Ok(client);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération du client {ClientId}", id);
            return StatusCode(500, new { message = "Erreur lors de la récupération du client" });
        }
    }

    /// <summary>
    /// Crée un nouveau client
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ClientDto>> CreateClient([FromBody] CreateClientRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (string.IsNullOrWhiteSpace(request.NomClient))
        {
            return BadRequest(new { message = "Le nom du client est requis" });
        }

        if (string.IsNullOrWhiteSpace(request.Telephone))
        {
            return BadRequest(new { message = "Le téléphone est requis" });
        }

        if (request.NomClient.Length > 100)
        {
            return BadRequest(new { message = "Le nom du client ne peut pas dépasser 100 caractères" });
        }

        if (request.PrenomClient.Length > 100)
        {
            return BadRequest(new { message = "Le prénom du client ne peut pas dépasser 100 caractères" });
        }

        if (request.Telephone.Length > 20)
        {
            return BadRequest(new { message = "Le téléphone ne peut pas dépasser 20 caractères" });
        }

        if (request.Email != null && request.Email.Length > 100)
        {
            return BadRequest(new { message = "L'email ne peut pas dépasser 100 caractères" });
        }

        try
        {
            var client = await _clientService.CreateClientAsync(request);
            return CreatedAtAction(nameof(GetClientById), new { id = client.IdClient }, client);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création du client");
            return StatusCode(500, new { message = "Erreur lors de la création du client" });
        }
    }

    /// <summary>
    /// Met à jour un client existant
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ClientDto>> UpdateClient(int id, [FromBody] UpdateClientRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.NomClient != null && request.NomClient.Length > 100)
        {
            return BadRequest(new { message = "Le nom du client ne peut pas dépasser 100 caractères" });
        }

        if (request.PrenomClient != null && request.PrenomClient.Length > 100)
        {
            return BadRequest(new { message = "Le prénom du client ne peut pas dépasser 100 caractères" });
        }

        if (request.Telephone != null && request.Telephone.Length > 20)
        {
            return BadRequest(new { message = "Le téléphone ne peut pas dépasser 20 caractères" });
        }

        if (request.Email != null && request.Email.Length > 100)
        {
            return BadRequest(new { message = "L'email ne peut pas dépasser 100 caractères" });
        }

        if (request.TotalCommandes.HasValue && request.TotalCommandes.Value < 0)
        {
            return BadRequest(new { message = "Le total des commandes ne peut pas être négatif" });
        }

        try
        {
            var client = await _clientService.UpdateClientAsync(id, request);
            if (client == null)
            {
                return NotFound(new { message = $"Client avec l'ID {id} introuvable" });
            }
            return Ok(client);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du client {ClientId}", id);
            return StatusCode(500, new { message = "Erreur lors de la mise à jour du client" });
        }
    }

    /// <summary>
    /// Supprime un client
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteClient(int id)
    {
        try
        {
            var deleted = await _clientService.DeleteClientAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Client avec l'ID {id} introuvable" });
            }
            return Ok(new { message = "Client supprimé avec succès" });
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression du client {ClientId}", id);
            return StatusCode(500, new { message = "Erreur lors de la suppression du client" });
        }
    }

    /// <summary>
    /// Active ou désactive un client
    /// </summary>
    [HttpPatch("{id}/actif")]
    public async Task<ActionResult<ClientDto>> ToggleClientStatus(int id)
    {
        try
        {
            var toggled = await _clientService.ToggleClientStatusAsync(id);
            if (!toggled)
            {
                return NotFound(new { message = $"Client avec l'ID {id} introuvable" });
            }
            
            var client = await _clientService.GetClientByIdAsync(id);
            return Ok(client);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors du changement de statut du client {ClientId}", id);
            return StatusCode(500, new { message = "Erreur lors du changement de statut" });
        }
    }

    /// <summary>
    /// Incrémente le total des commandes d'un client
    /// </summary>
    [HttpPost("{id}/increment-commandes")]
    public async Task<ActionResult<ClientDto>> IncrementTotalCommandes(int id)
    {
        try
        {
            var incremented = await _clientService.IncrementTotalCommandesAsync(id);
            if (!incremented)
            {
                return NotFound(new { message = $"Client avec l'ID {id} introuvable" });
            }
            
            var client = await _clientService.GetClientByIdAsync(id);
            return Ok(client);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de l'incrémentation du total des commandes du client {ClientId}", id);
            return StatusCode(500, new { message = "Erreur lors de l'incrémentation du total des commandes" });
        }
    }
}

