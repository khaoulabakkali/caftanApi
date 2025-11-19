using Microsoft.EntityFrameworkCore;
using mkBoutiqueCaftan.Data;
using mkBoutiqueCaftan.Models;

namespace mkBoutiqueCaftan.Services;

public interface IReservationService
{
    Task<IEnumerable<ReservationDto>> GetAllReservationsAsync(int? idSociete = null, StatutReservation? statut = null);
    Task<ReservationDto?> GetReservationByIdAsync(int id);
    Task<ReservationDto> CreateReservationAsync(CreateReservationRequest request);
    Task<ReservationDto?> UpdateReservationAsync(int id, UpdateReservationRequest request);
    Task<bool> DeleteReservationAsync(int id);
    Task<bool> UpdateReservationStatusAsync(int id, StatutReservation statut);
}

public class ReservationService : IReservationService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(ApplicationDbContext context, ILogger<ReservationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ReservationDto>> GetAllReservationsAsync(int? idSociete = null, StatutReservation? statut = null)
    {
        var query = _context.Reservations
            .Include(r => r.Client)
            .Include(r => r.Paiement)
            .AsQueryable();

        if (idSociete.HasValue)
        {
            query = query.Where(r => r.IdSociete == idSociete.Value);
        }

        if (statut.HasValue)
        {
            query = query.Where(r => r.StatutReservation == statut.Value);
        }

        var reservations = await query
            .OrderByDescending(r => r.DateReservation)
            .ToListAsync();
        
        return reservations.Select(MapToDto);
    }

    public async Task<ReservationDto?> GetReservationByIdAsync(int id)
    {
        var currentIdSociete = _userContextService.GetIdSociete();
        if (!currentIdSociete.HasValue)
        {
            throw new UnauthorizedAccessException("IdSociete non trouvé dans le token. Veuillez vous reconnecter.");
        }

        var reservation = await _context.Reservations
            .Include(r => r.Client)
            .Include(r => r.Paiement)
            .FirstOrDefaultAsync(r => r.IdReservation == id && r.IdSociete == currentIdSociete.Value);
        
        return reservation == null ? null : MapToDto(reservation);
    }

    public async Task<ReservationDto> CreateReservationAsync(CreateReservationRequest request)
    {
        var currentIdSociete = _userContextService.GetIdSociete();
        if (!currentIdSociete.HasValue)
        {
            throw new UnauthorizedAccessException("IdSociete non trouvé dans le token. Veuillez vous reconnecter.");
        }

        // Vérifier si le client existe
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.IdClient == request.IdClient && c.IdSociete == currentIdSociete.Value);
        
        if (client == null)
        {
            throw new InvalidOperationException($"Le client avec l'ID {request.IdClient} n'existe pas pour cette société.");
        }

        // Vérifier si le paiement existe (si fourni)
        if (request.IdPaiement.HasValue)
        {
            var paiement = await _context.Paiements
                .FirstOrDefaultAsync(p => p.IdPaiement == request.IdPaiement.Value && p.IdSociete == currentIdSociete.Value);
            
            if (paiement == null)
            {
                throw new InvalidOperationException($"Le paiement avec l'ID {request.IdPaiement} n'existe pas pour cette société.");
            }
        }

        // Valider les dates
        if (request.DateDebut >= request.DateFin)
        {
            throw new InvalidOperationException("La date de début doit être antérieure à la date de fin.");
        }

        if (request.MontantTotal < 0)
        {
            throw new InvalidOperationException("Le montant total ne peut pas être négatif.");
        }

        if (request.RemiseAppliquee < 0)
        {
            throw new InvalidOperationException("La remise appliquée ne peut pas être négative.");
        }

        var reservation = new Reservation
        {
            IdClient = request.IdClient,
            DateReservation = DateTime.Now,
            DateDebut = request.DateDebut,
            DateFin = request.DateFin,
            MontantTotal = request.MontantTotal,
            StatutReservation = request.StatutReservation,
            IdPaiement = request.IdPaiement,
            RemiseAppliquee = request.RemiseAppliquee,
            IdSociete = currentIdSociete.Value
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Réservation créée: ID {IdReservation} pour le client {IdClient}", reservation.IdReservation, reservation.IdClient);

        // Recharger avec les relations
        return await GetReservationByIdAsync(reservation.IdReservation) ?? MapToDto(reservation);
    }

    public async Task<ReservationDto?> UpdateReservationAsync(int id, UpdateReservationRequest request)
    {
        var currentIdSociete = _userContextService.GetIdSociete();
        if (!currentIdSociete.HasValue)
        {
            throw new UnauthorizedAccessException("IdSociete non trouvé dans le token. Veuillez vous reconnecter.");
        }

        var reservation = await _context.Reservations
            .Include(r => r.Client)
            .Include(r => r.Paiement)
            .FirstOrDefaultAsync(r => r.IdReservation == id && r.IdSociete == currentIdSociete.Value);
        
        if (reservation == null)
        {
            return null;
        }

        // Vérifier si le client existe (si fourni)
        if (request.IdClient.HasValue)
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.IdClient == request.IdClient.Value && c.IdSociete == currentIdSociete.Value);
            
            if (client == null)
            {
                throw new InvalidOperationException($"Le client avec l'ID {request.IdClient} n'existe pas pour cette société.");
            }
        }

        // Vérifier si le paiement existe (si fourni)
        if (request.IdPaiement.HasValue)
        {
            var paiement = await _context.Paiements
                .FirstOrDefaultAsync(p => p.IdPaiement == request.IdPaiement.Value && p.IdSociete == currentIdSociete.Value);
            
            if (paiement == null)
            {
                throw new InvalidOperationException($"Le paiement avec l'ID {request.IdPaiement} n'existe pas pour cette société.");
            }
        }

        // Valider les dates
        var dateDebut = request.DateDebut ?? reservation.DateDebut;
        var dateFin = request.DateFin ?? reservation.DateFin;
        
        if (dateDebut >= dateFin)
        {
            throw new InvalidOperationException("La date de début doit être antérieure à la date de fin.");
        }

        // Mettre à jour les propriétés
        if (request.IdClient.HasValue)
            reservation.IdClient = request.IdClient.Value;

        if (request.DateDebut.HasValue)
            reservation.DateDebut = request.DateDebut.Value;

        if (request.DateFin.HasValue)
            reservation.DateFin = request.DateFin.Value;

        if (request.MontantTotal.HasValue)
        {
            if (request.MontantTotal.Value < 0)
            {
                throw new InvalidOperationException("Le montant total ne peut pas être négatif.");
            }
            reservation.MontantTotal = request.MontantTotal.Value;
        }

        if (request.StatutReservation.HasValue)
            reservation.StatutReservation = request.StatutReservation.Value;

        if (request.IdPaiement.HasValue)
            reservation.IdPaiement = request.IdPaiement.Value;
        else if (request.IdPaiement == null && request.IdPaiement != reservation.IdPaiement)
            reservation.IdPaiement = null;

        if (request.RemiseAppliquee.HasValue)
        {
            if (request.RemiseAppliquee.Value < 0)
            {
                throw new InvalidOperationException("La remise appliquée ne peut pas être négative.");
            }
            reservation.RemiseAppliquee = request.RemiseAppliquee.Value;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Réservation mise à jour: ID {IdReservation}", reservation.IdReservation);

        // Recharger avec les relations
        return await GetReservationByIdAsync(reservation.IdReservation) ?? MapToDto(reservation);
    }

    public async Task<bool> DeleteReservationAsync(int id)
    {
        var currentIdSociete = _userContextService.GetIdSociete();
        if (!currentIdSociete.HasValue)
        {
            throw new UnauthorizedAccessException("IdSociete non trouvé dans le token. Veuillez vous reconnecter.");
        }

        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.IdReservation == id && r.IdSociete == currentIdSociete.Value);
        if (reservation == null)
        {
            return false;
        }

        _context.Reservations.Remove(reservation);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Réservation supprimée: ID {IdReservation}", reservation.IdReservation);
        return true;
    }

    public async Task<bool> UpdateReservationStatusAsync(int id, StatutReservation statut)
    {
        var currentIdSociete = _userContextService.GetIdSociete();
        if (!currentIdSociete.HasValue)
        {
            throw new UnauthorizedAccessException("IdSociete non trouvé dans le token. Veuillez vous reconnecter.");
        }

        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.IdReservation == id && r.IdSociete == currentIdSociete.Value);
        if (reservation == null)
        {
            return false;
        }

        reservation.StatutReservation = statut;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Statut de la réservation {IdReservation} changé à: {Statut}", id, statut);
        return true;
    }

    private static ReservationDto MapToDto(Reservation reservation)
    {
        return new ReservationDto
        {
            IdReservation = reservation.IdReservation,
            IdClient = reservation.IdClient,
            DateReservation = reservation.DateReservation,
            DateDebut = reservation.DateDebut,
            DateFin = reservation.DateFin,
            MontantTotal = reservation.MontantTotal,
            StatutReservation = reservation.StatutReservation,
            IdPaiement = reservation.IdPaiement,
            RemiseAppliquee = reservation.RemiseAppliquee,
            IdSociete = reservation.IdSociete,
            Client = reservation.Client != null ? new ClientDto
            {
                IdClient = reservation.Client.IdClient,
                NomClient = reservation.Client.NomClient,
                PrenomClient = reservation.Client.PrenomClient,
                Telephone = reservation.Client.Telephone,
                Email = reservation.Client.Email,
                AdressePrincipale = reservation.Client.AdressePrincipale,
                TotalCommandes = reservation.Client.TotalCommandes,
                DateCreationFiche = reservation.Client.DateCreationFiche,
                IdSociete = reservation.Client.IdSociete,
                Actif = reservation.Client.Actif
            } : null,
            Paiement = reservation.Paiement != null ? new PaiementDto
            {
                IdPaiement = reservation.Paiement.IdPaiement,
                IdReservation = reservation.Paiement.IdReservation,
                Montant = reservation.Paiement.Montant,
                DatePaiement = reservation.Paiement.DatePaiement,
                MethodePaiement = reservation.Paiement.MethodePaiement,
                Reference = reservation.Paiement.Reference,
                IdSociete = reservation.Paiement.IdSociete
            } : null
        };
    }
}

