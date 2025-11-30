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
    private readonly IUserContextService _userContextService;

    public ReservationService(ApplicationDbContext context, ILogger<ReservationService> logger, IUserContextService userContextService)
    {
        _context = context;
        _logger = logger;
        _userContextService = userContextService;
    }

    public async Task<IEnumerable<ReservationDto>> GetAllReservationsAsync(int? idSociete = null, StatutReservation? statut = null)
    {
        var idSocieteFromToken = _userContextService.GetIdSociete();
        if (!idSocieteFromToken.HasValue)
        {
            _logger.LogWarning("Tentative de récupération des réservations sans IdSociete dans le token");
            return new List<ReservationDto>();
        }

        var query = _context.Reservations
            .Include(r => r.Client)
            .Include(r => r.Article)
            .Include(r => r.ReservationArticles)
                .ThenInclude(ra => ra.Article)
            .Include(r => r.Paiement)
            .Where(r => r.IdSociete == idSocieteFromToken.Value);

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
        var idSociete = _userContextService.GetIdSociete();
        if (!idSociete.HasValue)
        {
            _logger.LogWarning("Tentative de récupération d'une réservation sans IdSociete dans le token");
            return null;
        }

        var reservation = await _context.Reservations
            .Include(r => r.Client)
            .Include(r => r.Article)
            .Include(r => r.ReservationArticles)
                .ThenInclude(ra => ra.Article)
            .Include(r => r.Paiement)
            .FirstOrDefaultAsync(r => r.IdReservation == id && r.IdSociete == idSociete.Value);
        
        return reservation == null ? null : MapToDto(reservation);
    }

    public async Task<ReservationDto> CreateReservationAsync(CreateReservationRequest request)
    {
        var idSociete = _userContextService.GetIdSociete();
        if (!idSociete.HasValue)
        {
            _logger.LogWarning("Tentative de création de réservation sans IdSociete dans le token");
            throw new UnauthorizedAccessException("IdSociete manquant dans le token");
        }

        // Vérifier si le client existe pour cette société
        var client = await _context.Clients
            .FirstOrDefaultAsync(c => c.IdClient == request.IdClient && c.IdSociete == idSociete.Value);
        
        if (client == null)
        {
            throw new InvalidOperationException($"Le client avec l'ID {request.IdClient} n'existe pas pour cette société.");
        }

        // Vérifier si le paiement existe (si fourni) pour cette société
        if (request.IdPaiement.HasValue)
        {
            var paiement = await _context.Paiements
                .FirstOrDefaultAsync(p => p.IdPaiement == request.IdPaiement.Value && p.IdSociete == idSociete.Value);
            
            if (paiement == null)
            {
                throw new InvalidOperationException($"Le paiement avec l'ID {request.IdPaiement} n'existe pas pour cette société.");
            }
        }

        // Vérifier si l'article existe (si fourni) pour cette société (ancien champ pour compatibilité)
        if (request.IdArticle.HasValue)
        {
            var article = await _context.Articles
                .FirstOrDefaultAsync(a => a.IdArticle == request.IdArticle.Value && a.IdSociete == idSociete.Value);
            
            if (article == null)
            {
                throw new InvalidOperationException($"L'article avec l'ID {request.IdArticle} n'existe pas pour cette société.");
            }
        }

        // Valider les articles fournis dans la liste
        if (request.Articles != null && request.Articles.Any())
        {
            var articleIds = request.Articles.Select(a => a.IdArticle).ToList();
            var articles = await _context.Articles
                .Where(a => articleIds.Contains(a.IdArticle) && a.IdSociete == idSociete.Value)
                .ToListAsync();

            if (articles.Count != articleIds.Count)
            {
                var foundIds = articles.Select(a => a.IdArticle).ToList();
                var missingIds = articleIds.Except(foundIds).ToList();
                throw new InvalidOperationException($"Les articles avec les IDs {string.Join(", ", missingIds)} n'existent pas pour cette société.");
            }

            // Valider les quantités
            foreach (var articleItem in request.Articles)
            {
                if (articleItem.Quantite <= 0)
                {
                    throw new InvalidOperationException($"La quantité pour l'article {articleItem.IdArticle} doit être supérieure à 0.");
                }
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
            IdArticle = request.IdArticle, // Pour compatibilité
            RemiseAppliquee = request.RemiseAppliquee,
            IdSociete = idSociete.Value,
            PhotoCIN = request.PhotoCIN
        };

        _context.Reservations.Add(reservation);
        await _context.SaveChangesAsync();

        // Ajouter les articles à la réservation
        if (request.Articles != null && request.Articles.Any())
        {
            var reservationArticles = request.Articles.Select(a => new ReservationArticle
            {
                IdReservation = reservation.IdReservation,
                IdArticle = a.IdArticle,
                Quantite = a.Quantite
            }).ToList();

            _context.ReservationArticles.AddRange(reservationArticles);
            await _context.SaveChangesAsync();
        }

        _logger.LogInformation("Réservation créée: ID {IdReservation} pour le client {IdClient}", reservation.IdReservation, reservation.IdClient);

        // Recharger avec les relations
        return await GetReservationByIdAsync(reservation.IdReservation) ?? MapToDto(reservation);
    }

    public async Task<ReservationDto?> UpdateReservationAsync(int id, UpdateReservationRequest request)
    {
        var idSociete = _userContextService.GetIdSociete();
        if (!idSociete.HasValue)
        {
            _logger.LogWarning("Tentative de mise à jour de réservation sans IdSociete dans le token");
            return null;
        }

        var reservation = await _context.Reservations
            .Include(r => r.Client)
            .Include(r => r.Article)
            .Include(r => r.Paiement)
            .FirstOrDefaultAsync(r => r.IdReservation == id && r.IdSociete == idSociete.Value);
        
        if (reservation == null)
        {
            return null;
        }

        // Vérifier si le client existe (si fourni) pour cette société
        if (request.IdClient.HasValue)
        {
            var client = await _context.Clients
                .FirstOrDefaultAsync(c => c.IdClient == request.IdClient.Value && c.IdSociete == idSociete.Value);
            
            if (client == null)
            {
                throw new InvalidOperationException($"Le client avec l'ID {request.IdClient} n'existe pas pour cette société.");
            }
        }

        // Vérifier si le paiement existe (si fourni) pour cette société
        if (request.IdPaiement.HasValue)
        {
            var paiement = await _context.Paiements
                .FirstOrDefaultAsync(p => p.IdPaiement == request.IdPaiement.Value && p.IdSociete == idSociete.Value);
            
            if (paiement == null)
            {
                throw new InvalidOperationException($"Le paiement avec l'ID {request.IdPaiement} n'existe pas pour cette société.");
            }
        }

        // Vérifier si l'article existe (si fourni) pour cette société (ancien champ pour compatibilité)
        if (request.IdArticle.HasValue)
        {
            var article = await _context.Articles
                .FirstOrDefaultAsync(a => a.IdArticle == request.IdArticle.Value && a.IdSociete == idSociete.Value);
            
            if (article == null)
            {
                throw new InvalidOperationException($"L'article avec l'ID {request.IdArticle} n'existe pas pour cette société.");
            }
        }

        // Valider les articles fournis dans la liste
        if (request.Articles != null && request.Articles.Any())
        {
            var articleIds = request.Articles.Select(a => a.IdArticle).ToList();
            var articles = await _context.Articles
                .Where(a => articleIds.Contains(a.IdArticle) && a.IdSociete == idSociete.Value)
                .ToListAsync();

            if (articles.Count != articleIds.Count)
            {
                var foundIds = articles.Select(a => a.IdArticle).ToList();
                var missingIds = articleIds.Except(foundIds).ToList();
                throw new InvalidOperationException($"Les articles avec les IDs {string.Join(", ", missingIds)} n'existent pas pour cette société.");
            }

            // Valider les quantités
            foreach (var articleItem in request.Articles)
            {
                if (articleItem.Quantite <= 0)
                {
                    throw new InvalidOperationException($"La quantité pour l'article {articleItem.IdArticle} doit être supérieure à 0.");
                }
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

        if (request.IdArticle.HasValue)
            reservation.IdArticle = request.IdArticle.Value;
        else if (request.IdArticle == null && request.IdArticle != reservation.IdArticle)
            reservation.IdArticle = null;

        // Mettre à jour les articles si fournis
        if (request.Articles != null)
        {
            // Supprimer les anciens articles de la réservation
            var existingReservationArticles = await _context.ReservationArticles
                .Where(ra => ra.IdReservation == id)
                .ToListAsync();
            
            _context.ReservationArticles.RemoveRange(existingReservationArticles);

            // Ajouter les nouveaux articles
            if (request.Articles.Any())
            {
                var reservationArticles = request.Articles.Select(a => new ReservationArticle
                {
                    IdReservation = reservation.IdReservation,
                    IdArticle = a.IdArticle,
                    Quantite = a.Quantite
                }).ToList();

                _context.ReservationArticles.AddRange(reservationArticles);
            }
        }

        if (request.RemiseAppliquee.HasValue)
        {
            if (request.RemiseAppliquee.Value < 0)
            {
                throw new InvalidOperationException("La remise appliquée ne peut pas être négative.");
            }
            reservation.RemiseAppliquee = request.RemiseAppliquee.Value;
        }

        // Mettre à jour PhotoCIN si fourni (peut être une chaîne vide pour supprimer)
        // Note: Pour les strings nullable, on ne peut pas distinguer "non fourni" de "fourni mais null"
        // Donc on met à jour uniquement si une valeur est fournie (même si vide)
        // Pour supprimer, envoyer une chaîne vide "" ou null explicitement
        if (request.PhotoCIN != null)
        {
            reservation.PhotoCIN = string.IsNullOrWhiteSpace(request.PhotoCIN) ? null : request.PhotoCIN;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Réservation mise à jour: ID {IdReservation}", reservation.IdReservation);

        // Recharger avec les relations
        return await GetReservationByIdAsync(reservation.IdReservation) ?? MapToDto(reservation);
    }

    public async Task<bool> DeleteReservationAsync(int id)
    {
        var idSociete = _userContextService.GetIdSociete();
        if (!idSociete.HasValue)
        {
            _logger.LogWarning("Tentative de suppression de réservation sans IdSociete dans le token");
            return false;
        }

        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.IdReservation == id && r.IdSociete == idSociete.Value);
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
        var idSociete = _userContextService.GetIdSociete();
        if (!idSociete.HasValue)
        {
            _logger.LogWarning("Tentative de changement de statut de réservation sans IdSociete dans le token");
            return false;
        }

        var reservation = await _context.Reservations
            .FirstOrDefaultAsync(r => r.IdReservation == id && r.IdSociete == idSociete.Value);
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
            IdArticle = reservation.IdArticle,
            RemiseAppliquee = reservation.RemiseAppliquee,
            IdSociete = reservation.IdSociete,
            PhotoCIN = reservation.PhotoCIN,
            Client = reservation.Client != null ? new ClientDto
            {
                IdClient = reservation.Client.IdClient,
                NomClient = reservation.Client.NomClient,
                PrenomClient = reservation.Client.PrenomClient,
                Telephone = reservation.Client.Telephone,
                Email = reservation.Client.Email,
                AdressePrincipale = reservation.Client.AdressePrincipale,
                IdSociete = reservation.Client.IdSociete,
                TotalCommandes = reservation.Client.TotalCommandes,
                DateCreationFiche = reservation.Client.DateCreationFiche,
                Actif = reservation.Client.Actif,
                PhotoCIN = reservation.Client.PhotoCIN
            } : null,
            Article = reservation.Article != null ? new ArticleDto
            {
                IdArticle = reservation.Article.IdArticle,
                NomArticle = reservation.Article.NomArticle,
                Description = reservation.Article.Description,
                PrixLocationBase = reservation.Article.PrixLocationBase,
                PrixAvanceBase = reservation.Article.PrixAvanceBase,
                IdTaille = reservation.Article.IdTaille,
                Couleur = reservation.Article.Couleur,
                Photo = reservation.Article.Photo,
                IdCategorie = reservation.Article.IdCategorie,
                IdSociete = reservation.Article.IdSociete,
                Actif = reservation.Article.Actif
            } : null,
            Articles = reservation.ReservationArticles != null && reservation.ReservationArticles.Any()
                ? reservation.ReservationArticles
                    .Where(ra => ra.Article != null)
                    .Select(ra => new ArticleReservationDto
                    {
                        Article = new ArticleDto
                        {
                            IdArticle = ra.Article.IdArticle,
                            NomArticle = ra.Article.NomArticle,
                            Description = ra.Article.Description,
                            PrixLocationBase = ra.Article.PrixLocationBase,
                            PrixAvanceBase = ra.Article.PrixAvanceBase,
                            IdTaille = ra.Article.IdTaille,
                            Couleur = ra.Article.Couleur,
                            Photo = ra.Article.Photo,
                            IdCategorie = ra.Article.IdCategorie,
                            IdSociete = ra.Article.IdSociete,
                            Actif = ra.Article.Actif
                        },
                        Quantite = ra.Quantite
                    }).ToList()
                : new List<ArticleReservationDto>(),
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

