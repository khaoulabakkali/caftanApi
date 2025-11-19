using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using mkBoutiqueCaftan.Models;
using mkBoutiqueCaftan.Services;

namespace mkBoutiqueCaftan.Controllers;

[ApiController]
[Route("api/reservations")]
[Authorize]
public class ReservationController : ControllerBase
{
    private readonly IReservationService _reservationService;
    private readonly ILogger<ReservationController> _logger;

    public ReservationController(IReservationService reservationService, ILogger<ReservationController> logger)
    {
        _reservationService = reservationService;
        _logger = logger;
    }

    /// <summary>
    /// Récupère toutes les réservations
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReservationDto>>> GetAllReservations(
        [FromQuery] int? idSociete = null,
        [FromQuery] StatutReservation? statut = null)
    {
        try
        {
            var reservations = await _reservationService.GetAllReservationsAsync(idSociete, statut);
            return Ok(reservations);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération des réservations");
            return StatusCode(500, new { message = "Erreur lors de la récupération des réservations" });
        }
    }

    /// <summary>
    /// Récupère une réservation par son ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ReservationDto>> GetReservationById(int id)
    {
        try
        {
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            if (reservation == null)
            {
                return NotFound(new { message = $"Réservation avec l'ID {id} introuvable" });
            }
            return Ok(reservation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la récupération de la réservation {ReservationId}", id);
            return StatusCode(500, new { message = "Erreur lors de la récupération de la réservation" });
        }
    }

    /// <summary>
    /// Crée une nouvelle réservation
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ReservationDto>> CreateReservation([FromBody] CreateReservationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.DateDebut >= request.DateFin)
        {
            return BadRequest(new { message = "La date de début doit être antérieure à la date de fin" });
        }

        if (request.MontantTotal < 0)
        {
            return BadRequest(new { message = "Le montant total ne peut pas être négatif" });
        }

        if (request.RemiseAppliquee < 0)
        {
            return BadRequest(new { message = "La remise appliquée ne peut pas être négative" });
        }

        try
        {
            var reservation = await _reservationService.CreateReservationAsync(request);
            return CreatedAtAction(nameof(GetReservationById), new { id = reservation.IdReservation }, reservation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la création de la réservation");
            return StatusCode(500, new { message = "Erreur lors de la création de la réservation" });
        }
    }

    /// <summary>
    /// Met à jour une réservation existante
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ReservationDto>> UpdateReservation(int id, [FromBody] UpdateReservationRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (request.DateDebut.HasValue && request.DateFin.HasValue && request.DateDebut.Value >= request.DateFin.Value)
        {
            return BadRequest(new { message = "La date de début doit être antérieure à la date de fin" });
        }

        if (request.MontantTotal.HasValue && request.MontantTotal.Value < 0)
        {
            return BadRequest(new { message = "Le montant total ne peut pas être négatif" });
        }

        if (request.RemiseAppliquee.HasValue && request.RemiseAppliquee.Value < 0)
        {
            return BadRequest(new { message = "La remise appliquée ne peut pas être négative" });
        }

        try
        {
            var reservation = await _reservationService.UpdateReservationAsync(id, request);
            if (reservation == null)
            {
                return NotFound(new { message = $"Réservation avec l'ID {id} introuvable" });
            }
            return Ok(reservation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour de la réservation {ReservationId}", id);
            return StatusCode(500, new { message = "Erreur lors de la mise à jour de la réservation" });
        }
    }

    /// <summary>
    /// Supprime une réservation
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteReservation(int id)
    {
        try
        {
            var deleted = await _reservationService.DeleteReservationAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = $"Réservation avec l'ID {id} introuvable" });
            }
            return Ok(new { message = "Réservation supprimée avec succès" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la suppression de la réservation {ReservationId}", id);
            return StatusCode(500, new { message = "Erreur lors de la suppression de la réservation" });
        }
    }

    /// <summary>
    /// Met à jour le statut d'une réservation
    /// </summary>
    [HttpPatch("{id}/statut")]
    public async Task<ActionResult<ReservationDto>> UpdateReservationStatus(int id, [FromBody] StatutReservation statut)
    {
        try
        {
            var updated = await _reservationService.UpdateReservationStatusAsync(id, statut);
            if (!updated)
            {
                return NotFound(new { message = $"Réservation avec l'ID {id} introuvable" });
            }
            
            var reservation = await _reservationService.GetReservationByIdAsync(id);
            return Ok(reservation);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erreur lors de la mise à jour du statut de la réservation {ReservationId}", id);
            return StatusCode(500, new { message = "Erreur lors de la mise à jour du statut" });
        }
    }
}

