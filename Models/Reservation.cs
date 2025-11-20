using System.Text.Json.Serialization;

namespace mkBoutiqueCaftan.Models;

public class Reservation
{
    public int IdReservation { get; set; }
    public int IdClient { get; set; }
    public DateTime DateReservation { get; set; } = DateTime.Now;
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public decimal MontantTotal { get; set; }
    public StatutReservation StatutReservation { get; set; } = StatutReservation.EnAttente;
    public int? IdPaiement { get; set; }
    public decimal RemiseAppliquee { get; set; } = 0.00m;
    public int IdSociete { get; set; }
    
    // Navigation properties
    public Client? Client { get; set; }
    public Societe? Societe { get; set; }
    
    [JsonIgnore]
    public Paiement? Paiement { get; set; }
}

public class ReservationDto
{
    public int IdReservation { get; set; }
    public int IdClient { get; set; }
    public DateTime DateReservation { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public decimal MontantTotal { get; set; }
    public StatutReservation StatutReservation { get; set; }
    public int? IdPaiement { get; set; }
    public decimal RemiseAppliquee { get; set; }
    public int IdSociete { get; set; }
    public ClientDto? Client { get; set; }
    public PaiementDto? Paiement { get; set; }
}

public class CreateReservationRequest
{
    public int IdClient { get; set; }
    public DateTime DateDebut { get; set; }
    public DateTime DateFin { get; set; }
    public decimal MontantTotal { get; set; }
    public StatutReservation StatutReservation { get; set; } = StatutReservation.EnAttente;
    public int? IdPaiement { get; set; }
    public decimal RemiseAppliquee { get; set; } = 0.00m;
}

public class UpdateReservationRequest
{
    public int? IdClient { get; set; }
    public DateTime? DateDebut { get; set; }
    public DateTime? DateFin { get; set; }
    public decimal? MontantTotal { get; set; }
    public StatutReservation? StatutReservation { get; set; }
    public int? IdPaiement { get; set; }
    public decimal? RemiseAppliquee { get; set; }
}

