using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using mkBoutiqueCaftan.Data;
using mkBoutiqueCaftan.Models;

namespace mkBoutiqueCaftan.Services;

public interface IPaiementService
{
    Task<IEnumerable<PaiementDto>> GetAllPaiementsAsync(int? idReservation = null);
    Task<PaiementDto?> GetPaiementByIdAsync(int id);
    Task<PaiementDto> CreatePaiementAsync(CreatePaiementRequest request);
    Task<PaiementDto?> UpdatePaiementAsync(int id, UpdatePaiementRequest request);
    Task<bool> DeletePaiementAsync(int id);
}

public class PaiementService : IPaiementService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaiementService> _logger;

    public PaiementService(ApplicationDbContext context, ILogger<PaiementService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<PaiementDto>> GetAllPaiementsAsync(int? idReservation = null)
    {
        var query = _context.Paiements
            .Include(p => p.Reservation)
            .AsQueryable();

        if (idReservation.HasValue)
        {
            query = query.Where(p => p.IdReservation == idReservation.Value);
        }

        var paiements = await query
            .OrderByDescending(p => p.DatePaiement)
            .ToListAsync();
        
        return paiements.Select(MapToDto);
    }

    public async Task<PaiementDto?> GetPaiementByIdAsync(int id)
    {
        var paiement = await _context.Paiements
            .Include(p => p.Reservation)
            .FirstOrDefaultAsync(p => p.IdPaiement == id);
        
        return paiement == null ? null : MapToDto(paiement);
    }

    public async Task<PaiementDto> CreatePaiementAsync(CreatePaiementRequest request)
    {
        // Vérifier si la réservation existe
        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.IdReservation == request.IdReservation);
        
        if (reservation == null)
        {
            throw new InvalidOperationException($"La réservation avec l'ID {request.IdReservation} n'existe pas.");
        }

        // Vérifier si un paiement existe déjà pour cette réservation
        var existingPaiement = await _context.Paiements
            .FirstOrDefaultAsync(p => p.IdReservation == request.IdReservation);
        
        if (existingPaiement != null)
        {
            throw new InvalidOperationException($"Un paiement existe déjà pour la réservation {request.IdReservation}.");
        }

        if (request.Montant <= 0)
        {
            throw new InvalidOperationException("Le montant du paiement doit être supérieur à zéro.");
        }

        if (!string.IsNullOrWhiteSpace(request.MethodePaiement) && request.MethodePaiement.Length > 50)
        {
            throw new InvalidOperationException("La méthode de paiement ne peut pas dépasser 50 caractères.");
        }

        if (!string.IsNullOrWhiteSpace(request.Reference) && request.Reference.Length > 100)
        {
            throw new InvalidOperationException("La référence ne peut pas dépasser 100 caractères.");
        }

        var paiement = new Paiement
        {
            IdReservation = request.IdReservation,
            Montant = request.Montant,
            DatePaiement = DateTime.Now,
            MethodePaiement = request.MethodePaiement,
            Reference = request.Reference
        };

        _context.Paiements.Add(paiement);
        await _context.SaveChangesAsync();

        // Mettre à jour la réservation avec l'ID du paiement
        reservation.IdPaiement = paiement.IdPaiement;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Paiement créé: ID {IdPaiement} pour la réservation {IdReservation}", paiement.IdPaiement, paiement.IdReservation);
        return MapToDto(paiement);
    }

    public async Task<PaiementDto?> UpdatePaiementAsync(int id, UpdatePaiementRequest request)
    {
        var paiement = await _context.Paiements
            .FirstOrDefaultAsync(p => p.IdPaiement == id);
        
        if (paiement == null)
        {
            return null;
        }

        // Vérifier si une nouvelle réservation est fournie et qu'elle existe
        if (request.IdReservation.HasValue)
        {
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(r => r.IdReservation == request.IdReservation.Value);
            
            if (reservation == null)
            {
                throw new InvalidOperationException($"La réservation avec l'ID {request.IdReservation} n'existe pas.");
            }

            // Vérifier si un autre paiement existe déjà pour cette nouvelle réservation
            var existingPaiement = await _context.Paiements
                .FirstOrDefaultAsync(p => p.IdReservation == request.IdReservation.Value && p.IdPaiement != id);
            
            if (existingPaiement != null)
            {
                throw new InvalidOperationException($"Un paiement existe déjà pour la réservation {request.IdReservation}.");
            }

            // Mettre à jour l'ancienne réservation pour supprimer la référence au paiement
            var oldReservation = await _context.Reservations.FindAsync(paiement.IdReservation);
            if (oldReservation != null && oldReservation.IdPaiement == id)
            {
                oldReservation.IdPaiement = null;
            }

            paiement.IdReservation = request.IdReservation.Value;

            // Mettre à jour la nouvelle réservation avec l'ID du paiement
            reservation.IdPaiement = id;
        }

        if (request.Montant.HasValue)
        {
            if (request.Montant.Value <= 0)
            {
                throw new InvalidOperationException("Le montant du paiement doit être supérieur à zéro.");
            }
            paiement.Montant = request.Montant.Value;
        }

        if (request.MethodePaiement != null)
        {
            if (request.MethodePaiement.Length > 50)
            {
                throw new InvalidOperationException("La méthode de paiement ne peut pas dépasser 50 caractères.");
            }
            paiement.MethodePaiement = request.MethodePaiement;
        }

        if (request.Reference != null)
        {
            if (request.Reference.Length > 100)
            {
                throw new InvalidOperationException("La référence ne peut pas dépasser 100 caractères.");
            }
            paiement.Reference = request.Reference;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Paiement mis à jour: ID {IdPaiement}", id);
        return MapToDto(paiement);
    }

    public async Task<bool> DeletePaiementAsync(int id)
    {
        var paiement = await _context.Paiements
            .FirstOrDefaultAsync(p => p.IdPaiement == id);
        
        if (paiement == null)
        {
            return false;
        }

        // Supprimer la référence au paiement dans la réservation
        var reservation = await _context.Reservations.FindAsync(paiement.IdReservation);
        if (reservation != null && reservation.IdPaiement == id)
        {
            reservation.IdPaiement = null;
        }

        _context.Paiements.Remove(paiement);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Paiement supprimé: ID {IdPaiement}", id);
        return true;
    }

    private static PaiementDto MapToDto(Paiement paiement)
    {
        return new PaiementDto
        {
            IdPaiement = paiement.IdPaiement,
            IdReservation = paiement.IdReservation,
            Montant = paiement.Montant,
            DatePaiement = paiement.DatePaiement,
            MethodePaiement = paiement.MethodePaiement,
            Reference = paiement.Reference
        };
    }
}

