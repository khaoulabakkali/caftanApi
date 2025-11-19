using Microsoft.EntityFrameworkCore;
using mkBoutiqueCaftan.Data;
using mkBoutiqueCaftan.Models;

namespace mkBoutiqueCaftan.Services;

public interface IClientService
{
    Task<IEnumerable<ClientDto>> GetAllClientsAsync(bool includeInactive = false);
    Task<ClientDto?> GetClientByIdAsync(int id);
    Task<ClientDto> CreateClientAsync(CreateClientRequest request);
    Task<ClientDto?> UpdateClientAsync(int id, UpdateClientRequest request);
    Task<bool> DeleteClientAsync(int id);
    Task<bool> ToggleClientStatusAsync(int id);
    Task<bool> IncrementTotalCommandesAsync(int id);
}

public class ClientService : IClientService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ClientService> _logger;

    public ClientService(ApplicationDbContext context, ILogger<ClientService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ClientDto>> GetAllClientsAsync(bool includeInactive = false)
    {
        var query = _context.Clients.AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(c => c.Actif);
        }

        var clients = await query
            .OrderBy(c => c.NomClient)
            .ThenBy(c => c.PrenomClient)
            .ToListAsync();
        
        return clients.Select(MapToDto);
    }

    public async Task<ClientDto?> GetClientByIdAsync(int id)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.IdClient == id);
        return client == null ? null : MapToDto(client);
    }

    public async Task<ClientDto> CreateClientAsync(CreateClientRequest request)
    {
        // Vérifier si le téléphone existe déjà
        var telephoneExists = await _context.Clients
            .AnyAsync(c => c.Telephone == request.Telephone);
        
        if (telephoneExists)
        {
            throw new InvalidOperationException($"Un client avec le téléphone '{request.Telephone}' existe déjà.");
        }

        // Vérifier si l'email existe déjà (si fourni)
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailExists = await _context.Clients
                .AnyAsync(c => c.Email != null && c.Email.ToLower() == request.Email.ToLower());
            
            if (emailExists)
            {
                throw new InvalidOperationException($"Un client avec l'email '{request.Email}' existe déjà.");
            }
        }

        var client = new Client
        {
            NomClient = request.NomClient,
            PrenomClient = request.PrenomClient,
            Telephone = request.Telephone,
            Email = request.Email,
            AdressePrincipale = request.AdressePrincipale,
            TotalCommandes = 0,
            DateCreationFiche = DateTime.Now,
            Actif = request.Actif
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Client créé: {NomClient} {PrenomClient} (ID: {IdClient})", client.NomClient, client.PrenomClient, client.IdClient);
        return MapToDto(client);
    }

    public async Task<ClientDto?> UpdateClientAsync(int id, UpdateClientRequest request)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.IdClient == id);
        if (client == null)
        {
            return null;
        }

        // Vérifier si le téléphone existe déjà (si fourni)
        if (!string.IsNullOrWhiteSpace(request.Telephone))
        {
            var telephoneExists = await _context.Clients
                .AnyAsync(c => c.Telephone == request.Telephone && c.IdClient != id);
            
            if (telephoneExists)
            {
                throw new InvalidOperationException($"Un client avec le téléphone '{request.Telephone}' existe déjà.");
            }
        }

        // Vérifier si l'email existe déjà (si fourni)
        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var emailExists = await _context.Clients
                .AnyAsync(c => c.Email != null && c.Email.ToLower() == request.Email.ToLower() && c.IdClient != id);
            
            if (emailExists)
            {
                throw new InvalidOperationException($"Un client avec l'email '{request.Email}' existe déjà.");
            }
        }

        // Mettre à jour les propriétés
        if (!string.IsNullOrWhiteSpace(request.NomClient))
            client.NomClient = request.NomClient;

        if (!string.IsNullOrWhiteSpace(request.PrenomClient))
            client.PrenomClient = request.PrenomClient;

        if (!string.IsNullOrWhiteSpace(request.Telephone))
            client.Telephone = request.Telephone;

        if (request.Email != null)
            client.Email = request.Email;

        if (request.AdressePrincipale != null)
            client.AdressePrincipale = request.AdressePrincipale;

        if (request.TotalCommandes.HasValue)
            client.TotalCommandes = request.TotalCommandes.Value;

        if (request.Actif.HasValue)
            client.Actif = request.Actif.Value;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Client mis à jour: {NomClient} {PrenomClient} (ID: {IdClient})", client.NomClient, client.PrenomClient, client.IdClient);
        return MapToDto(client);
    }

    public async Task<bool> DeleteClientAsync(int id)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.IdClient == id);
        if (client == null)
        {
            return false;
        }

        // Vérifier si le client a des réservations
        var hasReservations = await _context.Reservations
            .AnyAsync(r => r.IdClient == id);
        
        if (hasReservations)
        {
            throw new InvalidOperationException("Impossible de supprimer un client qui a des réservations. Désactivez-le à la place.");
        }

        _context.Clients.Remove(client);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Client supprimé: {NomClient} {PrenomClient} (ID: {IdClient})", client.NomClient, client.PrenomClient, client.IdClient);
        return true;
    }

    public async Task<bool> ToggleClientStatusAsync(int id)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.IdClient == id);
        if (client == null)
        {
            return false;
        }

        client.Actif = !client.Actif;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Statut du client {IdClient} changé à: {Actif}", id, client.Actif);
        return true;
    }

    public async Task<bool> IncrementTotalCommandesAsync(int id)
    {
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.IdClient == id);
        if (client == null)
        {
            return false;
        }

        client.TotalCommandes++;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Total commandes du client {IdClient} incrémenté à: {TotalCommandes}", id, client.TotalCommandes);
        return true;
    }

    private static ClientDto MapToDto(Client client)
    {
        return new ClientDto
        {
            IdClient = client.IdClient,
            NomClient = client.NomClient,
            PrenomClient = client.PrenomClient,
            Telephone = client.Telephone,
            Email = client.Email,
            AdressePrincipale = client.AdressePrincipale,
            TotalCommandes = client.TotalCommandes,
            DateCreationFiche = client.DateCreationFiche,
            Actif = client.Actif
        };
    }
}

